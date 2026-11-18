using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Application;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;

namespace QuanLyMuonTra.UnitTests.QuyDinh;

public sealed class ThamSoUngDungTests
{
    private static readonly JsonSerializerOptions TuyChonJson = new(JsonSerializerDefaults.Web);

    private const string ThamSoDayDu =
        """{"soNgayKhoa":3,"thoiGianGiuChoPhut":30,"anHanPhut":15,"soGioGiaHanToiDa":2,"nguongViPham":3,"soLanDangNhapSaiToiDa":5,"gioMoCua":"07:30","gioDongCua":"17:00:00"}""";

    [Fact]
    public void UC13_Json_ThamSoDocDuocGioHHmmVaHHmmss()
    {
        var yeuCau = JsonSerializer.Deserialize<CapNhatThamSoRequest>(ThamSoDayDu, TuyChonJson);

        Assert.Equal(new CapNhatThamSoRequest(3, 30, 15, 2, 3, 5, new TimeOnly(7, 30), new TimeOnly(17, 0)), yeuCau);
    }

    [Fact]
    public void UC13_Json_ThamSoGioNullChoPhep()
    {
        var json = ThamSoDayDu.Replace("\"07:30\"", "null").Replace("\"17:00:00\"", "null");

        var yeuCau = JsonSerializer.Deserialize<CapNhatThamSoRequest>(json, TuyChonJson);

        Assert.NotNull(yeuCau);
        Assert.Null(yeuCau.GioMoCua);
        Assert.Null(yeuCau.GioDongCua);
    }

    [Theory]
    [InlineData("\"soNgayKhoa\":3,")]
    [InlineData(",\"gioMoCua\":\"07:30\"")]
    [InlineData(",\"gioDongCua\":\"17:00:00\"")]
    public void UC13_Json_ThamSoThieuTruongBatBuoc_NemJsonException(string doanBo)
    {
        var json = ThamSoDayDu.Replace(doanBo, string.Empty);

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CapNhatThamSoRequest>(json, TuyChonJson));
    }

    [Theory]
    [InlineData("7:30")]
    [InlineData("24:00")]
    [InlineData("07:60")]
    [InlineData("07:30:00.5")]
    [InlineData("abc")]
    public void UC13_Json_ThamSoGioSaiDinhDang_NemJsonException(string gio)
    {
        var json = ThamSoDayDu.Replace("\"07:30\"", $"\"{gio}\"");

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CapNhatThamSoRequest>(json, TuyChonJson));
    }

    [Fact]
    public void UC13_Json_PhanHoiThamSoGhiGioDangHHmmss()
    {
        var dto = new ThamSoHeThongDto(3, 30, 15, 2, 3, 5, new TimeOnly(7, 30), null);

        var json = JsonSerializer.Serialize(dto, TuyChonJson);

        Assert.Contains("\"gioMoCua\":\"07:30:00\"", json);
        Assert.Contains("\"gioDongCua\":null", json);
    }

    [Fact]
    public void UC13_DangKyModuleQuyDinh_HandlerThamSoGiaiQuyetDuoc()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<ThamSoKhongKetNoiDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<ThamSoKhongKetNoiDbContext>())
            .AddQuyDinhModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();

        Assert.NotNull(phamVi.ServiceProvider.GetRequiredService<LayThamSoHandler>());
        Assert.NotNull(phamVi.ServiceProvider.GetRequiredService<CapNhatThamSoHandler>());
    }

    private sealed class ThamSoKhongKetNoiDbContext(DbContextOptions<ThamSoKhongKetNoiDbContext> options) : DbContext(options)
    {
    }
}
