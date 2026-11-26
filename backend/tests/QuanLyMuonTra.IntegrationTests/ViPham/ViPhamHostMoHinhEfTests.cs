using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.ViPham.Domain;

namespace QuanLyMuonTra.IntegrationTests.ViPham;

public class ViPhamHostMoHinhEfTests
{
    [Fact]
    public void UC09_HostMoHinhEf_ChuaCacThucTheCuaViPham()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung");
                builder.UseSetting("Jwt:Secret", "khoa-kiem-thu-khong-phai-bi-mat-0123456789");
                builder.UseSetting("Jwt:Issuer", "kiem-thu");
                builder.UseSetting("Jwt:Audience", "kiem-thu");
            });

        using var pham = factory.Services.CreateScope();
        var db = pham.ServiceProvider.GetRequiredService<DbContext>();

        Assert.NotNull(db.Model.FindEntityType(typeof(BanGhiViPham)));
        Assert.NotNull(db.Model.FindEntityType(typeof(BienBanViPham)));
        Assert.NotNull(db.Model.FindEntityType(typeof(ChiTietBienBan)));
    }
}

