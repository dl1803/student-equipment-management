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
    IFifoService capLuot)
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

            var dangGiuCho = dangKy.TrangThai == TrangThaiDangKyCho.DaThongBao;

            try
            {
                dangKy.Roi();
            }
            catch (NghiepVuException loi)
            {
                throw DichLoiMienNghiepVu.Chuyen(loi);
            }

            await db.SaveChangesAsync(ct);

            await DangKyChoTruyVan.DanhSoLaiHangChoAsync(db, dongTruoc.MaPhong, dongTruoc.MaLoai, ct);

            if (dangGiuCho)
            {
                await capLuot.CapLuotChoAsync(dongTruoc.MaPhong, dongTruoc.MaLoai, ct);
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
}
