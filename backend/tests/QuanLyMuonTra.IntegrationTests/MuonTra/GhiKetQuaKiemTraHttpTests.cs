using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public sealed class GhiKetQuaKiemTraHttpTests : IAsyncLifetime
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private const string NguoiPhatHanh = "quan-ly-muon-tra-d15v";
    private const string DoiTuongNhan = "quan-ly-muon-tra-d15v";
    private const string DuongDan = "/api/v1/quay/phieu-muon/PM-261126-999999/ket-qua-kiem-tra";
    private const string BodyHopLe = """{"dsKetQua":[{"maThietBi":"D15-TB-KHONG-CO","ketQua":"NGUYEN_VEN"}]}""";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public Task InitializeAsync()
    {
        try
        {
            _ = KetNoiKiemThu.LayChuoiKetNoi();
        }
        catch
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
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<ITrangThaiTaiKhoanReader, TrangThaiTaiKhoanGia>();
                });
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

    private Task<HttpResponseMessage> GoiAsync(string body, string? token)
    {
        var yeuCau = new HttpRequestMessage(HttpMethod.Post, DuongDan)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
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
    public async Task UC08_GhiKetQua_KhongCoToken_Tra401()
    {
        var phanHoi = await GoiAsync(BodyHopLe, null);

        Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
        Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_TokenSinhVien_Tra403KhongDuQuyen()
    {
        var phanHoi = await GoiAsync(BodyHopLe, TaoToken("D14-SV-A", "SINH_VIEN"));

        Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
        Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_DanhSachRongHoacKetQuaLa_Tra400()
    {
        var token = TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN");

        var rong = await GoiAsync("""{"dsKetQua":[]}""", token);
        var ketQuaLa = await GoiAsync("""{"dsKetQua":[{"maThietBi":"X","ketQua":"HONG_NHE"}]}""", token);
        var khungVoiNguyenVen = await GoiAsync(
            """{"dsKetQua":[{"maThietBi":"X","ketQua":"NGUYEN_VEN","maKhungPhat":"019c0000-0000-7000-8000-0000000d14d1"}]}""",
            token);

        foreach (var phanHoi in new[] { rong, ketQuaLa, khungVoiNguyenVen })
        {
            Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
            Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_TokenNhanVien_PhieuKhongTonTai_Tra404QuaHostThat()
    {
        var phanHoi = await GoiAsync(BodyHopLe, TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN"));

        Assert.Equal(HttpStatusCode.NotFound, phanHoi.StatusCode);
        Assert.Equal("KHONG_TIM_THAY", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    private sealed class TrangThaiTaiKhoanGia : ITrangThaiTaiKhoanReader
    {
        public Task<string?> LayTrangThaiKhoaAsync(Guid maNguoiDung, CancellationToken ct)
        {
            return Task.FromResult<string?>("HOAT_DONG");
        }
    }
}
