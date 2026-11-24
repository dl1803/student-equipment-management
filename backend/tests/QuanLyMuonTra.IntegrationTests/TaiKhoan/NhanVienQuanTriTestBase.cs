using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public abstract class NhanVienQuanTriTestBase : IAsyncLifetime
{
    protected const string DuongDan = "/api/v1/quan-tri/nhan-vien";
    protected const string TenAdminSeed = "admin-e2e-d06k";
    protected const string MatKhauAdminSeed = "MatKhauAdmin@123";
    protected const string MatKhauNhanVien = "MatKhauNv@123";
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly List<Guid> _dsNhanVienTao = [];
    private readonly List<Guid> _dsPhongTao = [];

    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;

    protected HttpClient Client { get; private set; } = null!;

    protected string TokenQuanTri { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", KetNoiKiemThu.LayChuoiKetNoi());
                builder.UseSetting("Jwt:Secret", "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len");
                builder.UseSetting("Jwt:Issuer", "quan-ly-muon-tra-e2e");
                builder.UseSetting("Jwt:Audience", "quan-ly-muon-tra-e2e");
                builder.UseSetting("Jwt:ExpiryMinutes", "60");
                builder.UseSetting("Jwt:RefreshTokenDays", "7");
                builder.UseSetting("Seed:AdminUser", TenAdminSeed);
                builder.UseSetting("Seed:AdminPassword", MatKhauAdminSeed);
            });

        Client = Factory.CreateClient();

        using (var phamVi = Factory.Services.CreateScope())
        {
            var boKhoiTao = phamVi.ServiceProvider.GetRequiredService<QuanTriVienSeeder>();
            await boKhoiTao.ChayAsync(TenAdminSeed, MatKhauAdminSeed, CancellationToken.None);
        }

        TokenQuanTri = (await DangNhapNhanVienAsync(TenAdminSeed, MatKhauAdminSeed)).AccessToken;
    }

    public async Task DisposeAsync()
    {
        try
        {
            using var phamVi = Factory.Services.CreateScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            foreach (var ma in _dsNhanVienTao)
            {
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.refresh_token WHERE chu_so_huu_ma = {ma.ToString()}");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nhan_vien_phong WHERE ma_nguoi_dung = {ma}");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung = {ma}");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {ma}");
            }

            foreach (var maPhong in _dsPhongTao)
            {
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nhan_vien_phong WHERE ma_phong = {maPhong}");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM danh_muc.phong_thiet_bi WHERE ma_phong = {maPhong}");
            }
        }
        catch
        {
        }

        Client.Dispose();
        await Factory.DisposeAsync();
    }

    protected static async Task<JsonElement> DocJsonAsync(HttpResponseMessage phanHoi) =>
        await phanHoi.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

    protected async Task<HttpResponseMessage> GuiAsync(HttpMethod phuongThuc, string duongDan, string? token, object? noiDung = null)
    {
        using var yeuCau = new HttpRequestMessage(phuongThuc, duongDan);
        if (token is not null)
        {
            yeuCau.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (noiDung is not null)
        {
            yeuCau.Content = JsonContent.Create(noiDung);
        }

        return await Client.SendAsync(yeuCau);
    }

    protected async Task<(string AccessToken, string RefreshToken)> DangNhapNhanVienAsync(string tenDangNhap, string matKhau)
    {
        var phanHoi = await Client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", new { tenDangNhap, matKhau });
        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);

        return (json.GetProperty("accessToken").GetString()!, json.GetProperty("refreshToken").GetString()!);
    }

    protected async Task<(Guid Ma, string TenDangNhap, string MaNv)> TaoNhanVienQuaApiAsync()
    {
        var hau = Guid.NewGuid().ToString("N")[..12];
        var tenDangNhap = $"nv-d06k-{hau}";
        var maNv = $"NV-D06K-{hau}";
        var phanHoi = await GuiAsync(
            HttpMethod.Post,
            DuongDan,
            TokenQuanTri,
            new { tenDangNhap, matKhau = MatKhauNhanVien, hoTen = "Nhan Vien Kiem Thu", maNV = maNv, email = (string?)null });
        Assert.Equal(HttpStatusCode.Created, phanHoi.StatusCode);
        var ma = Guid.Parse((await DocJsonAsync(phanHoi)).GetProperty("maNguoiDung").GetString()!);
        _dsNhanVienTao.Add(ma);

        return (ma, tenDangNhap, maNv);
    }

    protected async Task<Guid> ThemPhongAsync()
    {
        var maPhong = Guid.CreateVersion7();
        var tenPhong = $"Phong D06K {maPhong:N}";
        using var phamVi = Factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so) VALUES ({maPhong}, {tenPhong}, 'CS1')
            """);
        _dsPhongTao.Add(maPhong);

        return maPhong;
    }

    protected async Task<Guid> LayMaNguoiDungQuanTriAsync()
    {
        using var phamVi = Factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Database.SqlQuery<Guid>($"""
            SELECT ma_nguoi_dung AS "Value" FROM tai_khoan.nguoi_dung_he_thong WHERE ten_dang_nhap = {TenAdminSeed}
            """).SingleAsync();
    }

    protected async Task<TKetQua> TruyVanDonAsync<TKetQua>(FormattableString cauLenh)
    {
        using var phamVi = Factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Database.SqlQuery<TKetQua>(cauLenh).SingleAsync();
    }

    protected async Task ThucThiAsync(FormattableString cauLenh)
    {
        using var phamVi = Factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync(cauLenh);
    }
}

