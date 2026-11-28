using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class GhiKetQuaKiemTraHandler(
    DbContext db,
    IClock dongHo,
    IDanhMucReader danhMuc,
    IQuyDinhReader quyDinh,
    IQuyenPhongService quyenPhong,
    IAdvisoryLockService khoa,
    IFifoService capLuot,
    IAnhService anhService,
    GhiDongKetQuaNhanTra ghiDong)
{
    private sealed record DongDaKiem(KetQuaKiemTraYeuCau YeuCau, ThietBiDto ThietBi);

    public async Task<PhieuMuonDayDuDto> XuLyAsync(
        Guid maNv,
        string maPhieu,
        IReadOnlyList<KetQuaKiemTraYeuCau> dsKetQua,
        CancellationToken ct)
    {
        KiemTraDocPhieuMuon.KiemTraMaPhieu(maPhieu);
        if (dsKetQua.Count == 0)
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Danh sách kết quả kiểm tra không được để trống.");
        }

        var du = await PhieuMuonDoc.TaiAsync(db, danhMuc, maPhieu, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy phiếu mượn.");

        var dsMaPhongDuocCap = (await quyenPhong.LayDsMaPhongAsync(maNv, ct)).ToHashSet();
        if (!du.ThietBi.Values.Any(t => dsMaPhongDuocCap.Contains(t.MaPhong)))
        {
            throw new LoiNghiepVuException(
                MaLoi.PhongChuaCapQuyen,
                "Nhân viên chưa được cấp quyền trên phòng của phiếu mượn này.");
        }

        var dsDaKiem = new List<DongDaKiem>();
        foreach (var yeuCau in dsKetQua.OrderBy(d => d.MaThietBi, StringComparer.Ordinal))
        {
            if (!du.ThietBi.TryGetValue(yeuCau.MaThietBi, out var thietBi))
            {
                throw new LoiNghiepVuException(
                    MaLoi.ThietBiKhongThuocPhieu,
                    $"Thiết bị {yeuCau.MaThietBi} không thuộc phiếu mượn {maPhieu}.");
            }

            if (!dsMaPhongDuocCap.Contains(thietBi.MaPhong))
            {
                throw new LoiNghiepVuException(
                    MaLoi.PhongChuaCapQuyen,
                    $"Nhân viên chưa được cấp quyền trên phòng của thiết bị {yeuCau.MaThietBi}.");
            }

            dsDaKiem.Add(new DongDaKiem(yeuCau, thietBi));
        }

        var giaoDich = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        PhieuMuonDayDuDto ketQua;
        try
        {
            ketQua = await GhiAsync(maNv, du.Phieu.MaSv, maPhieu, dsDaKiem, ct);

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

        return ketQua;
    }

    private async Task<PhieuMuonDayDuDto> GhiAsync(
        Guid maNv,
        string maSv,
        string maPhieu,
        IReadOnlyList<DongDaKiem> dsDaKiem,
        CancellationToken ct)
    {
        await HoSoSinhVienTruyVan.KhoaAsync(db, maSv, ct);

        var dsDongNguyenVen = dsDaKiem
            .Where(d => d.YeuCau.KetQua == KetQuaKiemTra.NguyenVen)
            .OrderBy(d => d.ThietBi.MaPhong)
            .ThenBy(d => d.ThietBi.MaLoai)
            .ToList();
        var dsCapCanKhoa = dsDongNguyenVen
            .Select(d => (d.ThietBi.MaPhong, d.ThietBi.MaLoai))
            .Distinct()
            .ToList();
        if (dsCapCanKhoa.Count > 0)
        {
            await khoa.LockDanhSachPhongLoaiAsync(dsCapCanKhoa, ct);
        }

        var phieu = await db.Set<PhieuMuon>()
            .Include(p => p.ChiTiet)
            .SingleAsync(p => p.MaPhieu == maPhieu, ct);

        await KiemTraKhungPhatAsync(dsDaKiem, ct);

        var bayGio = dongHo.UtcNow.ToUniversalTime();
        foreach (var dong in dsDaKiem)
        {
            await ghiDong.GhiAsync(phieu, dong.YeuCau, maNv, bayGio, ct);
        }

        try
        {
            phieu.XetDaTra();
        }
        catch (NghiepVuException loi)
        {
            throw DichLoiMienNghiepVu.Chuyen(loi);
        }

        await db.SaveChangesAsync(ct);

        foreach (var dong in dsDongNguyenVen)
        {
            await capLuot.CapLuotChoAsync(dong.ThietBi.MaPhong, dong.ThietBi.MaLoai, ct);
        }

        var duMoi = await PhieuMuonDoc.TaiAsync(db, danhMuc, maPhieu, ct)
            ?? throw new InvalidOperationException($"Không đọc lại được phiếu mượn {maPhieu} vừa ghi kết quả.");

        return await PhieuMuonDoc.DungAsync(duMoi, anhService, ct);
    }

    private async Task KiemTraKhungPhatAsync(IReadOnlyList<DongDaKiem> dsDaKiem, CancellationToken ct)
    {
        foreach (var dong in dsDaKiem)
        {
            if (dong.YeuCau.KetQua is not (KetQuaKiemTra.HuHong or KetQuaKiemTra.ThieuPhuKien)
                || dong.YeuCau.MaKhungPhat is not { } maKhung
                || maKhung == Guid.Empty)
            {
                continue;
            }

            var khung = await quyDinh.LayKhungPhatAsync(maKhung, ct);
            if (khung is null || !khung.DangSuDung)
            {
                throw new LoiNghiepVuException(
                    MaLoi.DuLieuKhongHopLe,
                    $"Khung phạt của thiết bị {dong.YeuCau.MaThietBi} không tồn tại hoặc không còn được sử dụng.");
            }
        }
    }
}
