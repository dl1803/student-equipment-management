using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class TaoNhanVienHandler(
    DbContext db,
    IPasswordHasher<NguoiDungHeThong> boBam,
    NhanVienDtoBuilder boDung)
{
    public async Task<NhanVienDto> XuLyAsync(
        string tenDangNhap,
        string matKhau,
        string hoTen,
        string maNv,
        string? email,
        CancellationToken ct)
    {
        await KiemTraKhongTrungAsync(tenDangNhap, maNv, ct);

        var nhanVien = NhanVienQuanLy.Tao(
            Guid.CreateVersion7(),
            tenDangNhap,
            boBam.HashPassword(null!, matKhau),
            hoTen,
            maNv,
            email);
        db.Set<NhanVienQuanLy>().Add(nhanVien);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            await KiemTraKhongTrungAsync(tenDangNhap, maNv, ct);
            throw;
        }

        return await boDung.TaoMotAsync(ThongTinNhanVien.Tu(nhanVien), ct);
    }

    private async Task KiemTraKhongTrungAsync(string tenDangNhap, string maNv, CancellationToken ct)
    {
        if (await db.Set<NguoiDungHeThong>().AsNoTracking().AnyAsync(x => x.TenDangNhap == tenDangNhap, ct))
        {
            throw new LoiNghiepVuException("TEN_DANG_NHAP_TRUNG", "Tên đăng nhập đã tồn tại.");
        }

        if (await db.Set<NhanVienQuanLy>().AsNoTracking().AnyAsync(x => x.MaNv == maNv, ct))
        {
            throw new LoiNghiepVuException("MA_NV_TRUNG", "Mã nhân viên đã tồn tại.");
        }
    }
}

