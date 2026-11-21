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
using QuanLyMuonTra.Infrastructure.Email;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.ThongBao;

public sealed class ThongBaoHttpTests : IAsyncLifetime
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private const string NguoiPhatHanh = "quan-ly-muon-tra-d09k";
    private const string DoiTuongNhan = "quan-ly-muon-tra-d09k";
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
    public async Task KhongCoToken_Tra401ChuaXacThuc()
    {
        var phanHoi = await GoiAsync("/api/v1/sv/thong-bao", null);

        Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
        Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task TokenNhanVien_Tra403KhongDuQuyen()
    {
        var phanHoi = await GoiAsync("/api/v1/sv/thong-bao", TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN"));

        Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
        Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_ChuaCoThongBao_Tra200DungHinhDangPhanTrang()
    {
        var phanHoi = await GoiAsync("/api/v1/sv/thong-bao?trang=1&kichThuoc=5", TaoToken($"D09K-SV-{Guid.NewGuid():N}", "SINH_VIEN"));

        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal(0, json.GetProperty("tongSo").GetInt32());
        Assert.Equal(1, json.GetProperty("trang").GetInt32());
        Assert.Equal(5, json.GetProperty("kichThuoc").GetInt32());
        Assert.Equal(JsonValueKind.Array, json.GetProperty("danhSach").ValueKind);
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_PhanTrangSai_Tra400()
    {
        var token = TaoToken($"D09K-SV-{Guid.NewGuid():N}", "SINH_VIEN");

        var trangKhong = await GoiAsync("/api/v1/sv/thong-bao?trang=0", token);
        var kichThuocLon = await GoiAsync("/api/v1/sv/thong-bao?kichThuoc=101", token);

        Assert.Equal(HttpStatusCode.BadRequest, trangKhong.StatusCode);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(trangKhong)).GetProperty("maLoi").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, kichThuocLon.StatusCode);
    }

    [CanKetNoiTestFact]
    public Task Host_DangKyDayDuOutboxVaBoGuiEmailMacDinhLaLog()
    {
        using var phamVi = _factory!.Services.CreateScope();

        Assert.NotNull(phamVi.ServiceProvider.GetRequiredService<IThongBaoOutbox>());
        Assert.IsType<LogEmailSender>(phamVi.ServiceProvider.GetRequiredService<IEmailSender>());

        return Task.CompletedTask;
    }
}
