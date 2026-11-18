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

public sealed class YeuCauMuonSvHttpTests : IAsyncLifetime
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private const string NguoiPhatHanh = "quan-ly-muon-tra-d06v";
    private const string DoiTuongNhan = "quan-ly-muon-tra-d06v";
    private const string DuongDan = "/api/v1/sv/phieu-yeu-cau-muon";
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

    private static string BodyHopLe(Guid maPhong, Guid maLoai)
    {
        return $$"""{"maPhong":"{{maPhong}}","thoiGianMuonDuKienGio":2,"dsLoaiVaSoLuong":[{"maLoai":"{{maLoai}}","soLuong":1}]}""";
    }

    [CanKetNoiTestFact]
    public async Task KhongCoToken_BaEndpointTra401()
    {
        var cacPhanHoi = new[]
        {
            await GoiAsync(HttpMethod.Post, DuongDan, null, BodyHopLe(Guid.NewGuid(), Guid.NewGuid())),
            await GoiAsync(HttpMethod.Get, DuongDan, null),
            await GoiAsync(HttpMethod.Get, $"{DuongDan}/YC-261118-000001", null)
        };

        foreach (var phanHoi in cacPhanHoi)
        {
            Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
            Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task TokenNhanVien_BaEndpointTra403KhongDuQuyen()
    {
        var token = TaoToken(Guid.NewGuid().ToString(), "NHAN_VIEN");
        var cacPhanHoi = new[]
        {
            await GoiAsync(HttpMethod.Post, DuongDan, token, BodyHopLe(Guid.NewGuid(), Guid.NewGuid())),
            await GoiAsync(HttpMethod.Get, DuongDan, token),
            await GoiAsync(HttpMethod.Get, $"{DuongDan}/YC-261118-000001", token)
        };

        foreach (var phanHoi in cacPhanHoi)
        {
            Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
            Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task UC02_GuiBodyKhongHopLe_Tra400KemTruongLoi()
    {
        var token = TaoToken("D06-SV-HTTP", "SINH_VIEN");
        const string body = """{"maPhong":"00000000-0000-0000-0000-000000000000","thoiGianMuonDuKienGio":0,"dsLoaiVaSoLuong":[]}""";

        var phanHoi = await GoiAsync(HttpMethod.Post, DuongDan, token, body);

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", json.GetProperty("maLoi").GetString());
        var cacTruong = json.GetProperty("errors");
        Assert.True(cacTruong.TryGetProperty("maPhong", out _));
        Assert.True(cacTruong.TryGetProperty("thoiGianMuonDuKienGio", out _));
        Assert.True(cacTruong.TryGetProperty("dsLoaiVaSoLuong", out _));
    }

    [CanKetNoiTestFact]
    public async Task UC02_GuiLoaiTrungHoacSoLuongKhongDuong_Tra400()
    {
        var token = TaoToken("D06-SV-HTTP", "SINH_VIEN");
        var maPhong = Guid.NewGuid();
        var maLoai = Guid.NewGuid();
        var bodyTrung = $$"""{"maPhong":"{{maPhong}}","thoiGianMuonDuKienGio":2,"dsLoaiVaSoLuong":[{"maLoai":"{{maLoai}}","soLuong":1},{"maLoai":"{{maLoai}}","soLuong":1}]}""";
        var bodySoLuongAm = $$"""{"maPhong":"{{maPhong}}","thoiGianMuonDuKienGio":2,"dsLoaiVaSoLuong":[{"maLoai":"{{maLoai}}","soLuong":-1}]}""";

        var phanHoiTrung = await GoiAsync(HttpMethod.Post, DuongDan, token, bodyTrung);
        var phanHoiAm = await GoiAsync(HttpMethod.Post, DuongDan, token, bodySoLuongAm);

        Assert.Equal(HttpStatusCode.BadRequest, phanHoiTrung.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, phanHoiAm.StatusCode);
    }

    [CanKetNoiTestFact]
    public async Task UC02_GuiJsonHong_Tra400()
    {
        var token = TaoToken("D06-SV-HTTP", "SINH_VIEN");

        var phanHoi = await GoiAsync(HttpMethod.Post, DuongDan, token, "{");

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task UC02_GuiPhongKhongTonTai_Tra400VaKhongGhiDuLieu()
    {
        var token = TaoToken("D06-SV-HTTP", "SINH_VIEN");

        var phanHoi = await GoiAsync(HttpMethod.Post, DuongDan, token, BodyHopLe(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task UC02_DsCuaSvChuaCoPhieu_Tra200DanhSachRong()
    {
        var token = TaoToken("D06-SV-HTTP", "SINH_VIEN");

        var phanHoi = await GoiAsync(HttpMethod.Get, $"{DuongDan}?trang=1&kichThuoc=5", token);

        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal(0, json.GetProperty("tongSo").GetInt32());
        Assert.Equal(1, json.GetProperty("trang").GetInt32());
        Assert.Equal(5, json.GetProperty("kichThuoc").GetInt32());
        Assert.Equal(JsonValueKind.Array, json.GetProperty("danhSach").ValueKind);
        Assert.Equal(0, json.GetProperty("danhSach").GetArrayLength());
    }

    [CanKetNoiTestFact]
    public async Task UC02_ThamSoPhanTrangSai_Tra400()
    {
        var token = TaoToken("D06-SV-HTTP", "SINH_VIEN");
        string[] duongDan =
        [
            $"{DuongDan}?trang=0",
            $"{DuongDan}?kichThuoc=0",
            $"{DuongDan}?kichThuoc=101",
            $"{DuongDan}?trang=abc"
        ];

        foreach (var d in duongDan)
        {
            var phanHoi = await GoiAsync(HttpMethod.Get, d, token);

            Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
            Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
    }

    [CanKetNoiTestFact]
    public async Task UC02_ChiTietMaSaiDinhDangTra400MaKhongTonTaiTra404()
    {
        var token = TaoToken("D06-SV-HTTP", "SINH_VIEN");

        var saiDinhDang = await GoiAsync(HttpMethod.Get, $"{DuongDan}/khong-phai-ma", token);
        var khongTonTai = await GoiAsync(HttpMethod.Get, $"{DuongDan}/YC-000101-999999", token);

        Assert.Equal(HttpStatusCode.BadRequest, saiDinhDang.StatusCode);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(saiDinhDang)).GetProperty("maLoi").GetString());
        Assert.Equal(HttpStatusCode.NotFound, khongTonTai.StatusCode);
        Assert.Equal("KHONG_TIM_THAY", (await DocJsonAsync(khongTonTai)).GetProperty("maLoi").GetString());
    }
}
