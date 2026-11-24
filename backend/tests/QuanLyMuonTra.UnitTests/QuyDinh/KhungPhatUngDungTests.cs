using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Application;
using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.QuyDinh;

public sealed class KhungPhatUngDungTests
{
    private static readonly JsonSerializerOptions TuyChonJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public void UC13_Json_KhungPhatDocTyLeThapPhan()
    {
        var yeuCau = JsonSerializer.Deserialize<KhungPhatRequest>("""{"tenKhung":"Thử","tyLePhanTram":12.5}""", TuyChonJson);

        Assert.Equal(new KhungPhatRequest("Thử", 12.5m), yeuCau);
    }

    [Theory]
    [InlineData("""{"tenKhung":"Thử"}""")]
    [InlineData("""{"tyLePhanTram":12.5}""")]
    public void UC13_Json_KhungPhatThieuTruong_NemJsonException(string json)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<KhungPhatRequest>(json, TuyChonJson));
    }

    [Theory]
    [InlineData("Nhẹ", true)]
    [InlineData("   ", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void UC13_KiemTraKhungPhat_TenRongLaLoiCauTruc(string? ten, bool hopLe)
    {
        var ketQua = new KhungPhatRequestValidator().Validate(new KhungPhatRequest(ten!, 15m));

        Assert.Equal(hopLe, ketQua.IsValid);
    }

    [Fact]
    public void UC13_KiemTraKhungPhat_TyLeNgoaiKhoangKhongThuocValidator()
    {
        var ketQua = new KhungPhatRequestValidator().Validate(new KhungPhatRequest("Khung", 150m));

        Assert.True(ketQua.IsValid);
    }

    [Fact]
    public void UC13_TatKhungPhat_GoiHaiLan_VanTat()
    {
        var khung = new KhungPhat("Nhẹ", 15m);

        khung.Tat();
        khung.Tat();

        Assert.False(khung.DangSuDung);
    }

    [Fact]
    public async Task UC13_TaoKhungPhat_TyLeNgoaiKhoang_Nem422TruocKhiTruyCapCsdl()
    {
        using var db = TaoDbContext();
        var handler = new TaoKhungPhatHandler(db);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(new KhungPhatRequest("Khung", 150m), CancellationToken.None));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task UC13_DsKhungPhat_PhanTrangSai_Nem400TruocKhiTruyCapCsdl(int trang, int kichThuoc)
    {
        using var db = TaoDbContext();
        var handler = new DsKhungPhatHandler(db);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(true, trang, kichThuoc, CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public async Task UC13_XoaKhungPhat_KhongCoProviderThamChieu_TuChoiAnToan()
    {
        using var db = TaoDbContext();
        var handler = new XoaKhungPhatHandler(db, []);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.XuLyAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public void UC13_DangKyModuleQuyDinh_HandlerKhungPhatVaValidatorGiaiQuyetDuoc()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<KhungPhatKhongKetNoiDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<KhungPhatKhongKetNoiDbContext>())
            .AddQuyDinhModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();
        var dv = phamVi.ServiceProvider;

        Assert.NotNull(dv.GetRequiredService<DsKhungPhatHandler>());
        Assert.NotNull(dv.GetRequiredService<TaoKhungPhatHandler>());
        Assert.NotNull(dv.GetRequiredService<SuaKhungPhatHandler>());
        Assert.NotNull(dv.GetRequiredService<TatKhungPhatHandler>());
        Assert.NotNull(dv.GetRequiredService<XoaKhungPhatHandler>());
        Assert.NotNull(dv.GetRequiredService<IValidator<KhungPhatRequest>>());
    }

    private static KhungPhatKhongKetNoiDbContext TaoDbContext()
    {
        var options = new DbContextOptionsBuilder<KhungPhatKhongKetNoiDbContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
            .Options;
        return new KhungPhatKhongKetNoiDbContext(options);
    }

    private sealed class KhungPhatKhongKetNoiDbContext(DbContextOptions<KhungPhatKhongKetNoiDbContext> options) : DbContext(options)
    {
    }
}
