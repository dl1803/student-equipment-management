using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class GuiYeuCauMuonHandler(
    DbContext db,
    IClock dongHo,
    IDanhMucReader danhMuc,
    IQuyDinhReader quyDinh,
    IMuonTraReader muonTra,
    IAvailabilityService khaDung)
{
    public async Task<PhieuYeuCauMuonDto> XuLyAsync(
        string maSv,
        Guid maPhong,
        int thoiGianMuonDuKienGio,
        IReadOnlyList<LoaiVaSoLuongYeuCau> dsLoaiVaSoLuong,
        CancellationToken ct)
    {
        var thoiDiem = dongHo.UtcNow.ToUniversalTime();

        if (await danhMuc.LayPhongAsync(maPhong, ct) is null)
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Phòng thiết bị không tồn tại.");
        }

        foreach (var dong in dsLoaiVaSoLuong)
        {
            var loai = await danhMuc.LayLoaiAsync(dong.MaLoai, ct);
            if (loai is null || loai.NgungSuDung)
            {
                throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Loại thiết bị không tồn tại hoặc đã ngừng sử dụng.");
            }
        }

        var hoSo = await db.Database.SqlQuery<DongHoSoSinhVien>($"""
                SELECT ma_nhom AS "MaNhom", han_the AS "HanThe", trang_thai_hoc AS "TrangThaiHoc",
                       trang_thai_quyen_muon AS "TrangThaiQuyenMuon"
                FROM sinh_vien.sinh_vien
                WHERE ma_sv = {maSv}
                """)
            .SingleOrDefaultAsync(ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongDuQuyen, "Không tìm thấy hồ sơ sinh viên.");

        KiemTraDieuKienGuiYeuCau.KiemTraSinhVien(hoSo, VnTime.NgayLich(thoiDiem));

        var quyDinhNhom = await quyDinh.LayQuyDinhNhomAsync(hoSo.MaNhom, ct)
            ?? throw new InvalidOperationException($"Thiếu quy định mượn chung của nhóm {hoSo.MaNhom}.");

        KiemTraDieuKienGuiYeuCau.KiemTraThoiGianMuon(
            quyDinhNhom,
            dsLoaiVaSoLuong.Select(d => d.MaLoai),
            thoiGianMuonDuKienGio);

        var soDangCam = await muonTra.DemThietBiDangCamAsync(maSv, ct);
        var tongSoLuongYeuCau = dsLoaiVaSoLuong.Sum(d => (long)d.SoLuong);
        KiemTraDieuKienGuiYeuCau.KiemTraGioiHanThietBi(quyDinhNhom, soDangCam, tongSoLuongYeuCau);

        foreach (var dong in dsLoaiVaSoLuong.OrderBy(d => d.MaLoai))
        {
            if (await khaDung.TinhKhaDungAsync(maPhong, dong.MaLoai, ct) < dong.SoLuong)
            {
                throw new LoiNghiepVuException(
                    MaLoi.SoLuongKhaDungThayDoi,
                    "Số lượng khả dụng đã thay đổi, không còn đủ. Vui lòng chọn lại.");
            }
        }

        var moGiaoDich = db.Database.CurrentTransaction is null;
        var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync(ct) : null;
        string maYeuCau;
        try
        {
            maYeuCau = await ChenPhieuAsync(maSv, maPhong, thoiGianMuonDuKienGio, thoiDiem, ct);

            foreach (var dong in dsLoaiVaSoLuong)
            {
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO muon_tra.chi_tiet_yeu_cau_muon (ma_yeu_cau, ma_loai, so_luong)
                    VALUES ({maYeuCau}, {dong.MaLoai}, {dong.SoLuong})
                    """, ct);
            }

            if (giaoDich is not null)
            {
                await giaoDich.CommitAsync(ct);
            }
        }
        finally
        {
            if (giaoDich is not null)
            {
                await giaoDich.DisposeAsync();
            }
        }

        return await PhieuYeuCauMuonTruyVan.LayTheoMaAsync(db, danhMuc, maYeuCau, ct)
            ?? throw new InvalidOperationException($"Không đọc lại được phiếu yêu cầu {maYeuCau} vừa tạo.");
    }

    private async Task<string> ChenPhieuAsync(
        string maSv,
        Guid maPhong,
        int thoiGianMuonDuKienGio,
        DateTimeOffset thoiDiem,
        CancellationToken ct)
    {
        await using var lenh = db.Database.GetDbConnection().CreateCommand();
        lenh.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
        lenh.CommandText = """
            INSERT INTO muon_tra.phieu_yeu_cau_muon (ma_sv, ma_phong, thoi_gian_gui, thoi_gian_muon_du_kien_gio)
            VALUES (@ma_sv, @ma_phong, @thoi_gian_gui, @thoi_gian_muon_du_kien_gio)
            RETURNING ma_yeu_cau
            """;
        ThemThamSo(lenh, "ma_sv", maSv);
        ThemThamSo(lenh, "ma_phong", maPhong);
        ThemThamSo(lenh, "thoi_gian_gui", thoiDiem);
        ThemThamSo(lenh, "thoi_gian_muon_du_kien_gio", thoiGianMuonDuKienGio);

        return (string)(await lenh.ExecuteScalarAsync(ct))!;
    }

    private static void ThemThamSo(DbCommand lenh, string ten, object giaTri)
    {
        var thamSo = lenh.CreateParameter();
        thamSo.ParameterName = ten;
        thamSo.Value = giaTri;
        lenh.Parameters.Add(thamSo);
    }
}
