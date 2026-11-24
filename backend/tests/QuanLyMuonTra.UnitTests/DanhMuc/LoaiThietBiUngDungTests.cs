using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class LoaiThietBiUngDungTests
{
    private static readonly JsonSerializerOptions TuyChonJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public void UC12_KiemTraTaoLoai_DuLieuHopLe_Dat()
    {
        var ketQua = new LoaiThietBiTaoMoiRequestValidator()
            .Validate(new LoaiThietBiTaoMoiRequest("Laptop", null, null, 180, 50000));

        Assert.True(ketQua.IsValid);
    }

    [Fact]
    public void UC12_KiemTraTaoLoai_PhiBangKhong_Dat()
    {
        var ketQua = new LoaiThietBiTaoMoiRequestValidator()
            .Validate(new LoaiThietBiTaoMoiRequest("Cáp", null, null, 30, 0));

        Assert.True(ketQua.IsValid);
    }

    [Theory]
    [InlineData("", 30, 0, "TenLoai")]
    [InlineData("   ", 30, 0, "TenLoai")]
    [InlineData("Laptop", 0, 0, "ChuKyBaoTri")]
    [InlineData("Laptop", -1, 0, "ChuKyBaoTri")]
    [InlineData("Laptop", 30, -1, "PhiTreMoiNgay")]
    public void UC12_KiemTraTaoLoai_DuLieuSai_BaoLoiDungTruong(string tenLoai, int chuKy, long phi, string truongLoi)
    {
        var ketQua = new LoaiThietBiTaoMoiRequestValidator()
            .Validate(new LoaiThietBiTaoMoiRequest(tenLoai, null, null, chuKy, phi));

        Assert.False(ketQua.IsValid);
        Assert.Contains(ketQua.Errors, l => l.PropertyName == truongLoi);
    }

    [Fact]
    public void UC12_KiemTraSuaLoai_DuLieuHopLe_Dat()
    {
        var ketQua = new LoaiThietBiCapNhatRequestValidator()
            .Validate(new LoaiThietBiCapNhatRequest("Laptop", "Mô tả", "HP", 90));

        Assert.True(ketQua.IsValid);
    }

    [Theory]
    [InlineData("", 30, "TenLoai")]
    [InlineData("Laptop", 0, "ChuKyBaoTri")]
    [InlineData("Laptop", -5, "ChuKyBaoTri")]
    public void UC12_KiemTraSuaLoai_DuLieuSai_BaoLoiDungTruong(string tenLoai, int chuKy, string truongLoi)
    {
        var ketQua = new LoaiThietBiCapNhatRequestValidator()
            .Validate(new LoaiThietBiCapNhatRequest(tenLoai, null, null, chuKy));

        Assert.False(ketQua.IsValid);
        Assert.Contains(ketQua.Errors, l => l.PropertyName == truongLoi);
    }

    [Fact]
    public async Task UC12_TaoLoai_DuLieuSai_Nem400TruocKhiTruyCapCsdl()
    {
        using var db = TaoDbContext();
        var handler = new TaoLoaiThietBiHandler(db);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(new LoaiThietBiTaoMoiRequest("Laptop", null, null, 0, 0), CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public void UC12_Json_YeuCauTaoLoaiDocDuocTheoTenTruongOpenApi()
    {
        var yeuCau = JsonSerializer.Deserialize<LoaiThietBiTaoMoiRequest>(
            """{"tenLoai":"Laptop","moTa":null,"hangSanXuat":"Dell","chuKyBaoTri":180,"phiTreMoiNgay":50000}""",
            TuyChonJson);

        Assert.Equal(new LoaiThietBiTaoMoiRequest("Laptop", null, "Dell", 180, 50000), yeuCau);
    }

    [Theory]
    [InlineData("""{"tenLoai":"Laptop","chuKyBaoTri":180}""")]
    [InlineData("""{"tenLoai":"Laptop","moTa":null,"hangSanXuat":null,"chuKyBaoTri":180}""")]
    public void UC12_Json_TaoLoaiThieuPhiTreMoiNgay_NemJsonException(string json)
    {
        Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<LoaiThietBiTaoMoiRequest>(json, TuyChonJson));
    }

    [Fact]
    public void UC12_Json_PhanHoiLoaiDungTenTruongOpenApi()
    {
        var json = JsonSerializer.Serialize(
            new LoaiThietBiQuanTriDto(Guid.Empty, "Laptop", null, null, 180, 50000, false),
            TuyChonJson);
        using var tai = JsonDocument.Parse(json);

        var cacTen = tai.RootElement.EnumerateObject().Select(p => p.Name).ToArray();

        Assert.Equal(
            new[] { "maLoai", "tenLoai", "moTa", "hangSanXuat", "chuKyBaoTri", "phiTreMoiNgay", "ngungSuDung" },
            cacTen);
    }

    [Fact]
    public void UC12_YeuCauLoai_KhongCoPhiKhiSuaVaKhongCoTinhTrangHayNgungSuDung()
    {
        var taoMoi = typeof(LoaiThietBiTaoMoiRequest).GetProperties().Select(p => p.Name).OrderBy(t => t, StringComparer.Ordinal).ToArray();
        var capNhat = typeof(LoaiThietBiCapNhatRequest).GetProperties().Select(p => p.Name).OrderBy(t => t, StringComparer.Ordinal).ToArray();

        Assert.Equal(new[] { "ChuKyBaoTri", "HangSanXuat", "MoTa", "PhiTreMoiNgay", "TenLoai" }, taoMoi);
        Assert.Equal(new[] { "ChuKyBaoTri", "HangSanXuat", "MoTa", "TenLoai" }, capNhat);
    }

    [Fact]
    public void UC12_AnhXaLoaiThietBi_PhiCoPhanThapPhan_Nem400()
    {
        var loai = new LoaiThietBi("Laptop", null, null, 30, 0);
        typeof(LoaiThietBi).GetProperty(nameof(LoaiThietBi.PhiTreMoiNgay))!
            .SetValue(loai, 10.5m);

        var loi = Assert.Throws<LoiNghiepVuException>(() => AnhXaLoaiThietBi.SangDto(loai));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public void UC12_DangKyModuleDanhMuc_HandlerVaValidatorLoaiGiaiQuyetDuoc()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<LoaiKhongKetNoiDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<LoaiKhongKetNoiDbContext>())
            .AddDanhMucModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();
        var dv = phamVi.ServiceProvider;

        Assert.NotNull(dv.GetRequiredService<TaoLoaiThietBiHandler>());
        Assert.NotNull(dv.GetRequiredService<SuaLoaiThietBiHandler>());
        Assert.NotNull(dv.GetRequiredService<IValidator<LoaiThietBiTaoMoiRequest>>());
        Assert.NotNull(dv.GetRequiredService<IValidator<LoaiThietBiCapNhatRequest>>());
    }

    private static LoaiKhongKetNoiDbContext TaoDbContext()
    {
        var options = new DbContextOptionsBuilder<LoaiKhongKetNoiDbContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
            .Options;
        return new LoaiKhongKetNoiDbContext(options);
    }

    private sealed class LoaiKhongKetNoiDbContext(DbContextOptions<LoaiKhongKetNoiDbContext> options) : DbContext(options)
    {
    }
}
