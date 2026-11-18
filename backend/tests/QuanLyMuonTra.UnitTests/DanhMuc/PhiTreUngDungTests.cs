using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class PhiTreUngDungTests
{
    private static readonly JsonSerializerOptions TuyChonJson = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData(0L)]
    [InlineData(50000L)]
    [InlineData(99_999_999_999_999L)]
    public void UC13_CapNhatPhiTre_GiaTriHopLe_DoiPhi(long phi)
    {
        var loai = new LoaiThietBi("Laptop", null, null, 30, 10000);

        loai.CapNhatPhiTre(phi);

        Assert.Equal(phi, loai.PhiTreMoiNgay);
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(100_000_000_000_000L)]
    public void UC13_CapNhatPhiTre_NgoaiKhoang_Nem422VaGiuPhiCu(long phi)
    {
        var loai = new LoaiThietBi("Laptop", null, null, 30, 10000);

        var loi = Assert.Throws<LoiNghiepVuException>(() => loai.CapNhatPhiTre(phi));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
        Assert.Equal(10000, loai.PhiTreMoiNgay);
    }

    [Fact]
    public void UC13_Json_PhiTreDocDuocTheoTenTruongOpenApi()
    {
        var yeuCau = JsonSerializer.Deserialize<PhiTreRequest>("""{"phiTreMoiNgay":50000}""", TuyChonJson);

        Assert.Equal(new PhiTreRequest(50000), yeuCau);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"phiTreMoiNgay":50000.5}""")]
    [InlineData("""{"phiTreMoiNgay":"abc"}""")]
    public void UC13_Json_PhiTreThieuHoacKhongPhaiSoNguyen_NemJsonException(string json)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PhiTreRequest>(json, TuyChonJson));
    }

    [Fact]
    public void UC13_DangKyModuleDanhMuc_HandlerPhiTreGiaiQuyetDuoc()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<PhiTreKhongKetNoiDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<PhiTreKhongKetNoiDbContext>())
            .AddDanhMucModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();

        Assert.NotNull(phamVi.ServiceProvider.GetRequiredService<CapNhatPhiTreHandler>());
    }

    private sealed class PhiTreKhongKetNoiDbContext(DbContextOptions<PhiTreKhongKetNoiDbContext> options) : DbContext(options)
    {
    }
}
