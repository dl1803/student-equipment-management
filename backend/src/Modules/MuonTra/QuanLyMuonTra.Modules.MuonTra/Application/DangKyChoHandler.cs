using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DangKyChoHandler(
    DbContext db,
    IClock dongHo,
    IDanhMucReader danhMuc,
    IAvailabilityService khaDung,
    IAdvisoryLockService khoa)
{
    public async Task<DangKyChoSvDto> XuLyAsync(string maSv, Guid maPhong, Guid maLoai, CancellationToken ct)
    {
        var thoiDiem = dongHo.UtcNow.ToUniversalTime();

        if (await danhMuc.LayPhongAsync(maPhong, ct) is null)
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Phòng thiết bị không tồn tại.");
        }

        var loai = await danhMuc.LayLoaiAsync(maLoai, ct);
        if (loai is null || loai.NgungSuDung)
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Loại thiết bị không tồn tại hoặc đã ngừng sử dụng.");
        }

        var moGiaoDich = db.Database.CurrentTransaction is null;
        var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync(ct) : null;
        string maDangKy;
        try
        {
            await HoSoSinhVienTruyVan.KhoaAsync(db, maSv, ct);
            await khoa.LockPhongLoaiAsync(maPhong, maLoai, ct);

            var trangThaiQuyenMuon = await db.Database.SqlQuery<string>($"""
                    SELECT trang_thai_quyen_muon AS "Value" FROM sinh_vien.sinh_vien WHERE ma_sv = {maSv}
                    """)
                .SingleOrDefaultAsync(ct)
                ?? throw new LoiNghiepVuException(MaLoi.KhongDuQuyen, "Không tìm thấy hồ sơ sinh viên.");

            KiemTraDieuKienDangKyCho.KiemTraQuyenMuon(trangThaiQuyenMuon);
            KiemTraDieuKienDangKyCho.KiemTraConHang(await khaDung.TinhKhaDungAsync(maPhong, maLoai, ct));

            var soDangCho = await db.Database.SqlQuery<int>($"""
                    SELECT count(*)::integer AS "Value"
                    FROM muon_tra.dang_ky_cho
                    WHERE ma_phong = {maPhong} AND ma_loai = {maLoai} AND trang_thai = 'DANG_CHO'
                    """)
                .SingleAsync(ct);

            maDangKy = await ChenAsync(maSv, maPhong, maLoai, thoiDiem, soDangCho + 1, ct);

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

        return await DangKyChoTruyVan.LayTheoMaAsync(db, danhMuc, maDangKy, ct)
            ?? throw new InvalidOperationException($"Không đọc lại được đăng ký chờ {maDangKy} vừa tạo.");
    }

    private async Task<string> ChenAsync(
        string maSv,
        Guid maPhong,
        Guid maLoai,
        DateTimeOffset thoiDiem,
        int soThuTuCho,
        CancellationToken ct)
    {
        await using var lenh = db.Database.GetDbConnection().CreateCommand();
        lenh.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
        lenh.CommandText = """
            INSERT INTO muon_tra.dang_ky_cho (ma_sv, ma_phong, ma_loai, thoi_diem_dang_ky, so_thu_tu_cho)
            VALUES (@ma_sv, @ma_phong, @ma_loai, @thoi_diem_dang_ky, @so_thu_tu_cho)
            RETURNING ma_dang_ky
            """;
        ThemThamSo(lenh, "ma_sv", maSv);
        ThemThamSo(lenh, "ma_phong", maPhong);
        ThemThamSo(lenh, "ma_loai", maLoai);
        ThemThamSo(lenh, "thoi_diem_dang_ky", thoiDiem);
        ThemThamSo(lenh, "so_thu_tu_cho", soThuTuCho);

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
