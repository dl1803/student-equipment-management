using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

namespace QuanLyMuonTra.UnitTests.QuyDinh;

public sealed class QuyDinhReaderKhongCsdlTests
{
    [Fact]
    public void UC13_DangKyModuleQuyDinh_IQuyDinhReaderTroToiQuyDinhReader()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<KhongKetNoiDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<KhongKetNoiDbContext>())
            .AddQuyDinhModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();

        var reader = phamVi.ServiceProvider.GetRequiredService<IQuyDinhReader>();

        Assert.IsType<QuyDinhReader>(reader);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UC13_LayQuyDinhNhom_MaNhomRongHoacKhoangTrang_TraNullKhongTruyCapCsdl(string maNhom)
    {
        var options = new DbContextOptionsBuilder<KhongKetNoiDbContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
            .Options;
        using var db = new KhongKetNoiDbContext(options);
        var reader = new QuyDinhReader(db);

        var ketQua = await reader.LayQuyDinhNhomAsync(maNhom, CancellationToken.None);

        Assert.Null(ketQua);
    }

    private sealed class KhongKetNoiDbContext(DbContextOptions<KhongKetNoiDbContext> options) : DbContext(options)
    {
    }
}
