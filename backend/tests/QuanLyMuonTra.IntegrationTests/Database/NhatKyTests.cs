using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.Database;

public class NhatKyTests
{
    [Fact]
    public async Task NFR02_ThemThucTheNhayCam_GhiNhatKyThemVaLoaiMatKhauBam()
    {
        await using var dichVu = TaoDichVu(new NguoiDungGia());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var maNguoiDung = Guid.CreateVersion7();

        db.Set<NguoiDungKiemThuNhatKy>().Add(new NguoiDungKiemThuNhatKy
        {
            MaNguoiDung = maNguoiDung,
            TenDangNhap = $"kt-{maNguoiDung}",
            MatKhauBam = "BI-MAT-KHONG-DUOC-GHI",
            HoTen = "Nguoi kiem thu"
        });
        await db.SaveChangesAsync();

        var thaoTac = await LayChuoiAsync(db, $"""
            SELECT thao_tac AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = 'tai_khoan.nguoi_dung_he_thong' AND doi_tuong_id = {maNguoiDung.ToString()}
            """);
        var giaTriSau = await LayChuoiAsync(db, $"""
            SELECT gia_tri_sau::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = 'tai_khoan.nguoi_dung_he_thong' AND doi_tuong_id = {maNguoiDung.ToString()}
            """);

        Assert.Equal("THEM", Assert.Single(thaoTac));
        var json = Assert.Single(giaTriSau);
        Assert.Contains("TenDangNhap", json);
        Assert.DoesNotContain("MatKhauBam", json);
        Assert.DoesNotContain("BI-MAT-KHONG-DUOC-GHI", json);
    }

    [Fact]
    public async Task NFR02_SuaThucThe_GhiNhatKySuaChiCotBiDoi()
    {
        await using var dichVu = TaoDichVu(new NguoiDungGia());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var maPhong = Guid.CreateVersion7();
        var phong = new PhongKiemThuNhatKy { MaPhong = maPhong, TenPhong = "Ten cu", CoSo = "CS1" };
        db.Set<PhongKiemThuNhatKy>().Add(phong);
        await db.SaveChangesAsync();

        phong.TenPhong = "Ten moi";
        await db.SaveChangesAsync();

        var truoc = await LayChuoiAsync(db, $"""
            SELECT gia_tri_truoc::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.phong_thiet_bi' AND doi_tuong_id = {maPhong.ToString()}
            """);
        var sau = await LayChuoiAsync(db, $"""
            SELECT gia_tri_sau::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.phong_thiet_bi' AND doi_tuong_id = {maPhong.ToString()}
            """);

        var jsonTruoc = Assert.Single(truoc);
        var jsonSau = Assert.Single(sau);
        Assert.Contains("Ten cu", jsonTruoc);
        Assert.Contains("Ten moi", jsonSau);
        Assert.DoesNotContain("CoSo", jsonTruoc);
        Assert.DoesNotContain("CoSo", jsonSau);
    }

    [Fact]
    public async Task NFR02_XoaThucThe_GhiNhatKyXoaVoiGiaTriTruoc()
    {
        await using var dichVu = TaoDichVu(new NguoiDungGia());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var maPhong = Guid.CreateVersion7();
        var phong = new PhongKiemThuNhatKy { MaPhong = maPhong, TenPhong = "Phong se xoa", CoSo = "CS1" };
        db.Set<PhongKiemThuNhatKy>().Add(phong);
        await db.SaveChangesAsync();

        db.Set<PhongKiemThuNhatKy>().Remove(phong);
        await db.SaveChangesAsync();

        var truoc = await LayChuoiAsync(db, $"""
            SELECT COALESCE(gia_tri_truoc::text, '') AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'XOA' AND doi_tuong = 'danh_muc.phong_thiet_bi' AND doi_tuong_id = {maPhong.ToString()}
            """);
        var sau = await LayChuoiAsync(db, $"""
            SELECT COALESCE(gia_tri_sau::text, '') AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'XOA' AND doi_tuong = 'danh_muc.phong_thiet_bi' AND doi_tuong_id = {maPhong.ToString()}
            """);

        Assert.Contains("Phong se xoa", Assert.Single(truoc));
        Assert.Equal(string.Empty, Assert.Single(sau));
    }

