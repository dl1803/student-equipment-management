using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Api.SinhVien;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class LayHoSoSinhVienHandlerTests
{
    private static LayHoSoSinhVienHandler LayBoXuLy(PhienKiemThuHoSo phien) =>
        phien.DichVu.GetRequiredService<LayHoSoSinhVienHandler>();

    private static string[] LayMaLoi(SinhVienTheInfoResponse hoSo) =>
        hoSo.DsLyDoKhongHopLe.Select(x => x.MaLoi).ToArray();

    private static Task CapNhatSinhVienAsync(PhienKiemThuHoSo phien, string maSv, DateOnly hanThe, string trangThaiHoc, string trangThaiQuyenMuon) =>
        phien.Db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE sinh_vien.sinh_vien
            SET han_the = {hanThe}, trang_thai_hoc = {trangThaiHoc}, trang_thai_quyen_muon = {trangThaiQuyenMuon}
            WHERE ma_sv = {maSv}
            """);

    [Fact]
    public async Task UC07_QuetThe_DangCam3GioiHan2_KhongHopLeVuotGioiHan()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);

        var hoSo = await LayBoXuLy(phien).XuLyAsync("D02-SV-A", CancellationToken.None);

        Assert.NotNull(hoSo);
        Assert.Equal(3, hoSo.SoThietBiDangMuon);
        Assert.Equal(2, hoSo.SoThietBiToiDa);
        Assert.False(hoSo.HopLe);
        Assert.Equal(new[] { "VUOT_GIOI_HAN_THIET_BI" }, LayMaLoi(hoSo));
    }

    [Fact]
    public async Task UC07_QuetThe_DangCam1GioiHan2_HopLe()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);

        var hoSo = await LayBoXuLy(phien).XuLyAsync("D02-SV-B", CancellationToken.None);

        Assert.NotNull(hoSo);
        Assert.Equal(1, hoSo.SoThietBiDangMuon);
        Assert.True(hoSo.HopLe);
        Assert.Empty(hoSo.DsLyDoKhongHopLe);
        Assert.Equal("DAI_HOC", hoSo.MaNhom);
        Assert.False(string.IsNullOrWhiteSpace(hoSo.TenNhom));
    }

    [Fact]
    public async Task UC07_QuetThe_DaCamDuToiDa_KhongHopLe_K16()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(1);

        var hoSo = await LayBoXuLy(phien).XuLyAsync("D02-SV-B", CancellationToken.None);

        Assert.NotNull(hoSo);
        Assert.Equal(1, hoSo.SoThietBiDangMuon);
        Assert.Equal(1, hoSo.SoThietBiToiDa);
        Assert.False(hoSo.HopLe);
        Assert.Equal(new[] { "VUOT_GIOI_HAN_THIET_BI" }, LayMaLoi(hoSo));
    }

    [Fact]
    public async Task UC17_XemHoSoToi_DaCamDuToiDa_VanHopLe_KhiSoMuonThemBang0()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(1);

        var hoSo = await LayBoXuLy(phien).XuLyAsync("D02-SV-B", soMuonThem: 0, CancellationToken.None);

        Assert.NotNull(hoSo);
        Assert.Equal(1, hoSo.SoThietBiDangMuon);
        Assert.Equal(1, hoSo.SoThietBiToiDa);
        Assert.True(hoSo.HopLe);
        Assert.Empty(hoSo.DsLyDoKhongHopLe);
    }

    [Fact]
    public async Task UC07_QuetThe_CoNoBienBan_HienThiNoNhungKhongTaoLyDoRieng()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);

        var hoSo = await LayBoXuLy(phien).XuLyAsync("D02-SV-C", CancellationToken.None);

        Assert.NotNull(hoSo);
        Assert.Equal(0, hoSo.SoThietBiDangMuon);
        var bienBan = Assert.Single(hoSo.DsBienBanChuaNop);
        Assert.Equal(20000L, bienBan.TongTien);
        Assert.True(hoSo.HopLe);
        Assert.Empty(hoSo.DsLyDoKhongHopLe);
    }

    [Fact]
    public async Task UC07_QuetThe_HetHanHetHocKhoaQuyen_NhieuLyDoTheoThuTu()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);
        var homNay = VnTime.NgayLich(DateTimeOffset.UtcNow);
        await CapNhatSinhVienAsync(phien, "D02-SV-D", homNay.AddDays(-1), "THOI_HOC", "TAM_KHOA");

        var hoSo = await LayBoXuLy(phien).XuLyAsync("D02-SV-D", CancellationToken.None);

        Assert.NotNull(hoSo);
        Assert.False(hoSo.HopLe);
        Assert.Equal(
            new[] { "SV_THE_HET_HAN", "SV_KHONG_CON_THEO_HOC", "SV_BI_KHOA_QUYEN_MUON" },
            LayMaLoi(hoSo));
        Assert.All(hoSo.DsLyDoKhongHopLe, lyDo => Assert.False(string.IsNullOrWhiteSpace(lyDo.ThongDiep)));
    }

    [Fact]
    public async Task UC07_QuetThe_CamMuonDaiHan_KhongHopLeKhoaQuyenMuon()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);
        var homNay = VnTime.NgayLich(DateTimeOffset.UtcNow);
        await CapNhatSinhVienAsync(phien, "D02-SV-D", homNay.AddYears(1), "DANG_HOC", "CAM_MUON_DAI_HAN");

        var hoSo = await LayBoXuLy(phien).XuLyAsync("D02-SV-D", CancellationToken.None);

        Assert.NotNull(hoSo);
        Assert.Equal("CAM_MUON_DAI_HAN", hoSo.TrangThaiQuyenMuon);
        Assert.Equal(new[] { "SV_BI_KHOA_QUYEN_MUON" }, LayMaLoi(hoSo));
    }

    [Fact]
    public async Task UC07_QuetThe_HanTheDungHomNay_VanHopLe_K19()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);
        var homNay = VnTime.NgayLich(DateTimeOffset.UtcNow);
        await CapNhatSinhVienAsync(phien, "D02-SV-D", homNay, "DANG_HOC", "BINH_THUONG");

        var hoSo = await LayBoXuLy(phien).XuLyAsync("D02-SV-D", CancellationToken.None);

        Assert.NotNull(hoSo);
        Assert.True(hoSo.HopLe);
    }

    [Fact]
    public async Task UC07_QuetThe_MaCoKhoangTrangTimDuoc_MaKhongTonTaiTraNull()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);
        var boXuLy = LayBoXuLy(phien);

        var coKhoangTrang = await boXuLy.XuLyAsync("  D02-SV-A  ", CancellationToken.None);
        var khongTonTai = await boXuLy.XuLyAsync("D02-SV-KHONG-TON-TAI", CancellationToken.None);

        Assert.NotNull(coKhoangTrang);
        Assert.Equal("D02-SV-A", coKhoangTrang.MaSv);
        Assert.Null(khongTonTai);
    }
}

