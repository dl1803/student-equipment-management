using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class RoiDangKyChoHandler(
    DbContext db,
    IDanhMucReader danhMuc,
    IAdvisoryLockService khoa,
    IFifoService? capLuot = null)
{
    public async Task<DangKyChoSvDto> XuLyAsync(string maSv, string maDangKy, CancellationToken ct)
    {
        KiemTraDieuKienDangKyCho.KiemTraMaDangKy(maDangKy);

        var dongTruoc = await DangKyChoTruyVan.LayDongTheoMaAsync(db, maDangKy, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy đăng ký chờ.");

        if (dongTruoc.MaSv != maSv)
        {
            throw new LoiNghiepVuException(MaLoi.KhongDuQuyen, "Không có quyền rời đăng ký chờ của người khác.");
        }

        var moGiaoDich = db.Database.CurrentTransaction is null;
        var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync(ct) : null;
        try
        {
            await HoSoSinhVienTruyVan.KhoaAsync(db, maSv, ct);
            await khoa.LockPhongLoaiAsync(dongTruoc.MaPhong, dongTruoc.MaLoai, ct);

            var dangKy = await db.Set<DangKyCho>().SingleAsync(d => d.MaDangKy == maDangKy, ct);

            var boCapLuot = dangKy.TrangThai == TrangThaiDangKyCho.DaThongBao
                ? (capLuot ?? throw new InvalidOperationException(
                    "Chưa có bộ cấp lượt (C9, V13): không thể rời đăng ký đang giữ chỗ vì không thể cấp lượt cho người kế tiếp."))
                : null;

            try
            {
                dangKy.Roi();
            }
            catch (NghiepVuException loi)
            {
                throw DichLoiMienNghiepVu.Chuyen(loi);
            }

            await db.SaveChangesAsync(ct);

            await DanhSoLaiHangChoAsync(dongTruoc.MaPhong, dongTruoc.MaLoai, ct);

            if (boCapLuot is not null)
            {
                await boCapLuot.CapLuotChoAsync(dongTruoc.MaPhong, dongTruoc.MaLoai, ct);
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

        return await DangKyChoTruyVan.LayTheoMaAsync(db, danhMuc, maDangKy, ct)
            ?? throw new InvalidOperationException($"Không đọc lại được đăng ký chờ {maDangKy} vừa rời.");
    }

    private Task<int> DanhSoLaiHangChoAsync(Guid maPhong, Guid maLoai, CancellationToken ct)
    {
        return db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE muon_tra.dang_ky_cho AS d
            SET so_thu_tu_cho = x.so_thu_tu
            FROM (
                SELECT ma_dang_ky, (row_number() OVER (ORDER BY thoi_diem_dang_ky, ma_dang_ky))::integer AS so_thu_tu
                FROM muon_tra.dang_ky_cho
                WHERE ma_phong = {maPhong} AND ma_loai = {maLoai} AND trang_thai = 'DANG_CHO'
            ) AS x
            WHERE d.ma_dang_ky = x.ma_dang_ky AND d.so_thu_tu_cho IS DISTINCT FROM x.so_thu_tu
            """, ct);
    }
}
