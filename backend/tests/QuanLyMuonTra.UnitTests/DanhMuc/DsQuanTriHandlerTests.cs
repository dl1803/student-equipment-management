using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class DsQuanTriHandlerTests
{
    private static readonly JsonSerializerOptions TuyChonJson = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(1, -5)]
    public async Task UC12_DsQuanTri_TrangHoacKichThuocNgoaiKhoang_Nem400TruocKhiTruyCapCsdl(int trang, int kichThuoc)
    {
        using var db = TaoDbContext();
        var ct = CancellationToken.None;
        var cacLenh = new Func<Task>[]
        {
            () => new DsPhongThietBiHandler(db).XuLyAsync(trang, kichThuoc, ct),
            () => new DsLoaiThietBiHandler(db).XuLyAsync(null, trang, kichThuoc, ct),
            () => new DsThietBiHandler(db, new AnhServiceGia()).XuLyAsync(null, null, null, null, trang, kichThuoc, ct),
            () => new DsNhaCungCapHandler(db).XuLyAsync(trang, kichThuoc, ct),
        };

        foreach (var lenh in cacLenh)
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(lenh);

            Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(400, loi.MaHttp);
        }
    }

    [Fact]
    public async Task UC12_DsThietBi_TinhTrangKhongThuocNamMa_Nem400()
    {
        using var db = TaoDbContext();
        var handler = new DsThietBiHandler(db, new AnhServiceGia());

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(null, null, null, "KHONG_CO", 1, 20, CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public void UC12_DangKyModuleDanhMuc_BonHandlerDanhSachGiaiQuyetDuoc()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<KhongKetNoiDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<KhongKetNoiDbContext>())
            .AddScoped<IAnhService, AnhServiceGia>()
            .AddDanhMucModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();
        var dv = phamVi.ServiceProvider;

        Assert.NotNull(dv.GetRequiredService<DsPhongThietBiHandler>());
        Assert.NotNull(dv.GetRequiredService<DsLoaiThietBiHandler>());
        Assert.NotNull(dv.GetRequiredService<DsThietBiHandler>());
        Assert.NotNull(dv.GetRequiredService<DsNhaCungCapHandler>());
    }

    [Fact]
    public void UC12_Json_ThietBiQuanTriCoDsAnhVoiMaAnhVaUrl()
    {
        var maAnh = Guid.NewGuid();
        var thietBi = new ThietBiQuanTriDto(
            "TB-01",
            Guid.Empty,
            "Loai",
            Guid.Empty,
            "Phong",
            "Ten",
            null,
            null,
            new DateOnly(2026, 1, 5),
            null,
            "SAN_SANG",
            null,
            null,
            false,
            1000,
            [],
            [new AnhRefDto(maAnh, "https://luu-tru-gia.test/a.jpg")]);

        var json = JsonSerializer.Serialize(thietBi, TuyChonJson);
        using var tai = JsonDocument.Parse(json);

        var anh = Assert.Single(tai.RootElement.GetProperty("dsAnh").EnumerateArray());
        Assert.Equal(maAnh, anh.GetProperty("maAnh").GetGuid());
        Assert.Equal("https://luu-tru-gia.test/a.jpg", anh.GetProperty("url").GetString());
    }

    [Fact]
    public void UC12_Json_NhaCungCapGiuNccVietHoa()
    {
        var json = JsonSerializer.Serialize(new NhaCungCapQuanTriDto(Guid.Empty, "A", null), TuyChonJson);
        using var tai = JsonDocument.Parse(json);

        var cacTen = tai.RootElement.EnumerateObject().Select(p => p.Name).ToArray();

        Assert.Equal(new[] { "maNCC", "tenNCC", "thongTinLienHe" }, cacTen);
    }

    [Fact]
    public void UC12_Json_KetQuaPhanTrangDungTenTruongOpenApi()
    {
        var json = JsonSerializer.Serialize(new KetQuaPhanTrang<int>(0, 1, 20, []), TuyChonJson);
        using var tai = JsonDocument.Parse(json);

        var cacTen = tai.RootElement.EnumerateObject().Select(p => p.Name).ToArray();

        Assert.Equal(new[] { "tongSo", "trang", "kichThuoc", "danhSach" }, cacTen);
    }

    private static KhongKetNoiDbContext TaoDbContext()
    {
        var options = new DbContextOptionsBuilder<KhongKetNoiDbContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
            .Options;
        return new KhongKetNoiDbContext(options);
    }

    private sealed class KhongKetNoiDbContext(DbContextOptions<KhongKetNoiDbContext> options) : DbContext(options)
    {
    }
}
