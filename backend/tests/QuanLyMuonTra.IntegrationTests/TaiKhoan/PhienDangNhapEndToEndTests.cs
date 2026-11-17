using System.IdentityModel.Tokens.Jwt;
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
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public sealed class PhienDangNhapEndToEndTests : IAsyncLifetime
{
    private const string MatKhauSinhVien = "MatKhauSinhVien-1";
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
            });
        _client = _factory.CreateClient();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task UC19_DangNhapSinhVien_MatKhauDung_Tra200TokenSubLaMssvVaTaoHoSo()
    {
        var mssv = await ThemSinhVienTruongAsync("CNPM");
        try
        {
            var phanHoi = await DangNhapSinhVienAsync(mssv, MatKhauSinhVien);

            Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
            var json = await DocJsonAsync(phanHoi);
            Assert.Equal("SINH_VIEN", json.GetProperty("vaiTro").GetString());
            Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("refreshToken").GetString()));
            var token = new JwtSecurityTokenHandler().ReadJwtToken(json.GetProperty("accessToken").GetString());
            Assert.Equal(mssv, token.Claims.Single(c => c.Type == "sub").Value);
            Assert.Equal("SINH_VIEN", token.Claims.Single(c => c.Type == "role").Value);
            Assert.Equal(1, await DemTokenConHieuLucAsync(mssv));
            Assert.Equal(1, await DemHoSoSinhVienAsync(mssv));
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC19_DangNhapSinhVien_HoSoDaCoVaTruongRong_GiuQuyenMuonVaCapNhatTuTruong()
    {
        var mssv = await ThemSinhVienTruongAsync(null);
        try
        {
            using (var phamVi = _factory.Services.CreateScope())
            {
                var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky, trang_thai_quyen_muon)
                    VALUES ({mssv}, 'Ho Ten Cu', 'Khoa Cu', 'cu@kiemthu.local', 'DAI_HOC', DATE '2020-01-01', 'DANG_HOC', 'HK-CU', 'CAM_MUON_DAI_HAN')
                    """);
            }

            var phanHoi = await DangNhapSinhVienAsync(mssv, MatKhauSinhVien);

            Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
            using var phamViDoc = _factory.Services.CreateScope();
            var dbDoc = phamViDoc.ServiceProvider.GetRequiredService<AppDbContext>();
            var dong = await dbDoc.Database.SqlQuery<DongHoSo>($"""
                SELECT ho_ten AS "HoTen", khoa AS "Khoa", trang_thai_quyen_muon AS "TrangThaiQuyenMuon"
                FROM sinh_vien.sinh_vien
                WHERE ma_sv = {mssv}
                """).SingleAsync();
            Assert.Equal("Sinh Vien Kiem Thu", dong.HoTen);
            Assert.Null(dong.Khoa);
            Assert.Equal("CAM_MUON_DAI_HAN", dong.TrangThaiQuyenMuon);
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC19_DangNhapSinhVien_SaiMatKhau_Tra401DangNhapSaiVaKhongTaoGiDo()
    {
        var mssv = await ThemSinhVienTruongAsync("CNPM");
        try
        {
            var phanHoi = await DangNhapSinhVienAsync(mssv, "mat-khau-sai");

            Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
            Assert.Equal("DANG_NHAP_SAI", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
            Assert.Equal(0, await DemTokenConHieuLucAsync(mssv));
            Assert.Equal(0, await DemHoSoSinhVienAsync(mssv));
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC19_DangNhapSinhVien_MssvKhongTonTai_Tra401DangNhapSai()
    {
        var phanHoi = await DangNhapSinhVienAsync($"khong-co-{Guid.NewGuid():N}", MatKhauSinhVien);

        Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
        Assert.Equal("DANG_NHAP_SAI", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task UC19_DangNhapSinhVien_ThieuMssvHoacMatKhau_Tra400DuLieuKhongHopLe()
    {
        var phanHoi = await DangNhapSinhVienAsync(string.Empty, string.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task UC19_LamMoi_TokenHopLe_XoayVongTraCapTokenMoiVaVoHieuToken_Cu()
    {
        var mssv = await ThemSinhVienTruongAsync("CNPM");
        try
        {
            var (_, tokenCu) = await DangNhapThanhCongAsync(mssv);

            var phanHoi = await LamMoiAsync(tokenCu);

            Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
            var json = await DocJsonAsync(phanHoi);
            var tokenMoi = json.GetProperty("refreshToken").GetString()!;
            Assert.NotEqual(tokenCu, tokenMoi);
            Assert.Equal("SINH_VIEN", json.GetProperty("vaiTro").GetString());
            var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(json.GetProperty("accessToken").GetString());
            Assert.Equal(mssv, accessToken.Claims.Single(c => c.Type == "sub").Value);
            Assert.Equal(1, await DemTokenConHieuLucAsync(mssv));
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC19_LamMoi_DungLaiTokenDaXoayVong_Tra401VaThuHoiCaChuoi()
    {
        var mssv = await ThemSinhVienTruongAsync("CNPM");
        try
        {
            var (_, token1) = await DangNhapThanhCongAsync(mssv);
            var lan1 = await LamMoiAsync(token1);
            var token2 = (await DocJsonAsync(lan1)).GetProperty("refreshToken").GetString()!;

            var dungLai = await LamMoiAsync(token1);

            Assert.Equal(HttpStatusCode.Unauthorized, dungLai.StatusCode);
            Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(dungLai)).GetProperty("maLoi").GetString());
            var dungTokenMoi = await LamMoiAsync(token2);
            Assert.Equal(HttpStatusCode.Unauthorized, dungTokenMoi.StatusCode);
            Assert.Equal(0, await DemTokenConHieuLucAsync(mssv));
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC19_LamMoi_TokenKhongTonTai_Tra401ChuaXacThuc()
    {
        var phanHoi = await LamMoiAsync($"khong-co-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
        Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task UC19_LamMoi_TokenHetHan_Tra401()
    {
        var mssv = await ThemSinhVienTruongAsync("CNPM");
        try
        {
            var (_, token) = await DangNhapThanhCongAsync(mssv);
            using (var phamVi = _factory.Services.CreateScope())
            {
                var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    UPDATE tai_khoan.refresh_token SET het_han = now() - interval '1 minute'
                    WHERE ma_bam = {RefreshToken.Bam(token)}
                    """);
            }

            var phanHoi = await LamMoiAsync(token);

            Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC19_LamMoi_HaiYeuCauDongThoiCungToken_MotThanhCongMotBiTuChoiVaChuoiBiThuHoi()
    {
        var mssv = await ThemSinhVienTruongAsync("CNPM");
        try
        {
            var (_, token) = await DangNhapThanhCongAsync(mssv);

            var phanHoi = await Task.WhenAll(LamMoiAsync(token), LamMoiAsync(token));

            var thanhCong = phanHoi.Where(p => p.StatusCode == HttpStatusCode.OK).ToList();
            var biTuChoi = phanHoi.Where(p => p.StatusCode == HttpStatusCode.Unauthorized).ToList();
            Assert.Single(thanhCong);
            Assert.Single(biTuChoi);
            var tokenMoi = (await DocJsonAsync(thanhCong[0])).GetProperty("refreshToken").GetString()!;
            var dungTokenMoi = await LamMoiAsync(tokenMoi);
            Assert.Equal(HttpStatusCode.Unauthorized, dungTokenMoi.StatusCode);
            Assert.Equal(0, await DemTokenConHieuLucAsync(mssv));
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC19_LamMoi_NhanVienDuocXoayVongVaBiTuChoiKhiTaiKhoanBiKhoa()
    {
        var tenNv = $"nv-d05-{Guid.NewGuid():N}";
        var matKhau = "MatKhauNv@123";
        var maNv = Guid.CreateVersion7();
        var bam = new PasswordHasher<NguoiDungHeThong>().HashPassword(null!, matKhau);
        using (var phamVi = _factory.Services.CreateScope())
        {
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten, trang_thai_khoa)
                VALUES ({maNv}, {tenNv}, {bam}, 'Nhan Vien D05', 'HOAT_DONG');
                INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv) VALUES ({maNv}, {tenNv});
                """);
        }

        try
        {
            var dangNhap = await _client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", new { tenDangNhap = tenNv, matKhau });
            Assert.Equal(HttpStatusCode.OK, dangNhap.StatusCode);
            var token1 = (await DocJsonAsync(dangNhap)).GetProperty("refreshToken").GetString()!;

            var lan1 = await LamMoiAsync(token1);

            Assert.Equal(HttpStatusCode.OK, lan1.StatusCode);
            var json1 = await DocJsonAsync(lan1);
            Assert.Equal("NHAN_VIEN", json1.GetProperty("vaiTro").GetString());
            var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(json1.GetProperty("accessToken").GetString());
            Assert.Equal(maNv.ToString(), accessToken.Claims.Single(c => c.Type == "sub").Value);
            var token2 = json1.GetProperty("refreshToken").GetString()!;
            using (var phamVi = _factory.Services.CreateScope())
            {
                var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE tai_khoan.nguoi_dung_he_thong SET trang_thai_khoa = 'BI_KHOA' WHERE ma_nguoi_dung = {maNv}");
            }

            var khiBiKhoa = await LamMoiAsync(token2);

            Assert.Equal(HttpStatusCode.Unauthorized, khiBiKhoa.StatusCode);
            Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(khiBiKhoa)).GetProperty("maLoi").GetString());
        }
        finally
        {
            using var phamVi = _factory.Services.CreateScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.refresh_token WHERE chu_so_huu_ma = {maNv.ToString()}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung = {maNv}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {maNv}");
        }
    }

    [Fact]
    public async Task UC19_DangXuat_VoiAccessTokenKhongHopLe_Tra204VaTokenKhongLamMoiDuoc()
    {
        var mssv = await ThemSinhVienTruongAsync("CNPM");
        try
        {
            var (_, token) = await DangNhapThanhCongAsync(mssv);
            using var yeuCau = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/dang-xuat")
            {
                Content = JsonContent.Create(new { refreshToken = token })
            };
            yeuCau.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "access-token-het-han");

            var dangXuat = await _client.SendAsync(yeuCau);

            Assert.Equal(HttpStatusCode.NoContent, dangXuat.StatusCode);
            var lamMoi = await LamMoiAsync(token);
            Assert.Equal(HttpStatusCode.Unauthorized, lamMoi.StatusCode);
            Assert.Equal(0, await DemTokenConHieuLucAsync(mssv));
            var dangXuatLai = await DangXuatAsync(token);
            Assert.Equal(HttpStatusCode.NoContent, dangXuatLai.StatusCode);
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC19_DangXuat_TokenKhongTonTai_Tra401ChuaXacThuc()
    {
        var phanHoi = await DangXuatAsync($"khong-co-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.Unauthorized, phanHoi.StatusCode);
        Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [Theory]
    [InlineData("/api/v1/auth/lam-moi")]
    [InlineData("/api/v1/auth/dang-xuat")]
    public async Task UC19_ThieuRefreshToken_Tra400DuLieuKhongHopLe(string duongDan)
    {
        var phanHoi = await _client.PostAsJsonAsync(duongDan, new { refreshToken = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    private Task<HttpResponseMessage> DangNhapSinhVienAsync(string mssv, string matKhau) =>
        _client.PostAsJsonAsync("/api/v1/auth/sinh-vien/dang-nhap", new { mssv, matKhau });

    private Task<HttpResponseMessage> LamMoiAsync(string refreshToken) =>
        _client.PostAsJsonAsync("/api/v1/auth/lam-moi", new { refreshToken });

    private Task<HttpResponseMessage> DangXuatAsync(string refreshToken) =>
        _client.PostAsJsonAsync("/api/v1/auth/dang-xuat", new { refreshToken });

    private static async Task<JsonElement> DocJsonAsync(HttpResponseMessage phanHoi) =>
        await phanHoi.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

    private async Task<(string AccessToken, string RefreshToken)> DangNhapThanhCongAsync(string mssv)
    {
        var phanHoi = await DangNhapSinhVienAsync(mssv, MatKhauSinhVien);
        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);

        return (json.GetProperty("accessToken").GetString()!, json.GetProperty("refreshToken").GetString()!);
    }

    private async Task<string> ThemSinhVienTruongAsync(string? khoa)
    {
        var mssv = $"SV-D05-{Guid.NewGuid():N}";
        var bam = new PasswordHasher<object>().HashPassword(null!, MatKhauSinhVien);
        using var phamVi = _factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO truong_gia_lap.sinh_vien_truong (ma_sv, mat_khau_bam, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
            VALUES ({mssv}, {bam}, 'Sinh Vien Kiem Thu', 'KTPM2024', {khoa}::text, 'kiemthu@truong.local', 'DAI_HOC', DATE '2027-12-31', 'DANG_HOC', 'HK1-2026')
            """);

        return mssv;
    }

    private async Task DonSinhVienAsync(string mssv)
    {
        using var phamVi = _factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.refresh_token WHERE chu_so_huu_ma = {mssv}");
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM sinh_vien.sinh_vien WHERE ma_sv = {mssv}");
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM truong_gia_lap.sinh_vien_truong WHERE ma_sv = {mssv}");
    }

    private async Task<int> DemTokenConHieuLucAsync(string chuSoHuuMa)
    {
        using var phamVi = _factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Database.SqlQuery<int>($"""
            SELECT count(*)::int AS "Value" FROM tai_khoan.refresh_token
            WHERE chu_so_huu_ma = {chuSoHuuMa} AND da_thu_hoi = false
            """).SingleAsync();
    }

    private async Task<int> DemHoSoSinhVienAsync(string mssv)
    {
        using var phamVi = _factory.Services.CreateScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Database.SqlQuery<int>($"""
            SELECT count(*)::int AS "Value" FROM sinh_vien.sinh_vien WHERE ma_sv = {mssv}
            """).SingleAsync();
    }

    public sealed class DongHoSo
    {
        public string HoTen { get; set; } = string.Empty;

        public string? Khoa { get; set; }

        public string TrangThaiQuyenMuon { get; set; } = string.Empty;
    }
}
