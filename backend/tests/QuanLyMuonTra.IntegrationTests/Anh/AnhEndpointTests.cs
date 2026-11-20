using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuanLyMuonTra.Infrastructure.LuuTru;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;

namespace QuanLyMuonTra.IntegrationTests.Anh;

public sealed class AnhEndpointTests : IAsyncLifetime
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private const string NguoiPhatHanh = "quan-ly-muon-tra-d08k";
    private const string DoiTuongNhan = "quan-ly-muon-tra-d08k";
    private const string DuongDan = "/api/v1/anh";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly byte[] AnhJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];

    private readonly AnhStorageGia _luuTru = new();
    private readonly Guid _maNguoiDung = Guid.CreateVersion7();
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        var chuoiKetNoi = KetNoiKiemThu.LayChuoiKetNoi();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", chuoiKetNoi);
                builder.UseSetting("Jwt:Secret", BiMatJwt);
                builder.UseSetting("Jwt:Issuer", NguoiPhatHanh);
                builder.UseSetting("Jwt:Audience", DoiTuongNhan);
                builder.ConfigureTestServices(dichVu => dichVu.AddSingleton<IAnhStorage>(_luuTru));
            });
        _client = _factory.CreateClient();

        using var phamVi = _factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var tenDangNhap = $"d08k-{_maNguoiDung:N}";
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
            VALUES ({_maNguoiDung}, {tenDangNhap}, 'khong-dung', 'NV kiem thu D08-K')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv) VALUES ({_maNguoiDung}, {tenDangNhap})
            """);
    }

    public async Task DisposeAsync()
    {
        try
        {
            using var phamVi = _factory!.Services.CreateScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM luu_tru.anh_tai_len WHERE nguoi_tai_len = {_maNguoiDung}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung = {_maNguoiDung}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {_maNguoiDung}");
        }
        catch
        {
        }

        _client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    [CanKetNoiTestFact]
    public async Task KhongCoToken_Tra401()
    {
        var phanHoi = await TaiAnhAsync(null, AnhJpeg, "BAN_DAU");

        Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
        Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task TokenSinhVien_Tra403KhongDuQuyen()
    {
        var phanHoi = await TaiAnhAsync(TaoToken("D08K-SV", "SINH_VIEN"), AnhJpeg, "BAN_DAU");

        Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
        Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task NhanVien_AnhHopLe_Tra201VaGhiDongChuaGan()
    {
        var phanHoi = await TaiAnhAsync(TaoToken(_maNguoiDung.ToString(), "NHAN_VIEN"), AnhJpeg, "BAN_DAU");

        Assert.Equal(HttpStatusCode.Created, phanHoi.StatusCode);
        var maAnh = (await DocJsonAsync(phanHoi)).GetProperty("maAnh").GetGuid();
        var dongStorage = Assert.Single(_luuTru.DaTai);
        Assert.Equal("tinh-trang-anh", dongStorage.Bucket);
        Assert.Equal("image/jpeg", dongStorage.LoaiNoiDung);
        using var phamVi = _factory!.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var dong = await db.Set<AnhTaiLen>().AsNoTracking().SingleAsync(a => a.MaAnh == maAnh);
        Assert.Equal(_maNguoiDung, dong.NguoiTaiLen);
        Assert.Equal(dongStorage.Khoa, dong.KhoaLuuTru);
        Assert.Null(dong.DoiTuongId);
    }

    [CanKetNoiTestFact]
    public async Task QuanTri_AnhThietBi_Tra201VaoBucketCongKhai()
    {
        var phanHoi = await TaiAnhAsync(TaoToken(_maNguoiDung.ToString(), "QUAN_TRI"), AnhJpeg, "THIET_BI");

        Assert.Equal(HttpStatusCode.Created, phanHoi.StatusCode);
        Assert.Equal("thiet-bi-anh", Assert.Single(_luuTru.DaTai).Bucket);
    }

    [CanKetNoiTestFact]
    public async Task TepGiaMao_DuoiJpegVaContentTypeImage_Tra400KhongGhiGi()
    {
        var giaMao = "Day khong phai anh"u8.ToArray();

        var phanHoi = await TaiAnhAsync(TaoToken(_maNguoiDung.ToString(), "NHAN_VIEN"), giaMao, "BAN_DAU");

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        Assert.Empty(_luuTru.DaTai);
        Assert.Equal(0, await DemAnhAsync());
    }

    [CanKetNoiTestFact]
    public async Task AnhVuotHaiMB_Tra400()
    {
        var lon = new byte[2_097_153];
        lon[0] = 0xFF;
        lon[1] = 0xD8;
        lon[2] = 0xFF;

        var phanHoi = await TaiAnhAsync(TaoToken(_maNguoiDung.ToString(), "NHAN_VIEN"), lon, "BAN_DAU");

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        Assert.Empty(_luuTru.DaTai);
    }

    [CanKetNoiTestFact]
    public async Task ThieuHoacSaiGiaiDoan_Tra400()
    {
        var token = TaoToken(_maNguoiDung.ToString(), "NHAN_VIEN");

        var thieu = await TaiAnhAsync(token, AnhJpeg, null);
        var sai = await TaiAnhAsync(token, AnhJpeg, "KHAC");

        Assert.Equal(HttpStatusCode.BadRequest, thieu.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, sai.StatusCode);
        Assert.Empty(_luuTru.DaTai);
    }

    [CanKetNoiTestFact]
    public async Task ThieuTep_Tra400()
    {
        var phanHoi = await TaiAnhAsync(TaoToken(_maNguoiDung.ToString(), "NHAN_VIEN"), null, "BAN_DAU");

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [CanKetNoiTestFact]
    public async Task NoiDungJsonThayViMultipart_Tra400()
    {
        using var yeuCau = new HttpRequestMessage(HttpMethod.Post, DuongDan)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        yeuCau.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TaoToken(_maNguoiDung.ToString(), "NHAN_VIEN"));

        var phanHoi = await _client!.SendAsync(yeuCau);

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
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

    private static async Task<JsonElement> DocJsonAsync(HttpResponseMessage phanHoi) =>
        JsonSerializer.Deserialize<JsonElement>(await phanHoi.Content.ReadAsStringAsync(), JsonOptions);

    private Task<HttpResponseMessage> TaiAnhAsync(string? token, byte[]? noiDung, string? giaiDoan)
    {
        var form = new MultipartFormDataContent();
        if (noiDung is not null)
        {
            var phanTep = new ByteArrayContent(noiDung);
            phanTep.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            form.Add(phanTep, "file", "anh.jpg");
        }

        if (giaiDoan is not null)
        {
            form.Add(new StringContent(giaiDoan), "giaiDoan");
        }

        var yeuCau = new HttpRequestMessage(HttpMethod.Post, DuongDan) { Content = form };
        if (token is not null)
        {
            yeuCau.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return _client!.SendAsync(yeuCau);
    }

    private async Task<int> DemAnhAsync()
    {
        using var phamVi = _factory!.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Set<AnhTaiLen>().CountAsync(a => a.NguoiTaiLen == _maNguoiDung);
    }
}