    [Fact]
    public async Task NFR02_NguoiThucHien_DuocGhiTheoICurrentUser()
    {
        var nguoiDung = new NguoiDungGia();
        await using var dichVu = TaoDichVu(nguoiDung);
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var maNguoiDung = Guid.CreateVersion7();
        db.Set<NguoiDungKiemThuNhatKy>().Add(new NguoiDungKiemThuNhatKy
        {
            MaNguoiDung = maNguoiDung,
            TenDangNhap = $"kt-{maNguoiDung}",
            MatKhauBam = "khong-dung",
            HoTen = "Nguoi kiem thu"
        });
        await db.SaveChangesAsync();
        nguoiDung.MaNguoiDung = maNguoiDung;
        var maPhong = Guid.CreateVersion7();

        db.Set<PhongKiemThuNhatKy>().Add(new PhongKiemThuNhatKy { MaPhong = maPhong, TenPhong = "Phong", CoSo = "CS1" });
        await db.SaveChangesAsync();

        var nguoiThucHien = await LayChuoiAsync(db, $"""
            SELECT COALESCE(ma_nguoi_dung::text, '') AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = 'danh_muc.phong_thiet_bi' AND doi_tuong_id = {maPhong.ToString()}
            """);

        Assert.Equal(maNguoiDung.ToString(), Assert.Single(nguoiThucHien));
    }

    [Fact]
    public async Task NFR02_GhiNhatKyLoi_HoanTacThayDoiNghiepVu()
    {
        var nguoiDung = new NguoiDungGia { MaSv = "KHONG-TON-TAI-D03K" };
        await using var dichVu = TaoDichVu(nguoiDung);
        var maPhong = Guid.CreateVersion7();
        try
        {
            await using (var phamVi = dichVu.CreateAsyncScope())
            {
                var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Set<PhongKiemThuNhatKy>().Add(new PhongKiemThuNhatKy { MaPhong = maPhong, TenPhong = "Phong", CoSo = "CS1" });

                await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            }

            Assert.Equal(0, await DemPhongAsync(dichVu, maPhong));
        }
        finally
        {
            await using var phamVi = dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM danh_muc.phong_thiet_bi WHERE ma_phong = {maPhong}");
        }
    }

    [Fact]
    public async Task NFR02_GhiTuongMinhNgoaiGiaoDich_BiTuChoi()
    {
        await using var dichVu = TaoDichVu(new NguoiDungGia());
        await using var phamVi = dichVu.CreateAsyncScope();
        var logger = phamVi.ServiceProvider.GetRequiredService<IAuditLogger>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => logger.GhiAsync("KIEM_THU", "kiem_thu", null, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task NFR02_GhiTuongMinhSauSqlTrucTiep_CungGiaoDichVaLoaiTruongNhayCam()
    {
        await using var dichVu = TaoDichVu(new NguoiDungGia());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = phamVi.ServiceProvider.GetRequiredService<IAuditLogger>();
        var maPhong = Guid.CreateVersion7();
        await using var giaoDich = await db.Database.BeginTransactionAsync();

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so) VALUES ({maPhong}, 'Ten cu', 'CS1')");
        var soDong = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE danh_muc.phong_thiet_bi SET ten_phong = 'Ten moi' WHERE ma_phong = {maPhong} AND ten_phong = 'Ten cu'");
        Assert.Equal(1, soDong);

        await logger.GhiAsync(
            "DOI_TEN_PHONG",
            "danh_muc.phong_thiet_bi",
            maPhong.ToString(),
            new Dictionary<string, object?> { ["TenPhong"] = "Ten cu" },
            new Dictionary<string, object?> { ["TenPhong"] = "Ten moi", ["mat_khau_bam"] = "BI-MAT" },
            CancellationToken.None);

        var giaTriSau = await LayChuoiAsync(db, $"""
            SELECT gia_tri_sau::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'DOI_TEN_PHONG' AND doi_tuong_id = {maPhong.ToString()}
            """);

        var json = Assert.Single(giaTriSau);
        Assert.Contains("Ten moi", json);
        Assert.DoesNotContain("mat_khau_bam", json);
        Assert.DoesNotContain("BI-MAT", json);
    }

    [Fact]
    public async Task NFR02_GhiTuongMinhLoi_HoanTacCaSqlTrucTiepTruocDo()
    {
        var nguoiDung = new NguoiDungGia { MaSv = "KHONG-TON-TAI-D03K" };
        await using var dichVu = TaoDichVu(nguoiDung);
        var maPhong = Guid.CreateVersion7();
        await using (var phamVi = dichVu.CreateAsyncScope())
        {
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = phamVi.ServiceProvider.GetRequiredService<IAuditLogger>();
            await using var giaoDich = await db.Database.BeginTransactionAsync();
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so) VALUES ({maPhong}, 'Phong', 'CS1')");

            await Assert.ThrowsAsync<PostgresException>(
                () => logger.GhiAsync("KIEM_THU", "danh_muc.phong_thiet_bi", maPhong.ToString(), null, null, CancellationToken.None));
            await giaoDich.RollbackAsync();
        }

        Assert.Equal(0, await DemPhongAsync(dichVu, maPhong));
    }

