using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class DanhMucReaderKhongCsdlTests
{
    [Fact]
    public void UC12_DangKyModuleDanhMuc_IDanhMucReaderTroToiDanhMucReader()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<KhongKetNoiDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<KhongKetNoiDbContext>())
            .AddDanhMucModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();

        var reader = phamVi.ServiceProvider.GetRequiredService<IDanhMucReader>();

        Assert.IsType<DanhMucReader>(reader);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UC12_LayThietBi_MaRongHoacKhoangTrang_TraNullKhongTruyCapCsdl(string maThietBi)
    {
        var options = new DbContextOptionsBuilder<KhongKetNoiDbContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
            .Options;
        using var db = new KhongKetNoiDbContext(options);
        var reader = new DanhMucReader(db);

        var ketQua = await reader.LayThietBiAsync(maThietBi, CancellationToken.None);

        Assert.Null(ketQua);
    }

    private sealed class KhongKetNoiDbContext(DbContextOptions<KhongKetNoiDbContext> options) : DbContext(options)
    {
    }
}
