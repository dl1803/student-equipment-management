using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.QuyDinh;

public sealed class ThamSoHeThongTests
{
    private static ThamSoHeThong TaoMacDinh()
    {
        return new ThamSoHeThong(3, 30, 15, 2, 3, 5, null, null);
    }

    [Fact]
    public void UC13_KhoiTao_GiaTriMacDinhSrs33_GioQuayChuaCauHinh()
    {
        var thamSo = TaoMacDinh();

        Assert.Equal((short)1, thamSo.Id);
        Assert.Equal(3, thamSo.SoNgayKhoa);
        Assert.Equal(30, thamSo.ThoiGianGiuChoPhut);
        Assert.Equal(15, thamSo.AnHanPhut);
        Assert.Equal(2, thamSo.SoGioGiaHanToiDa);
        Assert.Equal(3, thamSo.NguongViPham);
        Assert.Equal(5, thamSo.SoLanDangNhapSaiToiDa);
        Assert.Null(thamSo.GioMoCua);
        Assert.Null(thamSo.GioDongCua);
    }

    [Fact]
    public void UC13_KhoiTao_SoNgayKhoaVaAnHanBang0_ChapNhan()
    {
        var thamSo = new ThamSoHeThong(0, 30, 0, 2, 3, 5, null, null);

        Assert.Equal(0, thamSo.SoNgayKhoa);
        Assert.Equal(0, thamSo.AnHanPhut);
    }

    [Theory]
    [InlineData(-1, 30, 15, 2, 3, 5)]
    [InlineData(3, 0, 15, 2, 3, 5)]
    [InlineData(3, 30, -1, 2, 3, 5)]
    [InlineData(3, 30, 15, 0, 3, 5)]
    [InlineData(3, 30, 15, 2, 0, 5)]
    [InlineData(3, 30, 15, 2, 3, 0)]
    public void UC13_KhoiTao_MotThamSoSoNguyenNgoaiMienGiaTri_NemGiaTriCauHinhKhongHopLe(
        int soNgayKhoa,
        int thoiGianGiuCho,
        int anHan,
        int soGioGiaHan,
        int nguongViPham,
        int soLanDangNhapSai)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() =>
            new ThamSoHeThong(soNgayKhoa, thoiGianGiuCho, anHan, soGioGiaHan, nguongViPham, soLanDangNhapSai, null, null));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void UC13_CapNhat_GioQuayHopLe_LuuCaHaiGio()
    {
        var thamSo = TaoMacDinh();

        thamSo.CapNhat(3, 30, 15, 2, 3, 5, new TimeOnly(7, 30), new TimeOnly(17, 0));

        Assert.Equal(new TimeOnly(7, 30), thamSo.GioMoCua);
        Assert.Equal(new TimeOnly(17, 0), thamSo.GioDongCua);
    }

    [Fact]
    public void UC13_CapNhat_GioQuayVeRongCaHai_XoaCauHinhGioQuay()
    {
        var thamSo = TaoMacDinh();
        thamSo.CapNhat(3, 30, 15, 2, 3, 5, new TimeOnly(7, 30), new TimeOnly(17, 0));

        thamSo.CapNhat(3, 30, 15, 2, 3, 5, null, null);

        Assert.Null(thamSo.GioMoCua);
        Assert.Null(thamSo.GioDongCua);
    }

    [Fact]
    public void UC13_CapNhat_ChiCoGioMoCua_NemGiaTriCauHinhKhongHopLe()
    {
        var thamSo = TaoMacDinh();

        var loi = Assert.Throws<LoiNghiepVuException>(() =>
            thamSo.CapNhat(3, 30, 15, 2, 3, 5, new TimeOnly(7, 30), null));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void UC13_CapNhat_ChiCoGioDongCua_NemGiaTriCauHinhKhongHopLe()
    {
        var thamSo = TaoMacDinh();

        var loi = Assert.Throws<LoiNghiepVuException>(() =>
            thamSo.CapNhat(3, 30, 15, 2, 3, 5, null, new TimeOnly(17, 0)));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Theory]
    [InlineData(17, 0, 7, 30)]
    [InlineData(8, 0, 8, 0)]
    public void UC13_CapNhat_GioMoCuaKhongNhoHonGioDongCua_NemGiaTriCauHinhKhongHopLe(
        int gioMo,
        int phutMo,
        int gioDong,
        int phutDong)
    {
        var thamSo = TaoMacDinh();

        var loi = Assert.Throws<LoiNghiepVuException>(() =>
            thamSo.CapNhat(3, 30, 15, 2, 3, 5, new TimeOnly(gioMo, phutMo), new TimeOnly(gioDong, phutDong)));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void UC13_CapNhat_GiaTriSai_NemVaKhongDoiGiaTriCu()
    {
        var thamSo = TaoMacDinh();

        Assert.Throws<LoiNghiepVuException>(() =>
            thamSo.CapNhat(7, 45, 20, 4, 6, 8, new TimeOnly(17, 0), new TimeOnly(7, 30)));

        Assert.Equal(3, thamSo.SoNgayKhoa);
        Assert.Equal(30, thamSo.ThoiGianGiuChoPhut);
        Assert.Equal(15, thamSo.AnHanPhut);
        Assert.Equal(2, thamSo.SoGioGiaHanToiDa);
        Assert.Equal(3, thamSo.NguongViPham);
        Assert.Equal(5, thamSo.SoLanDangNhapSaiToiDa);
        Assert.Null(thamSo.GioMoCua);
        Assert.Null(thamSo.GioDongCua);
    }
}
