using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

public sealed class QuanTriVienSeeder(
    DbContext db,
    IPasswordHasher<NguoiDungHeThong> boBam,
    ILogger<QuanTriVienSeeder> nhatKy)
{
    public const int DoDaiMatKhauToiThieu = 8;

    public async Task ChayAsync(string? tenDangNhap, string? matKhau, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrEmpty(matKhau))
        {
            throw new InvalidOperationException("Seed:AdminUser và Seed:AdminPassword phải được đặt cùng nhau.");
        }

        if (matKhau.Length < DoDaiMatKhauToiThieu)
        {
            throw new InvalidOperationException($"Seed:AdminPassword phải có tối thiểu {DoDaiMatKhauToiThieu} ký tự.");
        }

        if (await db.Set<QuanTriVien>().AnyAsync(ct))
        {
            return;
        }

        var quanTri = QuanTriVien.Tao(
            Guid.CreateVersion7(),
            tenDangNhap,
            boBam.HashPassword(null!, matKhau),
            tenDangNhap,
            tenDangNhap);
        db.Set<QuanTriVien>().Add(quanTri);

        try
        {
            await db.SaveChangesAsync(ct);
            nhatKy.LogInformation("Đã tạo tài khoản quản trị viên khởi tạo {TenDangNhap}.", tenDangNhap);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            if (await db.Set<QuanTriVien>().AnyAsync(ct))
            {
                return;
            }

            throw;
        }
    }
}

