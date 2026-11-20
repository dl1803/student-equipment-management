using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.BaoTri.Infrastructure;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

public sealed class ProviderThamChieuHostTests
{
    [Fact]
    public void UC12_Runtime_DuProviderThamChieuCuaMoiModuleVaHandlerXoaGiaiQuyetDuoc()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting(
                    "ConnectionStrings:Default",
                    "Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung");
                builder.UseSetting("Jwt:Secret", "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len");
            });
        using var phamVi = factory.Services.CreateScope();

        var danhMuc = phamVi.ServiceProvider.GetServices<IThamChieuDanhMucProvider>().Select(x => x.GetType()).ToList();
        var khungPhat = phamVi.ServiceProvider.GetServices<IThamChieuKhungPhatProvider>().Select(x => x.GetType()).ToList();

        Assert.Contains(typeof(ThamChieuMuonTraProvider), danhMuc);
        Assert.Contains(typeof(ThamChieuDanhMucViPhamProvider), danhMuc);
        Assert.Contains(typeof(ThamChieuBaoTriProvider), danhMuc);
        Assert.Contains(typeof(ThamChieuQuyDinhProvider), danhMuc);
        Assert.Contains(typeof(ThamChieuMuonTraProvider), khungPhat);
        Assert.Contains(typeof(ThamChieuKhungPhatViPhamProvider), khungPhat);
        Assert.NotNull(phamVi.ServiceProvider.GetRequiredService<XoaLoaiThietBiHandler>());
        Assert.NotNull(phamVi.ServiceProvider.GetRequiredService<XoaThietBiHandler>());
    }
}
