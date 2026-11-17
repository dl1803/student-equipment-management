using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using QuanLyMuonTra.IntegrationTests.Database;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public sealed class TraCuuHttpTests : IAsyncLifetime
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private const string NguoiPhatHanh = "quan-ly-muon-tra-d05v";
    private const string DoiTuongNhan = "quan-ly-muon-tra-d05v";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(CanKetNoiTestFactAttribute.TenBienMoiTruong)))
        {
            return Task.CompletedTask;
        }

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", KetNoiKiemThu.LayChuoiKetNoi());
                builder.UseSetting("Jwt:Secret", BiMatJwt);
                builder.UseSetting("Jwt:Issuer", NguoiPhatHanh);
                builder.UseSetting("Jwt:Audience", DoiTuongNhan);
                builder.UseSetting("Jwt:ExpiryMinutes", "60");
                builder.UseSetting("Jwt:RefreshTokenDays", "7");
            });
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    private static string TaoToken(string dinhDanh, string vaiTro)
    {
        var khoa = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(BiMatJwt));
        var thongTinKy = new SigningCredentials(khoa, SecurityAlgorithms.HmacSha256);
        var bayGio = DateTime.UtcNow;
        var cacClaim = new[]
        {
            new Claim("sub", dinhDanh),
            new Claim("role", vaiTro),
            new Claim("jti", Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(NguoiPhatHanh, DoiTuongNhan, cacClaim, bayGio, bayGio.AddMinutes(15), thongTinKy);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private Task<HttpResponseMessage> GoiAsync(string duongDan, string? token)
    {
        var yeuCau = new HttpRequestMessage(HttpMethod.Get, duongDan);
        if (token is not null)
        {
            yeuCau.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return _client!.SendAsync(yeuCau);
    }

    private static async Task<JsonElement> DocJsonAsync(HttpResponseMessage phanHoi)
    {
        var noiDung = await phanHoi.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(noiDung, JsonOptions);
    }

    [CanKetNoiTestFact]
    public async Task KhongCoToken_MoiEndpointTra401()
    {
        string[] duongDan =
        [
            "/api/v1/sv/tra-cuu/thiet-bi",
            $"/api/v1/sv/tra-cuu/thiet-bi/{Guid.NewGuid()}?maPhong={Guid.NewGuid()}",
            "/api/v1/sv/phong-thiet-bi",
            "/api/v1/sv/loai-thiet-bi",
            "/api/v1/quay/loai-thiet-bi"
        ];

        foreach (var d in duongDan)
        {
            var phanHoi = await GoiAsync(d, null);

            Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
            Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_GoiEndpointQuay_Tra403KhongDuQuyen()
    {
        var phanHoi = await GoiAsync("/api/v1/quay/loai-thiet-bi", TaoToken("D05-SV-A", "SINH_VIEN"));

        Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
        Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task TokenNhanVien_GoiEndpointSinhVien_Tra403KhongDuQuyen()
    {
        var phanHoi = await GoiAsync("/api/v1/sv/phong-thiet-bi", TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN"));

        Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
        Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_DsPhong_Tra200DungHinhDangPhanTrang()
    {
        var phanHoi = await GoiAsync("/api/v1/sv/phong-thiet-bi?trang=1&kichThuoc=1", TaoToken("D05-SV-A", "SINH_VIEN"));

        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal(1, json.GetProperty("trang").GetInt32());
        Assert.Equal(1, json.GetProperty("kichThuoc").GetInt32());
        Assert.True(json.GetProperty("tongSo").GetInt32() >= 0);
        Assert.Equal(JsonValueKind.Array, json.GetProperty("danhSach").ValueKind);
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_DsLoaiVaTraCuu_Tra200()
    {
        var token = TaoToken("D05-SV-A", "SINH_VIEN");

        var dsLoai = await GoiAsync("/api/v1/sv/loai-thiet-bi", token);
        var traCuu = await GoiAsync("/api/v1/sv/tra-cuu/thiet-bi?kichThuoc=5", token);

        Assert.Equal(HttpStatusCode.OK, dsLoai.StatusCode);
        Assert.Equal(HttpStatusCode.OK, traCuu.StatusCode);
        Assert.Equal(JsonValueKind.Array, (await DocJsonAsync(traCuu)).GetProperty("danhSach").ValueKind);
    }

    [CanKetNoiTestFact]
    public async Task ThamSoSai_Tra400DuLieuKhongHopLe()
    {
        var token = TaoToken("D05-SV-A", "SINH_VIEN");
        string[] duongDan =
        [
            "/api/v1/sv/tra-cuu/thiet-bi?trang=0",
            "/api/v1/sv/tra-cuu/thiet-bi?kichThuoc=101",
            "/api/v1/sv/tra-cuu/thiet-bi?trang=abc",
            "/api/v1/sv/tra-cuu/thiet-bi?maLoai=khong-phai-guid",
            "/api/v1/sv/tra-cuu/thiet-bi?conHang=abc",
            $"/api/v1/sv/tra-cuu/thiet-bi/{Guid.NewGuid()}",
            "/api/v1/sv/tra-cuu/thiet-bi/khong-phai-guid?maPhong=" + Guid.NewGuid(),
            "/api/v1/sv/phong-thiet-bi?kichThuoc=0",
            "/api/v1/sv/loai-thiet-bi?trang=-1"
        ];

        foreach (var d in duongDan)
        {
            var phanHoi = await GoiAsync(d, token);

            Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
            Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task ChiTietCapKhongTonTai_Tra404KhongTimThay()
    {
        var phanHoi = await GoiAsync(
            $"/api/v1/sv/tra-cuu/thiet-bi/{Guid.NewGuid()}?maPhong={Guid.NewGuid()}",
            TaoToken("D05-SV-A", "SINH_VIEN"));

        Assert.Equal(HttpStatusCode.NotFound, phanHoi.StatusCode);
        Assert.Equal("KHONG_TIM_THAY", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }
}
