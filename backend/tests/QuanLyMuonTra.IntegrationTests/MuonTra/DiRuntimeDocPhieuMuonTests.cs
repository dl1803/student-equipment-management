using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class DiRuntimeDocPhieuMuonTests
{
    [Fact]
    public void UC05_UC07_UC09_RuntimeDangKyBanThatChoReaderVaNamHandlerDocPhieu()
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

        Assert.IsType<PhieuMuonReader>(dichVu.GetRequiredService<IPhieuMuonReader>());
        Assert.NotNull(dichVu.GetRequiredService<DsPhieuMuonSvHandler>());
        Assert.NotNull(dichVu.GetRequiredService<ChiTietPhieuMuonSvHandler>());
        Assert.NotNull(dichVu.GetRequiredService<DsPhieuMuonQuayHandler>());
        Assert.NotNull(dichVu.GetRequiredService<ChiTietPhieuMuonQuayHandler>());
        Assert.NotNull(dichVu.GetRequiredService<DsPhieuQuaHan30NgayHandler>());
    }
}
