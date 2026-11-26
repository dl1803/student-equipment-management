using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class ChuyenLuotDanhSachChoHandler(
    DbContext db,
    IDanhMucReader danhMuc,
    IQuyenPhongService quyenPhong,
    IAdvisoryLockService khoa,
    IFifoService capLuot)
{
    public async Task<DangKyChoQuayDto> XuLyAsync(Guid maNv, string maDangKy, CancellationToken ct)
    {
        KiemTraDieuKienDangKyCho.KiemTraMaDangKy(maDangKy);

        var dongTruoc = await DangKyChoTruyVan.LayDongTheoMaAsync(db, maDangKy, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy đăng ký chờ.");

        await quyenPhong.EnsureAsync(maNv, dongTruoc.MaPhong, ct);

        var moGiaoDich = db.Database.CurrentTransaction is null;
        var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync(ct) : null;
        try
        {
            await HoSoSinhVienTruyVan.KhoaAsync(db, dongTruoc.MaSv, ct);
            await khoa.LockPhongLoaiAsync(dongTruoc.MaPhong, dongTruoc.MaLoai, ct);

            var dangKy = await db.Set<DangKyCho>().SingleAsync(d => d.MaDangKy == maDangKy, ct);

            try
            {
                dangKy.ChuyenLuot();
            }
            catch (NghiepVuException loi)
            {
                throw DichLoiMienNghiepVu.Chuyen(loi);
            }

            await db.SaveChangesAsync(ct);

            await capLuot.CapLuotChoAsync(dongTruoc.MaPhong, dongTruoc.MaLoai, ct);

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

        return await DangKyChoTruyVan.LayQuayTheoMaAsync(db, danhMuc, maDangKy, ct)
            ?? throw new InvalidOperationException($"Không đọc lại được đăng ký chờ {maDangKy} vừa chuyển lượt.");
    }
}
