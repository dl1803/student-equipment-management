using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class HostMoHinhEfMuonTraTests
{
    [Fact]
    public void UC02_HostMoHinhEf_ChuaCacThucTheCuaMuonTra()
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

        Assert.NotNull(db.Model.FindEntityType(typeof(PhieuYeuCauMuon)));
        Assert.NotNull(db.Model.FindEntityType(typeof(ChiTietYeuCauMuon)));
        Assert.NotNull(db.Model.FindEntityType(typeof(PhieuMuon)));
        Assert.NotNull(db.Model.FindEntityType(typeof(ChiTietPhieuMuon)));
    }
}
