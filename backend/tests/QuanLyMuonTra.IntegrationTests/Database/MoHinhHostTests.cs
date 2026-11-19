using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.IntegrationTests.Database;

public sealed class MoHinhHostTests
{
    [Fact]
    public void UC07_AppDbContextCuaHost_CoDayDuNamThucTheMuonTra()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=khong_ket_noi");
            builder.UseSetting("Jwt:Secret", "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len");
            builder.UseSetting("Jwt:Issuer", "quan-ly-muon-tra-d07k");
            builder.UseSetting("Jwt:Audience", "quan-ly-muon-tra-d07k");
        });
        using var phamVi = factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var kieu in new[]
                 {
                     typeof(PhieuYeuCauMuon),
                     typeof(ChiTietYeuCauMuon),
                     typeof(DangKyCho),
                     typeof(PhieuMuon),
                     typeof(ChiTietPhieuMuon)
                 })
        {
            Assert.NotNull(db.Model.FindEntityType(kieu));
        }
    }
}

