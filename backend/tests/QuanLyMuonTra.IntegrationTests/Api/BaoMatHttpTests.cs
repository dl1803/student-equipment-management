using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using QuanLyMuonTra.Api.BaoMat;

namespace QuanLyMuonTra.IntegrationTests.Api;

public sealed class BaoMatHttpTests
{
    private const string DuongDanDangNhapNhanVien = "/api/v1/auth/nhan-vien/dang-nhap";

    private static WebApplicationFactory<Program> TaoFactory(params (string Khoa, string GiaTri)[] cauHinhThem) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=khong_ket_noi");
            builder.UseSetting("Jwt:Secret", "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len");
            builder.UseSetting("Jwt:Issuer", "quan-ly-muon-tra-d07k");
            builder.UseSetting("Jwt:Audience", "quan-ly-muon-tra-d07k");
            foreach (var (khoa, giaTri) in cauHinhThem)
            {
                builder.UseSetting(khoa, giaTri);
            }
        });

    private static IConfiguration TaoCauHinh(params (string Khoa, string GiaTri)[] cacGiaTri) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(cacGiaTri.Select(x => new KeyValuePair<string, string?>(x.Khoa, x.GiaTri)))
            .Build();

    private static Task<HttpResponseMessage> GuiPreflightAsync(HttpClient client, string mien)
    {
        var yeuCau = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/toi");
        yeuCau.Headers.Add("Origin", mien);
        yeuCau.Headers.Add("Access-Control-Request-Method", "GET");

        return client.SendAsync(yeuCau);
    }

    private static Task<HttpResponseMessage> DangNhapRongAsync(HttpClient client) =>
        client.PostAsJsonAsync(DuongDanDangNhapNhanVien, new { tenDangNhap = string.Empty, matKhau = string.Empty });

    [Fact]
    public void NFR05_DocDsMienChoPhep_ChuanHoaVaBoTrung()
    {
        var cauHinh = TaoCauHinh(("Cors:AllowedOrigins", "https://a.example.test/, https://B.example.test ,https://a.example.test"));

        var dsMien = CauHinhBaoMat.DocDsMienChoPhep(cauHinh);

        Assert.Equal(new[] { "https://a.example.test", "https://B.example.test" }, dsMien);
    }

    [Fact]
    public void NFR05_DocDsMienChoPhep_ThieuCauHinh_TraRong_VaDauSaoBiTuChoi()
    {
        Assert.Empty(CauHinhBaoMat.DocDsMienChoPhep(TaoCauHinh()));
        Assert.Throws<InvalidOperationException>(
            () => CauHinhBaoMat.DocDsMienChoPhep(TaoCauHinh(("Cors:AllowedOrigins", "https://a.example.test,*"))));
    }

    [Fact]
    public void NFR05_DocGioiHanTanSuatAuth_ThieuCaHai_Null_ChiMotHoacSai_Loi_DayDu_DocDuoc()
    {
        Assert.Null(CauHinhBaoMat.DocGioiHanTanSuatAuth(TaoCauHinh()));
        Assert.Throws<InvalidOperationException>(
            () => CauHinhBaoMat.DocGioiHanTanSuatAuth(TaoCauHinh(("RateLimitAuth:SoYeuCau", "5"))));
        Assert.Throws<InvalidOperationException>(
            () => CauHinhBaoMat.DocGioiHanTanSuatAuth(TaoCauHinh(("RateLimitAuth:SoYeuCau", "0"), ("RateLimitAuth:CuaSoGiay", "60"))));

        var gioiHan = CauHinhBaoMat.DocGioiHanTanSuatAuth(
            TaoCauHinh(("RateLimitAuth:SoYeuCau", "5"), ("RateLimitAuth:CuaSoGiay", "60")));

        Assert.NotNull(gioiHan);
        Assert.Equal(5, gioiHan.SoYeuCau);
        Assert.Equal(TimeSpan.FromSeconds(60), gioiHan.CuaSo);
    }

    [Fact]
    public async Task NFR05_Cors_MienTrongDanhSachDuocPhep_MienLaBiTuChoi()
    {
        await using var factory = TaoFactory(("Cors:AllowedOrigins", "https://web.example.test, https://xem.example.test/"));
        using var client = factory.CreateClient();

        var chapNhan = await GuiPreflightAsync(client, "https://xem.example.test");
        var tuChoi = await GuiPreflightAsync(client, "https://khac.example.test");

        Assert.Equal("https://xem.example.test", Assert.Single(chapNhan.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.False(tuChoi.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task NFR05_Cors_ChuaCauHinh_KhongMienNaoDuocPhep()
    {
        await using var factory = TaoFactory();
        using var client = factory.CreateClient();

        var phanHoi = await GuiPreflightAsync(client, "https://web.example.test");

        Assert.False(phanHoi.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task NFR05_GioiHanTanSuatAuth_VuotNguong_Tra429QuaNhieuYeuCau_VaKhongAnhHuongDuongDanKhac()
    {
        await using var factory = TaoFactory(("RateLimitAuth:SoYeuCau", "2"), ("RateLimitAuth:CuaSoGiay", "60"));
        using var client = factory.CreateClient();

        var lan1 = await DangNhapRongAsync(client);
        var lan2 = await DangNhapRongAsync(client);
        var lan3 = await DangNhapRongAsync(client);
        var suKhoe = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.BadRequest, lan1.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, lan2.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, lan3.StatusCode);
        var json = await lan3.Content.ReadFromJsonAsync<JsonElement>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal("QUA_NHIEU_YEU_CAU", json.GetProperty("maLoi").GetString());
        Assert.Equal(HttpStatusCode.OK, suKhoe.StatusCode);
    }

    [Fact]
    public async Task NFR05_GioiHanTanSuatAuth_KhongChanAuthToi_KhiVuotNguongDangNhap()
    {
        await using var factory = TaoFactory(("RateLimitAuth:SoYeuCau", "1"), ("RateLimitAuth:CuaSoGiay", "60"));
        using var client = factory.CreateClient();

        var lan1 = await DangNhapRongAsync(client);
        var lan2 = await DangNhapRongAsync(client);
        Assert.Equal(HttpStatusCode.BadRequest, lan1.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, lan2.StatusCode);

        var authToi = await client.GetAsync("/api/v1/auth/toi");
        Assert.Equal(HttpStatusCode.Unauthorized, authToi.StatusCode);
    }

    [Fact]
    public async Task NFR05_Cors_MoiTruongDevelopment_ChoPhepLocalDev()
    {
        await using var factory = TaoFactory();
        using var client = factory.CreateClient();

        var chapNhan3000 = await GuiPreflightAsync(client, "http://localhost:3000");
        var chapNhan5173 = await GuiPreflightAsync(client, "http://localhost:5173");

        Assert.Equal("http://localhost:3000", Assert.Single(chapNhan3000.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Equal("http://localhost:5173", Assert.Single(chapNhan5173.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Fact]
    public async Task NFR05_GioiHanTanSuatAuth_ChuaCauHinh_KhongGioiHan()
    {
        await using var factory = TaoFactory();
        using var client = factory.CreateClient();

        for (var i = 0; i < 5; i++)
        {
            var phanHoi = await DangNhapRongAsync(client);

            Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        }
    }

    [Fact]
    public async Task NFR05_TieuDeBaoMat_CoNosniffTrenMoiPhanHoi()
    {
        await using var factory = TaoFactory();
        using var client = factory.CreateClient();

        var phanHoi = await client.GetAsync("/health");

        Assert.Equal("nosniff", Assert.Single(phanHoi.Headers.GetValues("X-Content-Type-Options")));
    }
}

