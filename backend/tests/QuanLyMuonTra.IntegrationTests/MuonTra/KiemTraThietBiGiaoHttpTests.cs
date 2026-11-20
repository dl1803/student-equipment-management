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

public sealed class KiemTraThietBiGiaoHttpTests : IAsyncLifetime
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private const string NguoiPhatHanh = "quan-ly-muon-tra-d08v";
    private const string DoiTuongNhan = "quan-ly-muon-tra-d08v";
    private const string DuongDan = "/api/v1/quay/phieu-muon/kiem-tra-thiet-bi";
    private const string NoiDung = """
        {"maThietBi":"D08-TB-01","maPhong":"019c0000-0000-7000-8000-0000000d8b01","maLoai":"019c0000-0000-7000-8000-0000000d8c01"}
        """;
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

    private Task<HttpResponseMessage> GoiAsync(string? token)
    {
        var yeuCau = new HttpRequestMessage(HttpMethod.Post, DuongDan)
        {
            Content = new StringContent(NoiDung, Encoding.UTF8, "application/json")
        };
        if (token is not null)
        {
            yeuCau.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return _client!.SendAsync(yeuCau);
    }

    private static async Task<string?> LayMaLoiAsync(HttpResponseMessage phanHoi)
    {
        var noiDung = await phanHoi.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(noiDung, JsonOptions).GetProperty("maLoi").GetString();
    }

    [CanKetNoiTestFact]
    public async Task KhongCoToken_Tra401()
    {
        var phanHoi = await GoiAsync(null);

        Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
        Assert.Equal("CHUA_XAC_THUC", await LayMaLoiAsync(phanHoi));
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_Tra403KhongDuQuyen()
    {
        var phanHoi = await GoiAsync(TaoToken("D08-SV-A", "SINH_VIEN"));

        Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
        Assert.Equal("KHONG_DU_QUYEN", await LayMaLoiAsync(phanHoi));
    }
}
