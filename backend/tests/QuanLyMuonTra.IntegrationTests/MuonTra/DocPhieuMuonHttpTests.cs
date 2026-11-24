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

public sealed class DocPhieuMuonHttpTests : IAsyncLifetime
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private const string NguoiPhatHanh = "quan-ly-muon-tra-d11v";
    private const string DoiTuongNhan = "quan-ly-muon-tra-d11v";
    private const string MaPhieu = "PM-261023-997001";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] DuongDanSv =
    [
        "/api/v1/sv/phieu-muon",
        $"/api/v1/sv/phieu-muon/{MaPhieu}"
    ];

    private static readonly string[] DuongDanQuay =
    [
        "/api/v1/quay/phieu-muon",
        $"/api/v1/quay/phieu-muon/{MaPhieu}",
        "/api/v1/quay/phieu-muon/qua-han-30-ngay"
    ];

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
    public async Task KhongCoToken_NamEndpointDocPhieuMuonTra401()
    {
        foreach (var duongDan in DuongDanSv.Concat(DuongDanQuay))
        {
            var phanHoi = await GoiAsync(duongDan, null);

            Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
            Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_GoiEndpointQuay_Tra403KhongDuQuyen()
    {
        var token = TaoToken("D11-SV-A", "SINH_VIEN");

        foreach (var duongDan in DuongDanQuay)
        {
            var phanHoi = await GoiAsync(duongDan, token);

            Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
            Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenNhanVien_GoiEndpointSinhVien_Tra403KhongDuQuyen()
    {
        var token = TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN");

        foreach (var duongDan in DuongDanSv)
        {
            var phanHoi = await GoiAsync(duongDan, token);

            Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
            Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_GoiDsPhieuMuon_Tra200()
    {
        var phanHoi = await GoiAsync("/api/v1/sv/phieu-muon?trang=1&kichThuoc=10", TaoToken("D11-SV-A", "SINH_VIEN"));

        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal(1, json.GetProperty("trang").GetInt32());
        Assert.Equal(10, json.GetProperty("kichThuoc").GetInt32());
        Assert.True(json.GetProperty("tongSo").GetInt32() >= 0);
        Assert.Equal(JsonValueKind.Array, json.GetProperty("danhSach").ValueKind);
    }

    [CanKetNoiTestFact]
    public async Task TokenNhanVien_GoiQuaHan30Ngay_Tra200UuTienTruocMa()
    {
        var phanHoi = await GoiAsync("/api/v1/quay/phieu-muon/qua-han-30-ngay?trang=1&kichThuoc=10", TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN"));

        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal(1, json.GetProperty("trang").GetInt32());
        Assert.Equal(10, json.GetProperty("kichThuoc").GetInt32());
        Assert.True(json.GetProperty("tongSo").GetInt32() >= 0);
        Assert.Equal(JsonValueKind.Array, json.GetProperty("danhSach").ValueKind);
    }

    [CanKetNoiTestFact]
    public async Task TokenNhanVien_GoiDsPhieuMuonQuay_Tra200()
    {
        var phanHoi = await GoiAsync("/api/v1/quay/phieu-muon?trang=1&kichThuoc=10", TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN"));

        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal(1, json.GetProperty("trang").GetInt32());
        Assert.Equal(10, json.GetProperty("kichThuoc").GetInt32());
        Assert.True(json.GetProperty("tongSo").GetInt32() >= 0);
        Assert.Equal(JsonValueKind.Array, json.GetProperty("danhSach").ValueKind);
    }

    private sealed class TrangThaiTaiKhoanGia : ITrangThaiTaiKhoanReader
    {
        public Task<string?> LayTrangThaiKhoaAsync(Guid maNguoiDung, CancellationToken ct)
        {
            return Task.FromResult<string?>("HOAT_DONG");
        }
    }
}
