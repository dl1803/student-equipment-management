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

public sealed class DuyetTuChoiYeuCauHttpTests : IAsyncLifetime
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private const string NguoiPhatHanh = "quan-ly-muon-tra-d07v";
    private const string DoiTuongNhan = "quan-ly-muon-tra-d07v";
    private const string GocQuay = "/api/v1/quay/phieu-yeu-cau-muon";
    private const string MaPhieu = "YC-261118-997001";
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

    private Task<HttpResponseMessage> GoiAsync(HttpMethod phuongThuc, string duongDan, string? token, string? json = null)
    {
        var yeuCau = new HttpRequestMessage(phuongThuc, duongDan);
        if (token is not null)
        {
            yeuCau.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (json is not null)
        {
            yeuCau.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return _client!.SendAsync(yeuCau);
    }

    private static async Task<JsonElement> DocJsonAsync(HttpResponseMessage phanHoi)
    {
        var noiDung = await phanHoi.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(noiDung, JsonOptions);
    }

    private Task<HttpResponseMessage>[] GoiBonEndpointAsync(string? token)
    {
        return
        [
            GoiAsync(HttpMethod.Get, GocQuay, token),
            GoiAsync(HttpMethod.Get, $"{GocQuay}/{MaPhieu}/kiem-tra", token),
            GoiAsync(HttpMethod.Post, $"{GocQuay}/{MaPhieu}/duyet", token),
            GoiAsync(HttpMethod.Post, $"{GocQuay}/{MaPhieu}/tu-choi", token, """{"lyDo":"Khong du dieu kien"}""")
        ];
    }

    [CanKetNoiTestFact]
    public async Task KhongCoToken_BonEndpointTra401()
    {
        foreach (var phanHoi in await Task.WhenAll(GoiBonEndpointAsync(null)))
        {
            Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
            Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_BonEndpointTra403KhongDuQuyen()
    {
        var token = TaoToken("D07-SV-A", "SINH_VIEN");

        foreach (var phanHoi in await Task.WhenAll(GoiBonEndpointAsync(token)))
        {
            Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
            Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }
}
