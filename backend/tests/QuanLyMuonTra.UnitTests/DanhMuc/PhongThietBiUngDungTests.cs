using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class PhongThietBiUngDungTests
{
    private static readonly JsonSerializerOptions TuyChonJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public void UC12_KiemTraPhong_DuLieuHopLe_Dat()
    {
        var ketQua = new PhongThietBiRequestValidator().Validate(new PhongThietBiRequest("Phòng A", "Cơ sở 1"));

        Assert.True(ketQua.IsValid);
    }

    [Theory]
    [InlineData(null, "Cơ sở 1", "TenPhong")]
    [InlineData("", "Cơ sở 1", "TenPhong")]
    [InlineData("   ", "Cơ sở 1", "TenPhong")]
    [InlineData("Phòng A", null, "CoSo")]
    [InlineData("Phòng A", "", "CoSo")]
    [InlineData("Phòng A", "   ", "CoSo")]
    public void UC12_KiemTraPhong_TruongRong_BaoLoiDungTruong(string? tenPhong, string? coSo, string truongLoi)
    {
        var ketQua = new PhongThietBiRequestValidator().Validate(new PhongThietBiRequest(tenPhong!, coSo!));

        Assert.False(ketQua.IsValid);
        Assert.Contains(ketQua.Errors, l => l.PropertyName == truongLoi);
    }

    [Fact]
    public void UC12_KiemTraPhong_HaiTruongRong_BaoLoiCaHaiTruong()
    {
        var ketQua = new PhongThietBiRequestValidator().Validate(new PhongThietBiRequest(string.Empty, string.Empty));

        var cacTruong = ketQua.Errors.Select(l => l.PropertyName).Distinct().OrderBy(t => t, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { "CoSo", "TenPhong" }, cacTruong);
    }

    [Fact]
    public async Task UC12_TaoPhong_DuLieuSai_Nem400TruocKhiTruyCapCsdl()
    {
        using var db = TaoDbContext();
        var handler = new TaoPhongThietBiHandler(db);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(new PhongThietBiRequest(string.Empty, "Cơ sở 1"), CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public void UC12_Json_YeuCauPhongDocDuocTheoTenTruongOpenApi()
    {
        var yeuCau = JsonSerializer.Deserialize<PhongThietBiRequest>("""{"tenPhong":"Phòng A","coSo":"Cơ sở 1"}""", TuyChonJson);

        Assert.Equal(new PhongThietBiRequest("Phòng A", "Cơ sở 1"), yeuCau);
    }

    [Fact]
    public void UC12_YeuCauPhong_ChiCoTenPhongVaCoSo_KhongCoTinhTrang()
    {
        var cacTen = typeof(PhongThietBiRequest).GetProperties().Select(p => p.Name).OrderBy(t => t, StringComparer.Ordinal).ToArray();

        Assert.Equal(new[] { "CoSo", "TenPhong" }, cacTen);
    }

    [Fact]
    public void UC12_DangKyModuleDanhMuc_HandlerVaValidatorPhongGiaiQuyetDuoc()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<PhongKhongKetNoiDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<PhongKhongKetNoiDbContext>())
            .AddDanhMucModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();
        var dv = phamVi.ServiceProvider;

        Assert.NotNull(dv.GetRequiredService<TaoPhongThietBiHandler>());
        Assert.NotNull(dv.GetRequiredService<SuaPhongThietBiHandler>());
        Assert.NotNull(dv.GetRequiredService<IValidator<PhongThietBiRequest>>());
    }

    private static PhongKhongKetNoiDbContext TaoDbContext()
    {
        var options = new DbContextOptionsBuilder<PhongKhongKetNoiDbContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
            .Options;
        return new PhongKhongKetNoiDbContext(options);
    }

    private sealed class PhongKhongKetNoiDbContext(DbContextOptions<PhongKhongKetNoiDbContext> options) : DbContext(options)
    {
    }
}
