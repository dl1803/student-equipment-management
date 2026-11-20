using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.BaoTri;
using QuanLyMuonTra.Modules.BaoTri.Infrastructure;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.ViPham;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;

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

    [Fact]
    public void UC12_DangKyBonModule_ProviderThamChieuDanhMucCoThemViPham()
    {
        using var dichVu = TaoDichVu();
        using var phamVi = dichVu.CreateScope();

        var cacKieu = phamVi.ServiceProvider
            .GetServices<IThamChieuDanhMucProvider>()
            .Select(p => p.GetType())
            .ToList();

        Assert.Contains(typeof(ThamChieuDanhMucViPhamProvider), cacKieu);
    }

    [Fact]
    public void UC13_DangKyModule_ProviderThamChieuKhungPhatCoMuonTraVaViPham()
    {
        using var dichVu = TaoDichVu();
        using var phamVi = dichVu.CreateScope();

        var cacKieu = phamVi.ServiceProvider
            .GetServices<IThamChieuKhungPhatProvider>()
            .Select(p => p.GetType())
            .ToList();

        Assert.Contains(typeof(ThamChieuMuonTraProvider), cacKieu);
        Assert.Contains(typeof(ThamChieuKhungPhatViPhamProvider), cacKieu);
    }

    private static ServiceProvider TaoDichVu()
    {
        return new ServiceCollection()
            .AddDbContext<ThamChieuTestDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<ThamChieuTestDbContext>())
            .AddMuonTraModule()
            .AddQuyDinhModule()
            .AddBaoTriModule()
            .AddViPhamModule()
            .BuildServiceProvider();
    }

    private sealed class ThamChieuTestDbContext(DbContextOptions<ThamChieuTestDbContext> options) : DbContext(options)
    {
    }
}
