using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public sealed class XacThucEndToEndTests : IAsyncLifetime
{
    private const string TenAdminSeed = "admin-e2e-d04k";
    private const string MatKhauAdminSeed = "MatKhauAdmin@123";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
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

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Dọn dẹp dữ liệu kiểm thử
        try
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var admin = await db.Set<QuanTriVien>().FirstOrDefaultAsync(q => q.TenDangNhap == TenAdminSeed);
            if (admin != null)
            {
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.refresh_token WHERE chu_so_huu_ma = {admin.MaNguoiDung.ToString()}");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.quan_tri_vien WHERE ma_nguoi_dung = {admin.MaNguoiDung}");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {admin.MaNguoiDung}");
            }
        }
        catch
        {
            // Bỏ qua lỗi dọn dẹp
        }

        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Phan7_DangNhapDung_Tra200_CoTokenVaVaiTroQuanTri()
    {
        var body = new { tenDangNhap = TenAdminSeed, matKhau = MatKhauAdminSeed };
        var res = await _client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", body);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var json = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(json.TryGetProperty("accessToken", out var at) && !string.IsNullOrWhiteSpace(at.GetString()));
        Assert.True(json.TryGetProperty("refreshToken", out var rt) && !string.IsNullOrWhiteSpace(rt.GetString()));
        Assert.Equal("QUAN_TRI", json.GetProperty("vaiTro").GetString());
    }

    [Fact]
    public async Task Phan7_DangNhapSaiMatKhau_Tra401_MaLoiDangNhapSai()
    {
        var body = new { tenDangNhap = TenAdminSeed, matKhau = "MatKhauSai@123" };
        var res = await _client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", body);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        var json = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("DANG_NHAP_SAI", json.GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task Phan7_ThieuTenDangNhapHoacMatKhau_Tra400_DuLieuKhongHopLe()
    {
        var body = new { tenDangNhap = "", matKhau = "" };
        var res = await _client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", body);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var json = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", json.GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task Phan7_LayThongTinToi_KhongCoToken_Tra401_ChuaXacThuc()
    {
        var res = await _client.GetAsync("/api/v1/auth/toi");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Phan7_LayThongTinToi_CoTokenQuanTri_Tra200_DsPhongRong()
    {
        // 1. Đăng nhập lấy token
        var loginRes = await _client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", new { tenDangNhap = TenAdminSeed, matKhau = MatKhauAdminSeed });
        var loginData = await loginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = loginData.GetProperty("accessToken").GetString();

        // 2. Gọi GET /api/v1/auth/toi
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/toi");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var toiData = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("QUAN_TRI", toiData.GetProperty("vaiTro").GetString());
        Assert.Empty(toiData.GetProperty("dsPhong").EnumerateArray());
    }

    [Fact]
    public async Task Phan7_KhoaTaiKhoan_DungTokenCuGoi_Tra403_TaiKhoanBiKhoa_RoiMoLaiTra200()
    {
        // 1. Đăng nhập lấy token
        var loginRes = await _client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", new { tenDangNhap = TenAdminSeed, matKhau = MatKhauAdminSeed });
        var loginData = await loginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = loginData.GetProperty("accessToken").GetString();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Set<QuanTriVien>().FirstAsync(q => q.TenDangNhap == TenAdminSeed);

        // 2. Khóa tài khoản trong DB
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE tai_khoan.nguoi_dung_he_thong SET trang_thai_khoa = 'BI_KHOA' WHERE ma_nguoi_dung = {user.MaNguoiDung}");

        try
        {
            // 3. Dùng token cũ gọi GET /auth/toi -> kỳ vọng 403
            using (var reqKhoa = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/toi"))
            {
                reqKhoa.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resKhoa = await _client.SendAsync(reqKhoa);
                Assert.Equal(HttpStatusCode.Forbidden, resKhoa.StatusCode);
            }

            // 4. Mở lại tài khoản
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE tai_khoan.nguoi_dung_he_thong SET trang_thai_khoa = 'HOAT_DONG' WHERE ma_nguoi_dung = {user.MaNguoiDung}");

            // 5. Gọi lại -> kỳ vọng 200
            using (var reqMo = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/toi"))
            {
                reqMo.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resMo = await _client.SendAsync(reqMo);
                Assert.Equal(HttpStatusCode.OK, resMo.StatusCode);
            }
        }
        finally
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE tai_khoan.nguoi_dung_he_thong SET trang_thai_khoa = 'HOAT_DONG' WHERE ma_nguoi_dung = {user.MaNguoiDung}");
        }
    }

    [Fact]
    public async Task Phan7_NhapSaiQuaSoLanToiDa_TaiKhoanBiTamKhoa_DangNhapLaiTra403()
    {
        var tenNguoiDung = $"user-lock-{Guid.NewGuid():N}";
        var matKhauDung = "MatKhauChuan@123";
        var matKhauBam = new PasswordHasher<NguoiDungHeThong>().HashPassword(null!, matKhauDung);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var maNguoiDung = Guid.CreateVersion7();
            var maQt = $"QT{Guid.NewGuid():N}"[..10];
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten, trang_thai_khoa)
                VALUES ({maNguoiDung}, {tenNguoiDung}, {matKhauBam}, 'Test User Lock', 'HOAT_DONG');
                INSERT INTO tai_khoan.quan_tri_vien (ma_nguoi_dung, ma_qt) VALUES ({maNguoiDung}, {maQt});
            """);
        }

        try
        {
            // Nhập sai 5 lần (ngưỡng mặc định của tham_so_he_thong)
            for (int i = 0; i < 5; i++)
            {
                var resSai = await _client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", new { tenDangNhap = tenNguoiDung, matKhau = "MatKhauSai" });
                Assert.Equal(HttpStatusCode.Unauthorized, resSai.StatusCode);
            }

            // Lần thứ 6, dù nhập đúng mật khẩu cũng phải bị chặn 403 TAI_KHOAN_BI_KHOA
            var resKhoa = await _client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", new { tenDangNhap = tenNguoiDung, matKhau = matKhauDung });
            Assert.Equal(HttpStatusCode.Forbidden, resKhoa.StatusCode);
            var json = await resKhoa.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            Assert.Equal("TAI_KHOAN_BI_KHOA", json.GetProperty("maLoi").GetString());
        }
        finally
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.quan_tri_vien WHERE ma_nguoi_dung IN (SELECT ma_nguoi_dung FROM tai_khoan.nguoi_dung_he_thong WHERE ten_dang_nhap = {tenNguoiDung})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ten_dang_nhap = {tenNguoiDung}");
        }
    }

    [Fact]
    public async Task Phan7_NhanVienCoQuyenPhong_DangNhapVaLayThongTinToi_TraDungPhongTuDanhMucReader()
    {
        var tenNv = $"nv-phong-{Guid.NewGuid():N}";
        var matKhau = "MatKhauNv@123";
        var matKhauBam = new PasswordHasher<NguoiDungHeThong>().HashPassword(null!, matKhau);
        var maPhong = Guid.Parse("019c0000-0000-7000-8000-000000000001");
        var maNv = Guid.CreateVersion7();
        var maNvMa = $"NV{Guid.NewGuid():N}"[..10];

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
                VALUES ({maPhong}, 'Phòng Thiết bị A', 'Cơ sở 1')
                ON CONFLICT (ma_phong) DO NOTHING;

                INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten, trang_thai_khoa)
                VALUES ({maNv}, {tenNv}, {matKhauBam}, 'Nhan Vien Phong A', 'HOAT_DONG');

                INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv) VALUES ({maNv}, {maNvMa});

                INSERT INTO tai_khoan.nhan_vien_phong (ma_nguoi_dung, ma_phong) VALUES ({maNv}, {maPhong});
            """);
        }

        try
        {
            // 1. Đăng nhập
            var loginRes = await _client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", new { tenDangNhap = tenNv, matKhau });
            Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
            var loginData = await loginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            var token = loginData.GetProperty("accessToken").GetString();
            Assert.Equal("NHAN_VIEN", loginData.GetProperty("vaiTro").GetString());

            // 2. Gọi GET /api/v1/auth/toi
            using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/toi");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var toiData = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            Assert.Equal("NHAN_VIEN", toiData.GetProperty("vaiTro").GetString());

            var dsPhong = toiData.GetProperty("dsPhong").EnumerateArray().ToList();
            Assert.Single(dsPhong);
            var phong = dsPhong[0];
            Assert.Equal(maPhong, phong.GetProperty("maPhong").GetGuid());
            Assert.Equal("Phòng Thiết bị A", phong.GetProperty("tenPhong").GetString());
            Assert.Equal("Cơ sở 1", phong.GetProperty("coSo").GetString());
        }
        finally
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.refresh_token WHERE chu_so_huu_ma = {maNv.ToString()}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nhan_vien_phong WHERE ma_nguoi_dung = {maNv}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung = {maNv}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {maNv}");
        }
    }
}
