using System.Net;
using System.Net.Http.Json;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public sealed class KhoaMoKhoaDatLaiMatKhauNhanVienEndToEndTests : NhanVienQuanTriTestBase
{
    [Fact]
    public async Task UC14_KhoaNhanVien_TokenCuBiTuChoiNgay_RefreshTokenBiThuHoi_KhoaLaiTra409()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        var (accessToken, refreshToken) = await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien);

        var khoa = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/khoa", TokenQuanTri);

        Assert.Equal(HttpStatusCode.OK, khoa.StatusCode);
        Assert.Equal("BI_KHOA", (await DocJsonAsync(khoa)).GetProperty("trangThaiKhoa").GetString());
        var dungTokenCu = await GuiAsync(HttpMethod.Get, "/api/v1/auth/toi", accessToken);
        Assert.Equal(HttpStatusCode.Forbidden, dungTokenCu.StatusCode);
        var soTokenConHieuLuc = await TruyVanDonAsync<int>($"""
            SELECT count(*)::int AS "Value" FROM tai_khoan.refresh_token
            WHERE chu_so_huu_ma = {nhanVien.Ma.ToString()} AND da_thu_hoi = false
            """);
        Assert.Equal(0, soTokenConHieuLuc);
        var lamMoi = await Client.PostAsJsonAsync("/api/v1/auth/lam-moi", new { refreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, lamMoi.StatusCode);
        Assert.Equal("CHUA_XAC_THUC", (await DocJsonAsync(lamMoi)).GetProperty("maLoi").GetString());

        var khoaLai = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/khoa", TokenQuanTri);
        Assert.Equal(HttpStatusCode.Conflict, khoaLai.StatusCode);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", (await DocJsonAsync(khoaLai)).GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task UC14_MoKhoaNhanVien_DangNhapLaiDuoc_RefreshTokenCuVanBiThuHoi_MoLaiTra409()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        var (_, refreshTokenCu) = await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien);
        await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/khoa", TokenQuanTri);

        var moKhoa = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/mo-khoa", TokenQuanTri);

        Assert.Equal(HttpStatusCode.OK, moKhoa.StatusCode);
        Assert.Equal("HOAT_DONG", (await DocJsonAsync(moKhoa)).GetProperty("trangThaiKhoa").GetString());
        var lamMoiTokenCu = await Client.PostAsJsonAsync("/api/v1/auth/lam-moi", new { refreshToken = refreshTokenCu });
        Assert.Equal(HttpStatusCode.Unauthorized, lamMoiTokenCu.StatusCode);
        await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien);

        var moLai = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/mo-khoa", TokenQuanTri);
        Assert.Equal(HttpStatusCode.Conflict, moLai.StatusCode);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", (await DocJsonAsync(moLai)).GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task UC14_MoKhoaNhanVienTamKhoaDangNhap_VeHoatDongVaDatLaiBoDem()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        await ThucThiAsync($"""
            UPDATE tai_khoan.nguoi_dung_he_thong SET trang_thai_khoa = 'TAM_KHOA_DANG_NHAP', so_lan_dang_nhap_sai = 5
            WHERE ma_nguoi_dung = {nhanVien.Ma}
            """);

        var moKhoa = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/mo-khoa", TokenQuanTri);

        Assert.Equal(HttpStatusCode.OK, moKhoa.StatusCode);
        var soLanSai = await TruyVanDonAsync<int>($"""
            SELECT so_lan_dang_nhap_sai AS "Value" FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {nhanVien.Ma}
            """);
        Assert.Equal(0, soLanSai);
        await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien);
    }

    [Fact]
    public async Task UC14_KhoaNhanVienDangTamKhoaDangNhap_ChuyenBiKhoa()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        await ThucThiAsync($"""
            UPDATE tai_khoan.nguoi_dung_he_thong SET trang_thai_khoa = 'TAM_KHOA_DANG_NHAP' WHERE ma_nguoi_dung = {nhanVien.Ma}
            """);

        var khoa = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/khoa", TokenQuanTri);

        Assert.Equal(HttpStatusCode.OK, khoa.StatusCode);
        Assert.Equal("BI_KHOA", (await DocJsonAsync(khoa)).GetProperty("trangThaiKhoa").GetString());
    }

    [Fact]
    public async Task UC14_KhoaMoKhoa_MaKhongTonTaiHoacLaQuanTri_Tra404()
    {
        var maQuanTri = await LayMaNguoiDungQuanTriAsync();

        foreach (var ma in new[] { Guid.NewGuid(), maQuanTri })
        {
            var khoa = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{ma}/khoa", TokenQuanTri);
            var moKhoa = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{ma}/mo-khoa", TokenQuanTri);

            Assert.Equal(HttpStatusCode.NotFound, khoa.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, moKhoa.StatusCode);
        }
    }

    [Fact]
    public async Task UC14_DatLaiMatKhau_Tra204_MatKhauCuHetHieuLuc_KhongLoMatKhauTrongNhatKy()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        const string matKhauMoi = "MatKhauMoi@456";

        var phanHoi = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/dat-lai-mat-khau", TokenQuanTri, new { matKhauMoi });

        Assert.Equal(HttpStatusCode.NoContent, phanHoi.StatusCode);
        var dangNhapCu = await Client.PostAsJsonAsync("/api/v1/auth/nhan-vien/dang-nhap", new { tenDangNhap = nhanVien.TenDangNhap, matKhau = MatKhauNhanVien });
        Assert.Equal(HttpStatusCode.Unauthorized, dangNhapCu.StatusCode);
        await DangNhapNhanVienAsync(nhanVien.TenDangNhap, matKhauMoi);

        var matKhauBam = await TruyVanDonAsync<string>($"""
            SELECT mat_khau_bam AS "Value" FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {nhanVien.Ma}
            """);
        var noiDungNhatKy = await TruyVanDonAsync<string>($"""
            SELECT COALESCE(string_agg(COALESCE(gia_tri_truoc::text, '') || COALESCE(gia_tri_sau::text, ''), ' '), '') AS "Value"
            FROM nhat_ky.nhat_ky_he_thong WHERE doi_tuong_id = {nhanVien.Ma.ToString()}
            """);
        Assert.DoesNotContain(matKhauMoi, noiDungNhatKy);
        Assert.DoesNotContain(matKhauBam, noiDungNhatKy);
        Assert.DoesNotContain("MatKhauBam", noiDungNhatKy);
    }

    [Fact]
    public async Task UC14_DatLaiMatKhau_NganHonTamKyTu_Tra400_VaMaKhongTonTaiTra404()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();

        var ngan = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/dat-lai-mat-khau", TokenQuanTri, new { matKhauMoi = "1234567" });
        var khongTonTai = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{Guid.NewGuid()}/dat-lai-mat-khau", TokenQuanTri, new { matKhauMoi = "MatKhauMoi@456" });

        Assert.Equal(HttpStatusCode.BadRequest, ngan.StatusCode);
        Assert.True((await DocJsonAsync(ngan)).GetProperty("errors").TryGetProperty("matKhauMoi", out _));
        Assert.Equal(HttpStatusCode.NotFound, khongTonTai.StatusCode);
        await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien);
    }

    [Fact]
    public async Task NFR01_KhoaMoKhoaDatLaiMatKhau_NhanVienGoiBiTuChoi403()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        var tokenNhanVien = (await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien)).AccessToken;

        var khoa = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/khoa", tokenNhanVien);
        var datLai = await GuiAsync(HttpMethod.Post, $"{DuongDan}/{nhanVien.Ma}/dat-lai-mat-khau", tokenNhanVien, new { matKhauMoi = "MatKhauMoi@456" });

        Assert.Equal(HttpStatusCode.Forbidden, khoa.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, datLai.StatusCode);
        await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien);
    }
}

