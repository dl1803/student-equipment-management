using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class DiRuntimeDangKyChoTests
{
    [Fact]
    public void UC03_UC10_RuntimeResolveHandlerVaBoCapLuotThat()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung");
                builder.UseSetting("Jwt:Secret", "khoa-kiem-thu-khong-phai-bi-mat-0123456789");
                builder.UseSetting("Jwt:Issuer", "kiem-thu");
                builder.UseSetting("Jwt:Audience", "kiem-thu");
            });

        using var phamVi = factory.Services.CreateScope();
        var dichVu = phamVi.ServiceProvider;

        Assert.NotNull(dichVu.GetRequiredService<DangKyChoHandler>());
        Assert.NotNull(dichVu.GetRequiredService<DsDangKyChoSvHandler>());
        Assert.NotNull(dichVu.GetRequiredService<RoiDangKyChoHandler>());
        Assert.IsType<FifoService>(dichVu.GetRequiredService<IFifoService>());
    }
}
