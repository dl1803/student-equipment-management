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
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;

namespace QuanLyMuonTra.IntegrationTests.ViPham;

public sealed class ViPhamHttpTests : IAsyncLifetime
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private const string NguoiPhatHanh = "quan-ly-muon-tra-d14k";
    private const string DoiTuongNhan = "quan-ly-muon-tra-d14k";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] DuongDanSv =
    [
        "/api/v1/sv/bien-ban-vi-pham",
        "/api/v1/sv/vi-pham"
    ];

    private static readonly string[] DuongDanQuay =
    [
        "/api/v1/quay/bien-ban-vi-pham",
        "/api/v1/quay/phieu-muon/PM-999999-000001/de-xuat-bien-ban"
    ];

    private const string DuongDanQuanTri = "/api/v1/quan-tri/bien-ban-vi-pham";

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
    public async Task KhongCoToken_NamEndpointDocViPhamTra401()
    {
        foreach (var duongDan in DuongDanSv.Concat(DuongDanQuay).Append(DuongDanQuanTri))
        {
            var phanHoi = await GoiAsync(duongDan, null);

            Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
            Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_GoiEndpointQuayVaQuanTri_Tra403()
    {
        var token = TaoToken("KT14-SV-HTTP", "SINH_VIEN");

        foreach (var duongDan in DuongDanQuay.Append(DuongDanQuanTri))
        {
            var phanHoi = await GoiAsync(duongDan, token);

            Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
            Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenNhanVien_GoiEndpointSinhVienVaQuanTri_Tra403()
    {
        var token = TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN");

        foreach (var duongDan in DuongDanSv.Append(DuongDanQuanTri))
        {
            var phanHoi = await GoiAsync(duongDan, token);

            Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
            Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenQuanTri_GoiEndpointQuayVaSinhVien_Tra403()
    {
        var token = TaoToken(Guid.NewGuid().ToString(), "QUAN_TRI");

        foreach (var duongDan in DuongDanQuay.Concat(DuongDanSv))
        {
            var phanHoi = await GoiAsync(duongDan, token);

            Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_GoiHaiDanhSach_Tra200DungHinhDang()
    {
        var token = TaoToken("KT14-SV-HTTP", "SINH_VIEN");

        foreach (var duongDan in DuongDanSv)
        {
            var phanHoi = await GoiAsync($"{duongDan}?trang=1&kichThuoc=10", token);

            Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
            var json = await DocJsonAsync(phanHoi);
            Assert.Equal(1, json.GetProperty("trang").GetInt32());
            Assert.Equal(10, json.GetProperty("kichThuoc").GetInt32());
            Assert.True(json.GetProperty("tongSo").GetInt32() >= 0);
            Assert.Equal(JsonValueKind.Array, json.GetProperty("danhSach").ValueKind);
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenNhanVienKhongCoPhong_GoiDanhSachBienBan_Tra200Rong()
    {
        var phanHoi = await GoiAsync("/api/v1/quay/bien-ban-vi-pham?trang=1&kichThuoc=10", TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN"));

        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal(0, json.GetProperty("tongSo").GetInt32());
        Assert.Equal(JsonValueKind.Array, json.GetProperty("danhSach").ValueKind);
        Assert.Equal(0, json.GetProperty("danhSach").GetArrayLength());
    }

    [CanKetNoiTestFact]
    public async Task TokenQuanTri_GoiDanhSachBienBan_Tra200DungHinhDang()
    {
        var phanHoi = await GoiAsync($"{DuongDanQuanTri}?maSV=KT14-SV-KHONG-CO&trang=1&kichThuoc=10", TaoToken(Guid.NewGuid().ToString(), "QUAN_TRI"));

        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal(0, json.GetProperty("tongSo").GetInt32());
        Assert.Equal(JsonValueKind.Array, json.GetProperty("danhSach").ValueKind);
    }

    [CanKetNoiTestFact]
    public async Task TrangThaiKhongHopLe_VaPhanTrangKhongHopLe_Tra400()
    {
        var token = TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN");

        var trangThai = await GoiAsync("/api/v1/quay/bien-ban-vi-pham?trangThai=KHAC", token);
        var kichThuoc = await GoiAsync("/api/v1/quay/bien-ban-vi-pham?kichThuoc=101", token);
        var maPhieu = await GoiAsync("/api/v1/quay/phieu-muon/ABC/de-xuat-bien-ban", token);

        Assert.Equal(HttpStatusCode.BadRequest, trangThai.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, kichThuoc.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, maPhieu.StatusCode);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(maPhieu)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task DeXuat_PhieuKhongTonTai_Tra404()
    {
        var phanHoi = await GoiAsync("/api/v1/quay/phieu-muon/PM-999999-000001/de-xuat-bien-ban", TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN"));

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

