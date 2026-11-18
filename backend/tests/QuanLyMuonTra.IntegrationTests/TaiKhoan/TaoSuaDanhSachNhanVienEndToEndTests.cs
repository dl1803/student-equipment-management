using System.Net;
using System.Text.Json;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public sealed class TaoSuaDanhSachNhanVienEndToEndTests : NhanVienQuanTriTestBase
{
    [Fact]
    public async Task UC14_TaoNhanVien_Tra201_DangNhapDuocVaMatKhauKhongLuuDangRo()
    {
        var hau = Guid.NewGuid().ToString("N")[..12];
        var tenDangNhap = $"nv-d06k-{hau}";

        var phanHoi = await GuiAsync(
            HttpMethod.Post,
            DuongDan,
            TokenQuanTri,
            new { tenDangNhap, matKhau = MatKhauNhanVien, hoTen = "Nhan Vien Moi", maNV = $"NV-D06K-{hau}", email = "nv@truong.local" });

        Assert.Equal(HttpStatusCode.Created, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        var ma = Guid.Parse(json.GetProperty("maNguoiDung").GetString()!);
        Assert.Equal($"NV-D06K-{hau}", json.GetProperty("maNV").GetString());
        Assert.Equal(tenDangNhap, json.GetProperty("tenDangNhap").GetString());
        Assert.Equal("nv@truong.local", json.GetProperty("email").GetString());
        Assert.Equal("HOAT_DONG", json.GetProperty("trangThaiKhoa").GetString());
        Assert.Empty(json.GetProperty("dsPhong").EnumerateArray());
        Assert.False(json.TryGetProperty("matKhau", out _));
        Assert.False(json.TryGetProperty("matKhauBam", out _));

        await DangNhapNhanVienAsync(tenDangNhap, MatKhauNhanVien);
        var matKhauBam = await TruyVanDonAsync<string>($"""
            SELECT mat_khau_bam AS "Value" FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {ma}
            """);
        Assert.NotEqual(MatKhauNhanVien, matKhauBam);
        Assert.DoesNotContain(MatKhauNhanVien, matKhauBam);

        await ThucThiAsync($"DELETE FROM tai_khoan.refresh_token WHERE chu_so_huu_ma = {ma.ToString()}");
        await ThucThiAsync($"DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung = {ma}");
        await ThucThiAsync($"DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {ma}");
    }

    [Fact]
    public async Task UC14_TaoNhanVien_TrungTenDangNhap_Tra409()
    {
        var daCo = await TaoNhanVienQuaApiAsync();

        var phanHoi = await GuiAsync(
            HttpMethod.Post,
            DuongDan,
            TokenQuanTri,
            new { tenDangNhap = daCo.TenDangNhap, matKhau = MatKhauNhanVien, hoTen = "Trung Ten", maNV = $"NV-KHAC-{Guid.NewGuid():N}"[..20], email = (string?)null });

        Assert.Equal(HttpStatusCode.Conflict, phanHoi.StatusCode);
        Assert.Equal("TEN_DANG_NHAP_TRUNG", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task UC14_TaoNhanVien_TrungMaNv_Tra409()
    {
        var daCo = await TaoNhanVienQuaApiAsync();

        var phanHoi = await GuiAsync(
            HttpMethod.Post,
            DuongDan,
            TokenQuanTri,
            new { tenDangNhap = $"nv-khac-{Guid.NewGuid():N}", matKhau = MatKhauNhanVien, hoTen = "Trung Ma", maNV = daCo.MaNv, email = (string?)null });

        Assert.Equal(HttpStatusCode.Conflict, phanHoi.StatusCode);
        Assert.Equal("MA_NV_TRUNG", (await DocJsonAsync(phanHoi)).GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task UC14_TaoNhanVien_MatKhauNganHonTamKyTu_Tra400_KhongTaoTaiKhoan()
    {
        var tenDangNhap = $"nv-ngan-{Guid.NewGuid():N}";

        var phanHoi = await GuiAsync(
            HttpMethod.Post,
            DuongDan,
            TokenQuanTri,
            new { tenDangNhap, matKhau = "1234567", hoTen = "Mat Khau Ngan", maNV = $"NV-NGAN-{Guid.NewGuid():N}"[..20], email = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", json.GetProperty("maLoi").GetString());
        Assert.True(json.GetProperty("errors").TryGetProperty("matKhau", out _));
        var soDong = await TruyVanDonAsync<int>($"""
            SELECT count(*)::int AS "Value" FROM tai_khoan.nguoi_dung_he_thong WHERE ten_dang_nhap = {tenDangNhap}
            """);
        Assert.Equal(0, soDong);
    }

    [Fact]
    public async Task UC14_TaoNhanVien_EmailSaiDinhDang_Tra400()
    {
        var phanHoi = await GuiAsync(
            HttpMethod.Post,
            DuongDan,
            TokenQuanTri,
            new { tenDangNhap = $"nv-email-{Guid.NewGuid():N}", matKhau = MatKhauNhanVien, hoTen = "Email Sai", maNV = $"NV-EMAIL-{Guid.NewGuid():N}"[..20], email = "khong-co-a-cong" });

        Assert.Equal(HttpStatusCode.BadRequest, phanHoi.StatusCode);
        Assert.True((await DocJsonAsync(phanHoi)).GetProperty("errors").TryGetProperty("email", out _));
    }

    [Fact]
    public async Task NFR01_QuanLyNhanVien_ChiQuanTriDuocGoi()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        var tokenNhanVien = (await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien)).AccessToken;

        var khongToken = await GuiAsync(HttpMethod.Get, DuongDan, null);
        var tokenNv = await GuiAsync(HttpMethod.Get, DuongDan, tokenNhanVien);
        var tokenNvTao = await GuiAsync(HttpMethod.Post, DuongDan, tokenNhanVien, new { tenDangNhap = "x", matKhau = "12345678", hoTen = "x", maNV = "x", email = (string?)null });

        Assert.Equal(HttpStatusCode.Unauthorized, khongToken.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, tokenNv.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, tokenNvTao.StatusCode);
    }

    [Fact]
    public async Task UC14_SuaNhanVien_DoiHoTenVaEmail_Tra200()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();

        var phanHoi = await GuiAsync(
            HttpMethod.Put,
            $"{DuongDan}/{nhanVien.Ma}",
            TokenQuanTri,
            new { hoTen = "Ho Ten Da Sua", email = "da-sua@truong.local" });

        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var json = await DocJsonAsync(phanHoi);
        Assert.Equal("Ho Ten Da Sua", json.GetProperty("hoTen").GetString());
        Assert.Equal("da-sua@truong.local", json.GetProperty("email").GetString());
        Assert.Equal(nhanVien.TenDangNhap, json.GetProperty("tenDangNhap").GetString());
    }

    [Fact]
    public async Task UC14_SuaNhanVien_MaKhongTonTaiHoacLaQuanTri_Tra404()
    {
        var maQuanTri = await LayMaNguoiDungQuanTriAsync();

        var khongTonTai = await GuiAsync(HttpMethod.Put, $"{DuongDan}/{Guid.NewGuid()}", TokenQuanTri, new { hoTen = "A", email = (string?)null });
        var laQuanTri = await GuiAsync(HttpMethod.Put, $"{DuongDan}/{maQuanTri}", TokenQuanTri, new { hoTen = "A", email = (string?)null });
        var maSaiDinhDang = await GuiAsync(HttpMethod.Put, $"{DuongDan}/khong-phai-guid", TokenQuanTri, new { hoTen = "A", email = (string?)null });

        Assert.Equal(HttpStatusCode.NotFound, khongTonTai.StatusCode);
        Assert.Equal("KHONG_TIM_THAY", (await DocJsonAsync(khongTonTai)).GetProperty("maLoi").GetString());
        Assert.Equal(HttpStatusCode.NotFound, laQuanTri.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, maSaiDinhDang.StatusCode);
    }

    [Fact]
    public async Task UC14_DanhSachNhanVien_TimThayNhanVienVuaTaoVaKiemTraThamSo()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();

        var phanHoi = await GuiAsync(HttpMethod.Get, $"{DuongDan}?trang=1&kichThuoc=1", TokenQuanTri);
        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        var trang1 = await DocJsonAsync(phanHoi);
        Assert.Equal(1, trang1.GetProperty("trang").GetInt32());
        Assert.Equal(1, trang1.GetProperty("kichThuoc").GetInt32());
        Assert.True(trang1.GetProperty("tongSo").GetInt32() >= 1);
        Assert.Single(trang1.GetProperty("danhSach").EnumerateArray());

        Assert.True(await CoTrongDanhSachAsync(nhanVien.Ma));

        var trangKhong = await GuiAsync(HttpMethod.Get, $"{DuongDan}?trang=0&kichThuoc=20", TokenQuanTri);
        var kichThuocQuaLon = await GuiAsync(HttpMethod.Get, $"{DuongDan}?trang=1&kichThuoc=101", TokenQuanTri);
        Assert.Equal(HttpStatusCode.BadRequest, trangKhong.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, kichThuocQuaLon.StatusCode);
    }

    private async Task<bool> CoTrongDanhSachAsync(Guid ma)
    {
        for (var trang = 1; ; trang++)
        {
            var phanHoi = await GuiAsync(HttpMethod.Get, $"{DuongDan}?trang={trang}&kichThuoc=100", TokenQuanTri);
            Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
            var danhSach = (await DocJsonAsync(phanHoi)).GetProperty("danhSach").EnumerateArray().ToList();
            if (danhSach.Count == 0)
            {
                return false;
            }

            if (danhSach.Any(x => x.GetProperty("maNguoiDung").GetString() == ma.ToString()))
            {
                return true;
            }
        }
    }
}

