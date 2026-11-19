using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using QuanLyMuonTra.IntegrationTests.TaiKhoan;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class HoSoSinhVienEndToEndTests : NhanVienQuanTriTestBase
{
    private const string MatKhauSinhVien = "MatKhauSinhVien-1";
    private const string DuongDanSvToi = "/api/v1/sv/toi";
    private const string DuongDanQuetThe = "/api/v1/quay/sinh-vien/the";

    [Fact]
    public async Task UC17_SvToi_SinhVienDangNhap_Tra200HoSoCuaChinhMinh()
    {
        var mssv = await ThemSinhVienTruongAsync();
        try
        {
            var tokenSv = await DangNhapSinhVienAsync(mssv);
            var soToiDa = await DocGioiHanDaiHocAsync();

            var phanHoi = await GuiAsync(HttpMethod.Get, DuongDanSvToi, tokenSv);

            Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
            KiemTraHoSoHopLe(await DocJsonAsync(phanHoi), mssv, soToiDa);
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC07_QuetThe_NhanVienChuaCapPhong_VanTra200HoSoDayDu()
    {
        var mssv = await ThemSinhVienTruongAsync();
        try
        {
            await DangNhapSinhVienAsync(mssv);
            var tokenNv = await TaoNhanVienVaDangNhapAsync();
            var soToiDa = await DocGioiHanDaiHocAsync();

            var phanHoi = await GuiAsync(HttpMethod.Get, $"{DuongDanQuetThe}/{mssv}", tokenNv);

            Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
            KiemTraHoSoHopLe(await DocJsonAsync(phanHoi), mssv, soToiDa);
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC07_QuetThe_TheHetHanHetHocKhoaQuyen_KhongHopLe_VaSvXemDuocLyDoCuaMinh()
    {
        var mssv = await ThemSinhVienTruongAsync();
        try
        {
            var tokenSv = await DangNhapSinhVienAsync(mssv);
            var tokenNv = await TaoNhanVienVaDangNhapAsync();
            await ThucThiAsync($"""
                UPDATE sinh_vien.sinh_vien
                SET han_the = DATE '2020-01-01', trang_thai_hoc = 'THOI_HOC', trang_thai_quyen_muon = 'TAM_KHOA'
                WHERE ma_sv = {mssv}
                """);
            var mongDoi = new[] { "SV_THE_HET_HAN", "SV_KHONG_CON_THEO_HOC", "SV_BI_KHOA_QUYEN_MUON" };

            var quetThe = await GuiAsync(HttpMethod.Get, $"{DuongDanQuetThe}/{mssv}", tokenNv);
            var svToi = await GuiAsync(HttpMethod.Get, DuongDanSvToi, tokenSv);

            Assert.Equal(HttpStatusCode.OK, quetThe.StatusCode);
            var jsonQuetThe = await DocJsonAsync(quetThe);
            Assert.False(jsonQuetThe.GetProperty("hopLe").GetBoolean());
            Assert.Equal(mongDoi, LayMaLoi(jsonQuetThe));
            Assert.All(
                jsonQuetThe.GetProperty("dsLyDoKhongHopLe").EnumerateArray(),
                lyDo => Assert.False(string.IsNullOrWhiteSpace(lyDo.GetProperty("thongDiep").GetString())));
            Assert.Equal(HttpStatusCode.OK, svToi.StatusCode);
            Assert.Equal(mongDoi, LayMaLoi(await DocJsonAsync(svToi)));
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task UC07_QuetThe_MssvKhongTonTai_Tra404KhongTimThay()
    {
        var tokenNv = await TaoNhanVienVaDangNhapAsync();

        var phanHoi = await GuiAsync(HttpMethod.Get, $"{DuongDanQuetThe}/KHONG-CO-{Guid.NewGuid():N}", tokenNv);

        Assert.Equal(HttpStatusCode.NotFound, phanHoi.StatusCode);
        Assert.Equal("KHONG_TIM_THAY", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task NFR01_QuetThe_ChiNhanVienDuocQuet_QuanTriVaSinhVienVaKhongTokenBiTuChoi()
    {
        var mssv = await ThemSinhVienTruongAsync();
        try
        {
            var tokenSv = await DangNhapSinhVienAsync(mssv);
            var duongDan = $"{DuongDanQuetThe}/{mssv}";

            var quanTri = await GuiAsync(HttpMethod.Get, duongDan, TokenQuanTri);
            var sinhVien = await GuiAsync(HttpMethod.Get, duongDan, tokenSv);
            var khongToken = await GuiAsync(HttpMethod.Get, duongDan, null);

            Assert.Equal(HttpStatusCode.Forbidden, quanTri.StatusCode);
            Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(quanTri)).GetProperty("maLoi").GetString());
            Assert.Equal(HttpStatusCode.Forbidden, sinhVien.StatusCode);
            Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(sinhVien)).GetProperty("maLoi").GetString());
            Assert.Equal(HttpStatusCode.Unauthorized, khongToken.StatusCode);
            Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(khongToken)).GetProperty("maLoi").GetString());
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    [Fact]
    public async Task NFR01_SvToi_NhanVienVaQuanTriVaKhongTokenBiTuChoi()
    {
        var tokenNv = await TaoNhanVienVaDangNhapAsync();

        var nhanVien = await GuiAsync(HttpMethod.Get, DuongDanSvToi, tokenNv);
        var quanTri = await GuiAsync(HttpMethod.Get, DuongDanSvToi, TokenQuanTri);
        var khongToken = await GuiAsync(HttpMethod.Get, DuongDanSvToi, null);

        Assert.Equal(HttpStatusCode.Forbidden, nhanVien.StatusCode);
        Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(nhanVien)).GetProperty("maLoi").GetString());
        Assert.Equal(HttpStatusCode.Forbidden, quanTri.StatusCode);
        Assert.Equal("KHONG_DU_QUYEN", (await DocJsonAsync(quanTri)).GetProperty("maLoi").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized, khongToken.StatusCode);
        Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(khongToken)).GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task NFR01_QuetThe_NhanVienBiKhoaVanCoJwt_Tra403TaiKhoanBiKhoa()
    {
        var mssv = await ThemSinhVienTruongAsync();
        try
        {
            await DangNhapSinhVienAsync(mssv);
            var nhanVien = await TaoNhanVienQuaApiAsync();
            var tokenNv = (await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien)).AccessToken;
            var khoa = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/khoa", TokenQuanTri);
            Assert.Equal(HttpStatusCode.OK, khoa.StatusCode);

            var phanHoi = await GuiAsync(HttpMethod.Get, $"{DuongDanQuetThe}/{mssv}", tokenNv);

            Assert.Equal(HttpStatusCode.Forbidden, phanHoi.StatusCode);
            Assert.Equal("TAI_KHOAN_BI_KHOA", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
        }
        finally
        {
            await DonSinhVienAsync(mssv);
        }
    }

    private static void KiemTraHoSoHopLe(JsonElement json, string mssv, int soToiDa)
    {
        Assert.Equal(mssv, json.GetProperty("maSV").GetString());
        Assert.Equal("Sinh Vien Kiem Thu", json.GetProperty("hoTen").GetString());
        Assert.Equal("KTPM2024", json.GetProperty("lop").GetString());
        Assert.Equal("CNPM", json.GetProperty("khoa").GetString());
        Assert.Equal("DAI_HOC", json.GetProperty("maNhom").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("tenNhom").GetString()));
        Assert.Equal("2027-12-31", json.GetProperty("hanThe").GetString());
        Assert.Equal("DANG_HOC", json.GetProperty("trangThaiHoc").GetString());
        Assert.Equal("BINH_THUONG", json.GetProperty("trangThaiQuyenMuon").GetString());
        Assert.Equal(0, json.GetProperty("soThietBiDangMuon").GetInt32());
        Assert.Equal(soToiDa, json.GetProperty("soThietBiToiDa").GetInt32());
        Assert.Empty(json.GetProperty("dsBienBanChuaNop").EnumerateArray());
        Assert.True(json.GetProperty("hopLe").GetBoolean());
        Assert.Empty(json.GetProperty("dsLyDoKhongHopLe").EnumerateArray());
    }

    private static string[] LayMaLoi(JsonElement json) =>
        json.GetProperty("dsLyDoKhongHopLe")
            .EnumerateArray()
            .Select(lyDo => lyDo.GetProperty("maLoi").GetString()!)
            .ToArray();

    private async Task<string> TaoNhanVienVaDangNhapAsync()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();

        return (await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien)).AccessToken;
    }

    private async Task<int> DocGioiHanDaiHocAsync() =>
        await TruyVanDonAsync<int>($"""
            SELECT so_thiet_bi_toi_da AS "Value" FROM quy_dinh.quy_dinh_muon
            WHERE ma_nhom = 'DAI_HOC' AND ma_loai IS NULL
            """);

    private async Task<string> ThemSinhVienTruongAsync()
    {
        var mssv = $"SV-D07K-{Guid.NewGuid():N}";
        var bam = new PasswordHasher<object>().HashPassword(null!, MatKhauSinhVien);
        await ThucThiAsync($"""
            INSERT INTO truong_gia_lap.sinh_vien_truong (ma_sv, mat_khau_bam, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
            VALUES ({mssv}, {bam}, 'Sinh Vien Kiem Thu', 'KTPM2024', 'CNPM', 'kiemthu@truong.local', 'DAI_HOC', DATE '2027-12-31', 'DANG_HOC', 'HK1-2026')
            """);

        return mssv;
    }

    private async Task<string> DangNhapSinhVienAsync(string mssv)
    {
        var phanHoi = await Client.PostAsJsonAsync("/api/v1/auth/sinh-vien/dang-nhap", new { mssv, matKhau = MatKhauSinhVien });
        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);

        return (await DocJsonAsync(phanHoi)).GetProperty("accessToken").GetString()!;
    }

    private async Task DonSinhVienAsync(string mssv)
    {
        await ThucThiAsync($"DELETE FROM tai_khoan.refresh_token WHERE chu_so_huu_ma = {mssv}");
        await ThucThiAsync($"DELETE FROM sinh_vien.sinh_vien WHERE ma_sv = {mssv}");
        await ThucThiAsync($"DELETE FROM truong_gia_lap.sinh_vien_truong WHERE ma_sv = {mssv}");
    }
}

