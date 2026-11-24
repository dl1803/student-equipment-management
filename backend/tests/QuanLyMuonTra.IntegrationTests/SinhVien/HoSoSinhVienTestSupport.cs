using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class PhienKiemThuHoSo : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly AsyncServiceScope _phamVi;
    private readonly IDbContextTransaction _giaoDich;

    private PhienKiemThuHoSo(
        WebApplicationFactory<Program> factory,
        AsyncServiceScope phamVi,
        IDbContextTransaction giaoDich,
        AppDbContext db)
    {
        _factory = factory;
        _phamVi = phamVi;
        _giaoDich = giaoDich;
        Db = db;
    }

    public AppDbContext Db { get; }

    public IServiceProvider DichVu => _phamVi.ServiceProvider;

    public static async Task<PhienKiemThuHoSo> BatDauAsync(int gioiHanThietBiDaiHoc)
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", KetNoiKiemThu.LayChuoiKetNoi());
            builder.UseSetting("Jwt:Secret", "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len");
            builder.UseSetting("Jwt:Issuer", "quan-ly-muon-tra-d07k");
            builder.UseSetting("Jwt:Audience", "quan-ly-muon-tra-d07k");
        });
        var phamVi = factory.Services.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var giaoDich = await db.Database.BeginTransactionAsync();

        var duongDan = Path.Combine(AppContext.BaseDirectory, "MuonTra", "D02-V-du-lieu-thu.sql");
        var sql = await File.ReadAllTextAsync(duongDan);
        await using (var lenh = db.Database.GetDbConnection().CreateCommand())
        {
            lenh.Transaction = giaoDich.GetDbTransaction();
            lenh.CommandText = sql;
            await lenh.ExecuteNonQueryAsync();
        }

        var soDong = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE quy_dinh.quy_dinh_muon SET so_thiet_bi_toi_da = {gioiHanThietBiDaiHoc}
            WHERE ma_nhom = 'DAI_HOC' AND ma_loai IS NULL
            """);
        Assert.Equal(1, soDong);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO vi_pham.vi_pham (ma_vi_pham, ma_sv, loai_vi_pham, hoc_ky, ma_bien_ban)
            VALUES ({Guid.Parse("019c0000-0000-7000-8000-0000000d0f01")}, 'D02-SV-C', 'HU_HONG', '2026-1', 'BB-261114-990001')
            """);

        return new PhienKiemThuHoSo(factory, phamVi, giaoDich, db);
    }

    public async ValueTask DisposeAsync()
    {
        await _giaoDich.DisposeAsync();
        await _phamVi.DisposeAsync();
        await _factory.DisposeAsync();
    }
}

