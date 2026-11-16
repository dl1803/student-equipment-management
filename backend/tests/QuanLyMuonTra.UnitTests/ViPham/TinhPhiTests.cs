using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.ViPham.Domain;

namespace QuanLyMuonTra.UnitTests.ViPham;

public class TinhPhiTests
{
    private const int AnHanPhut = 15;

    private static DateTimeOffset Vn(int ngay, int gio, int phut)
    {
        return new DateTimeOffset(2026, 11, ngay, gio, phut, 0, TimeSpan.FromHours(7));
    }

    [Theory]
    [InlineData(-30, 0)]
    [InlineData(0, 0)]
    [InlineData(15, 0)]
    [InlineData(16, 1)]
    public void UC09_TraSoVoiHanTraCungNgay_ChiTinhTreKhiQuaAnHan15Phut(int phutSauHan, int soNgayMongDoi)
    {
        var hanTra = Vn(16, 10, 0);

        var soNgayTre = TinhPhi.SoNgayTre(hanTra, hanTra.AddMinutes(phutSauHan), AnHanPhut);

        Assert.Equal(soNgayMongDoi, soNgayTre);
    }

    [Fact]
    public void UC09_TraDungMocAnHanQuaNuaDemVn_KhongTinhTre()
    {
        Assert.Equal(0, TinhPhi.SoNgayTre(Vn(16, 23, 50), Vn(17, 0, 5), AnHanPhut));
    }

    [Fact]
    public void UC09_TraQuaAnHanMotPhutQuaNuaDemVn_TinhMotNgay()
    {
        Assert.Equal(1, TinhPhi.SoNgayTre(Vn(16, 23, 50), Vn(17, 0, 6), AnHanPhut));
    }

    [Fact]
    public void UC09_HanVaTraCungNgayUtcKhacNgayVn_TinhMotNgay()
    {
        var hanTra = new DateTimeOffset(2026, 11, 16, 16, 0, 0, TimeSpan.Zero);
        var thoiDiemTra = new DateTimeOffset(2026, 11, 16, 17, 30, 0, TimeSpan.Zero);

        Assert.Equal(1, TinhPhi.SoNgayTre(hanTra, thoiDiemTra, AnHanPhut));
    }

    [Fact]
    public void UC09_HaiNgayLichVnNhungMotNgayLichUtc_TinhHaiNgay()
    {
        var hanTra = Vn(16, 23, 30);
        var thoiDiemTra = Vn(18, 0, 10);

        Assert.Equal(2, TinhPhi.SoNgayTre(hanTra, thoiDiemTra, AnHanPhut));
    }

    [Fact]
    public void UC09_TraTreBaNgayLich_TinhBaNgay()
    {
        Assert.Equal(3, TinhPhi.SoNgayTre(Vn(16, 10, 0), Vn(19, 9, 0), AnHanPhut));
    }

    [Fact]
    public void UC09_TraTungPhanOHaiThoiDiemKhacNhau_MoiThietBiTinhSoNgayTreRieng()
    {
        var hanTra = Vn(16, 10, 0);

        Assert.Equal(2, TinhPhi.SoNgayTre(hanTra, Vn(18, 9, 0), AnHanPhut));
        Assert.Equal(4, TinhPhi.SoNgayTre(hanTra, Vn(20, 9, 0), AnHanPhut));
    }

    [Fact]
    public void NFR09_LaptopVaMayChieuTreBaNgay_TongHaiTramBonMuoiNghinDong()
    {
        var phiLaptop = TinhPhi.PhiTre(3, 50000m, 20000000m);
        var phiMayChieu = TinhPhi.PhiTre(3, 30000m, 10000000m);

        Assert.Equal(240000m, phiLaptop + phiMayChieu);
    }

    [Fact]
    public void UC09_PhiTreVuotGiaTriHienHanh_ChiTinhBangGiaTriHienHanh()
    {
        Assert.Equal(120000m, TinhPhi.PhiTre(3, 50000m, 120000m));
    }

    [Fact]
    public void UC09_PhiTreChamDungGiaTriHienHanh_GiuNguyenPhi()
    {
        Assert.Equal(150000m, TinhPhi.PhiTre(3, 50000m, 150000m));
    }

    [Fact]
    public void UC09_TreTrongAnHan_PhiBangKhong()
    {
        var soNgayTre = TinhPhi.SoNgayTre(Vn(16, 10, 0), Vn(16, 10, 15), AnHanPhut);

        Assert.Equal(0m, TinhPhi.PhiTre(soNgayTre, 50000m, 1000000m));
    }

    [Fact]
    public void NFR09_DoiPhiLoaiSauKhiGiao_PhiTinhTheoPhiLuuTrongChiTietPhieu()
    {
        const decimal phiLoaiSauKhiDoi = 70000m;
        var chiTiet = new ChiTietPhieuMuon(Guid.NewGuid(), "PM-261116-000001", "TB-1", "Tốt", 50000m);

        var phi = TinhPhi.PhiTre(3, chiTiet.PhiTreMoiNgay, 10000000m);

        Assert.Equal(150000m, phi);
        Assert.NotEqual(TinhPhi.PhiTre(3, phiLoaiSauKhiDoi, 10000000m), phi);
    }

    [Theory]
    [InlineData(1000000, 15, 150000)]
    [InlineData(1000000, 40, 400000)]
    [InlineData(1000000, 80, 800000)]
    public void UC09_BoiThuongHuHongTheoKhungPhat_TyLeNhanGiaTriHienHanh(int giaTri, int tyLe, int soTienMongDoi)
    {
        Assert.Equal(soTienMongDoi, TinhPhi.BoiThuongHuHong(giaTri, tyLe));
    }

    [Fact]
    public void NFR09_BoiThuongMuoiHaiPhayNamDong_LamTronLenMuoiBaDong()
    {
        Assert.Equal(13m, TinhPhi.BoiThuongHuHong(100m, 12.5m));
    }

    [Fact]
    public void NFR09_BoiThuongHaiPhayNamDong_LamTronLenBaDong()
    {
        Assert.Equal(3m, TinhPhi.BoiThuongHuHong(100m, 2.5m));
    }

    [Fact]
    public void NFR09_BoiThuongDuoiNuaDong_LamTronXuongMuoiHaiDong()
    {
        Assert.Equal(12m, TinhPhi.BoiThuongHuHong(100m, 12.49m));
    }

    [Fact]
    public void UC09_BoiThuongMat_BangTronGiaTriHienHanh()
    {
        Assert.Equal(8000000m, TinhPhi.BoiThuongMat(8000000m));
    }
}
