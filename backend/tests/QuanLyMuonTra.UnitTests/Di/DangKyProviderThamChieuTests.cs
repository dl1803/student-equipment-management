using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.BaoTri;
using QuanLyMuonTra.Modules.BaoTri.Infrastructure;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

namespace QuanLyMuonTra.UnitTests.Di;

public sealed class DangKyProviderThamChieuTests
{
    [Fact]
    public void UC12_DangKyBaModule_ProviderThamChieuDanhMucCoDuQuyDinhBaoTriVaMuonTra()
    {
        using var dichVu = TaoDichVu();
        using var phamVi = dichVu.CreateScope();

        var cacKieu = phamVi.ServiceProvider
            .GetServices<IThamChieuDanhMucProvider>()
            .Select(p => p.GetType())
            .ToList();

        Assert.Contains(typeof(ThamChieuQuyDinhProvider), cacKieu);
        Assert.Contains(typeof(ThamChieuBaoTriProvider), cacKieu);
        Assert.Contains(typeof(ThamChieuMuonTraProvider), cacKieu);
    }

    [Fact]
    public void UC12_DangKyBaModule_MoiKieuProviderChiXuatHienMotLan()
    {
        using var dichVu = TaoDichVu();
        using var phamVi = dichVu.CreateScope();

        var cacKieu = phamVi.ServiceProvider
            .GetServices<IThamChieuDanhMucProvider>()
            .Select(p => p.GetType())
            .ToList();

        Assert.Equal(cacKieu.Distinct().Count(), cacKieu.Count);
    }

    private static ServiceProvider TaoDichVu()
    {
        return new ServiceCollection()
            .AddDbContext<ThamChieuTestDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<ThamChieuTestDbContext>())
            .AddMuonTraModule()
            .AddQuyDinhModule()
            .AddBaoTriModule()
            .BuildServiceProvider();
    }

    private sealed class ThamChieuTestDbContext(DbContextOptions<ThamChieuTestDbContext> options) : DbContext(options)
    {
    }
}
