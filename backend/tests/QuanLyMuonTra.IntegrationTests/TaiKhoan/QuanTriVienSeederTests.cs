using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public class QuanTriVienSeederTests
{
    [Fact]
    public async Task UC19_Seed_ChuaCoQuanTri_TaoMotQuanTriVaBamMatKhau()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaQuanTriAsync(db);
        var boBam = new PasswordHasher<NguoiDungHeThong>();
        var tenDangNhap = $"qt-kt-{Guid.NewGuid():N}";
        var seeder = new QuanTriVienSeeder(db, boBam, NullLogger<QuanTriVienSeeder>.Instance);

        await seeder.ChayAsync(tenDangNhap, "MatKhauKiemThu-1", CancellationToken.None);

        var quanTri = await db.Set<QuanTriVien>().AsNoTracking().SingleAsync();
        Assert.Equal(tenDangNhap, quanTri.TenDangNhap);
        Assert.Equal(tenDangNhap, quanTri.MaQt);
        Assert.NotEqual("MatKhauKiemThu-1", quanTri.MatKhauBam);
        Assert.Equal(PasswordVerificationResult.Success, boBam.VerifyHashedPassword(quanTri, quanTri.MatKhauBam, "MatKhauKiemThu-1"));
        Assert.Equal(MaTrangThaiKhoa.HoatDong, quanTri.TrangThaiKhoa);
    }

    [Fact]
    public async Task UC19_Seed_DaCoQuanTri_KhongTaoThemVaKhongDoiMatKhau()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaQuanTriAsync(db);
        var boBam = new PasswordHasher<NguoiDungHeThong>();
        var seeder = new QuanTriVienSeeder(db, boBam, NullLogger<QuanTriVienSeeder>.Instance);
        var tenDau = $"qt-kt-{Guid.NewGuid():N}";
        await seeder.ChayAsync(tenDau, "MatKhauKiemThu-1", CancellationToken.None);

        await seeder.ChayAsync($"qt-kt-{Guid.NewGuid():N}", "MatKhauKhac-2", CancellationToken.None);

        var quanTri = await db.Set<QuanTriVien>().AsNoTracking().SingleAsync();
        Assert.Equal(tenDau, quanTri.TenDangNhap);
        Assert.Equal(PasswordVerificationResult.Success, boBam.VerifyHashedPassword(quanTri, quanTri.MatKhauBam, "MatKhauKiemThu-1"));
    }

    [Fact]
    public async Task UC19_Seed_MatKhauNganHoacThieuCauHinh_NemLoiVaKhongGhiDuLieu()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaQuanTriAsync(db);
        var seeder = new QuanTriVienSeeder(db, new PasswordHasher<NguoiDungHeThong>(), NullLogger<QuanTriVienSeeder>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.ChayAsync("qt-kt", "ngan", CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.ChayAsync("qt-kt", null, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.ChayAsync(null, "MatKhauKiemThu-1", CancellationToken.None));

        Assert.False(await db.Set<QuanTriVien>().AsNoTracking().AnyAsync());
    }

    [Fact]
    public async Task NFR02_Seed_GhiNhatKyThemVaKhongLoMatKhau()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaQuanTriAsync(db);
        var tenDangNhap = $"qt-kt-{Guid.NewGuid():N}";
        var seeder = new QuanTriVienSeeder(db, new PasswordHasher<NguoiDungHeThong>(), NullLogger<QuanTriVienSeeder>.Instance);

        await seeder.ChayAsync(tenDangNhap, "MatKhauKiemThu-1", CancellationToken.None);

        var quanTri = await db.Set<QuanTriVien>().AsNoTracking().SingleAsync();
        var giaTriSau = await db.Database.SqlQuery<string>($"""
            SELECT gia_tri_sau::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong_id = {quanTri.MaNguoiDung.ToString()}
            """).ToListAsync();
        var json = Assert.Single(giaTriSau);
        Assert.Contains("TenDangNhap", json);
        Assert.DoesNotContain("MatKhauBam", json);
        Assert.DoesNotContain("MatKhauKiemThu-1", json);
    }

    private static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(NguoiDungHeThong).Assembly])
            .BuildServiceProvider();

    private static async Task XoaQuanTriAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("DELETE FROM tai_khoan.quan_tri_vien");
        await db.Database.ExecuteSqlRawAsync("""
            DELETE FROM tai_khoan.nguoi_dung_he_thong n
            WHERE NOT EXISTS (SELECT 1 FROM tai_khoan.nhan_vien_quan_ly v WHERE v.ma_nguoi_dung = n.ma_nguoi_dung)
            """);
    }
}

