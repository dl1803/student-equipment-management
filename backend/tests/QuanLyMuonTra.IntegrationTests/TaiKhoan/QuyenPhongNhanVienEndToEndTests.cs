using System.Net;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public sealed class QuyenPhongNhanVienEndToEndTests : NhanVienQuanTriTestBase
{
    [Fact]
    public async Task UC14_CapQuyenPhong_CoHieuLucNgay_KhongCho30GiayDem()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        var phong = await ThemPhongAsync();
        var tokenNhanVien = (await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien)).AccessToken;

        var truoc = await GuiAsync(HttpMethod.Get, "/api/v1/auth/toi", tokenNhanVien);
        Assert.Empty((await DocJsonAsync(truoc)).GetProperty("dsPhong").EnumerateArray());

        var capQuyen = await GuiAsync(HttpMethod.Put, $"{DuongDan}/{nhanVien.Ma}/quyen-phong", TokenQuanTri, new { dsMaPhong = new[] { phong } });

        Assert.Equal(HttpStatusCode.OK, capQuyen.StatusCode);
        var dsPhongTrongPhanHoi = (await DocJsonAsync(capQuyen)).GetProperty("dsPhong").EnumerateArray().ToList();
        Assert.Equal(phong.ToString(), Assert.Single(dsPhongTrongPhanHoi).GetProperty("maPhong").GetString());
        var sau = await GuiAsync(HttpMethod.Get, "/api/v1/auth/toi", tokenNhanVien);
        Assert.Equal(
            phong.ToString(),
            Assert.Single((await DocJsonAsync(sau)).GetProperty("dsPhong").EnumerateArray()).GetProperty("maPhong").GetString());
    }

    [Fact]
    public async Task UC14_ThuHoiQuyenPhong_ThayToanBoDanhSach_CoHieuLucNgay()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        var phongA = await ThemPhongAsync();
        var phongB = await ThemPhongAsync();
        var tokenNhanVien = (await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien)).AccessToken;
        await GuiAsync(HttpMethod.Put, $"{DuongDan}/{nhanVien.Ma}/quyen-phong", TokenQuanTri, new { dsMaPhong = new[] { phongA, phongB } });
        var dayDu = await GuiAsync(HttpMethod.Get, "/api/v1/auth/toi", tokenNhanVien);
        Assert.Equal(2, (await DocJsonAsync(dayDu)).GetProperty("dsPhong").GetArrayLength());

        var chiConB = await GuiAsync(HttpMethod.Put, $"{DuongDan}/{nhanVien.Ma}/quyen-phong", TokenQuanTri, new { dsMaPhong = new[] { phongB } });
        Assert.Equal(HttpStatusCode.OK, chiConB.StatusCode);
        var saThuHoiA = await GuiAsync(HttpMethod.Get, "/api/v1/auth/toi", tokenNhanVien);
        Assert.Equal(
            phongB.ToString(),
            Assert.Single((await DocJsonAsync(saThuHoiA)).GetProperty("dsPhong").EnumerateArray()).GetProperty("maPhong").GetString());

        var thuHoiHet = await GuiAsync(HttpMethod.Put, $"{DuongDan}/{nhanVien.Ma}/quyen-phong", TokenQuanTri, new { dsMaPhong = Array.Empty<Guid>() });
        Assert.Equal(HttpStatusCode.OK, thuHoiHet.StatusCode);
        var sauKhiThuHoiHet = await GuiAsync(HttpMethod.Get, "/api/v1/auth/toi", tokenNhanVien);
        Assert.Empty((await DocJsonAsync(sauKhiThuHoiHet)).GetProperty("dsPhong").EnumerateArray());
    }

    [Fact]
    public async Task NFR02_CapVaThuHoiQuyenPhong_GhiNhatKyVoiTacNhanQuanTri()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        var phong = await ThemPhongAsync();
        var maQuanTri = await LayMaNguoiDungQuanTriAsync();
        var doiTuongId = $"{nhanVien.Ma},{phong}";

        await GuiAsync(HttpMethod.Put, $"{DuongDan}/{nhanVien.Ma}/quyen-phong", TokenQuanTri, new { dsMaPhong = new[] { phong } });
        await GuiAsync(HttpMethod.Put, $"{DuongDan}/{nhanVien.Ma}/quyen-phong", TokenQuanTri, new { dsMaPhong = Array.Empty<Guid>() });

        var soDongThem = await TruyVanDonAsync<int>($"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = 'tai_khoan.nhan_vien_phong' AND thao_tac = 'THEM'
              AND doi_tuong_id = {doiTuongId} AND ma_nguoi_dung = {maQuanTri}
            """);
        var soDongXoa = await TruyVanDonAsync<int>($"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = 'tai_khoan.nhan_vien_phong' AND thao_tac = 'XOA'
              AND doi_tuong_id = {doiTuongId} AND ma_nguoi_dung = {maQuanTri}
            """);
        Assert.Equal(1, soDongThem);
        Assert.Equal(1, soDongXoa);
    }

    [Fact]
    public async Task UC14_CapQuyenPhong_DuLieuKhongHopLe_VaMaKhongTonTai()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        var phong = await ThemPhongAsync();
        var duongDan = $"{DuongDan}/{nhanVien.Ma}/quyen-phong";

        var trungLap = await GuiAsync(HttpMethod.Put, duongDan, TokenQuanTri, new { dsMaPhong = new[] { phong, phong } });
        var thieuTruong = await GuiAsync(HttpMethod.Put, duongDan, TokenQuanTri, new { });
        var phongKhongTonTai = await GuiAsync(HttpMethod.Put, duongDan, TokenQuanTri, new { dsMaPhong = new[] { Guid.NewGuid() } });
        var nhanVienKhongTonTai = await GuiAsync(HttpMethod.Put, $"{DuongDan}/{Guid.NewGuid()}/quyen-phong", TokenQuanTri, new { dsMaPhong = new[] { phong } });

        Assert.Equal(HttpStatusCode.BadRequest, trungLap.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, thieuTruong.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, phongKhongTonTai.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, nhanVienKhongTonTai.StatusCode);
        var soQuyen = await TruyVanDonAsync<int>($"""
            SELECT count(*)::int AS "Value" FROM tai_khoan.nhan_vien_phong WHERE ma_nguoi_dung = {nhanVien.Ma}
            """);
        Assert.Equal(0, soQuyen);
    }

    [Fact]
    public async Task NFR01_CapQuyenPhong_NhanVienGoiBiTuChoi403()
    {
        var nhanVien = await TaoNhanVienQuaApiAsync();
        var phong = await ThemPhongAsync();
        var tokenNhanVien = (await DangNhapNhanVienAsync(nhanVien.TenDangNhap, MatKhauNhanVien)).AccessToken;

        var tuCapQuyen = await GuiAsync(HttpMethod.Put, $"{DuongDan}/{nhanVien.Ma}/quyen-phong", tokenNhanVien, new { dsMaPhong = new[] { phong } });

        Assert.Equal(HttpStatusCode.Forbidden, tuCapQuyen.StatusCode);
    }
}