    private static ServiceProvider TaoDichVu(NguoiDungGia nguoiDung) =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(NhatKyTests).Assembly])
            .AddSingleton<ICurrentUser>(nguoiDung)
            .BuildServiceProvider();

    private static Task<List<string>> LayChuoiAsync(AppDbContext db, FormattableString sql) =>
        db.Database.SqlQuery<string>(sql).ToListAsync();

    private static async Task<int> DemPhongAsync(ServiceProvider dichVu, Guid maPhong)
    {
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Database.SqlQuery<int>($"""
            SELECT count(*)::int AS "Value" FROM danh_muc.phong_thiet_bi WHERE ma_phong = {maPhong}
            """).SingleAsync();
    }

    public class NguoiDungGia : ICurrentUser
    {
        public Guid? MaNguoiDung { get; set; }

        public string? MaSv { get; set; }
    }

    public class PhongKiemThuNhatKy : IAuditable
    {
        public Guid MaPhong { get; set; }

        public string TenPhong { get; set; } = string.Empty;

        public string CoSo { get; set; } = string.Empty;
    }

    public class PhongKiemThuNhatKyCauHinh : IEntityTypeConfiguration<PhongKiemThuNhatKy>
    {
        public void Configure(EntityTypeBuilder<PhongKiemThuNhatKy> builder)
        {
            builder.ToTable("phong_thiet_bi", "danh_muc");
            builder.HasKey(p => p.MaPhong);
            builder.Property(p => p.MaPhong).HasColumnName("ma_phong").ValueGeneratedNever();
            builder.Property(p => p.TenPhong).HasColumnName("ten_phong");
            builder.Property(p => p.CoSo).HasColumnName("co_so");
        }
    }

    public class NguoiDungKiemThuNhatKy : IAuditable
    {
        public Guid MaNguoiDung { get; set; }

        public string TenDangNhap { get; set; } = string.Empty;

        public string MatKhauBam { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;
    }

    public class NguoiDungKiemThuNhatKyCauHinh : IEntityTypeConfiguration<NguoiDungKiemThuNhatKy>
    {
        public void Configure(EntityTypeBuilder<NguoiDungKiemThuNhatKy> builder)
        {
            builder.ToTable("nguoi_dung_he_thong", "tai_khoan");
            builder.HasKey(n => n.MaNguoiDung);
            builder.Property(n => n.MaNguoiDung).HasColumnName("ma_nguoi_dung").ValueGeneratedNever();
            builder.Property(n => n.TenDangNhap).HasColumnName("ten_dang_nhap");
            builder.Property(n => n.MatKhauBam).HasColumnName("mat_khau_bam");
            builder.Property(n => n.HoTen).HasColumnName("ho_ten");
        }
    }
}

