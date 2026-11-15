using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.QuyDinh;

public sealed class QuyDinhMuonTests
{
    private static readonly Guid MayChieu = Guid.Parse("019c0000-0000-7000-8000-000000000012");

    [Fact]
    public void UC13_TaoChung_DaiHoc2ThietBi5Gio_CoSoThietBiVaKhongCoLoai()
    {
        var quyDinh = QuyDinhMuon.TaoChung("DAI_HOC", 2, 5);

        Assert.NotEqual(Guid.Empty, quyDinh.MaQuyDinh);
        Assert.Equal("DAI_HOC", quyDinh.MaNhom);
        Assert.Null(quyDinh.MaLoai);
        Assert.Equal(2, quyDinh.SoThietBiToiDa);
        Assert.Equal(5, quyDinh.ThoiGianMuonToiDaGio);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-1, 5)]
    [InlineData(2, 0)]
    [InlineData(2, -5)]
    public void UC13_TaoChung_SoThietBiHoacThoiGianKhongDuong_NemGiaTriCauHinhKhongHopLe(int soThietBi, int gio)
    {
        var loi = Assert.Throws<NghiepVuException>(() => QuyDinhMuon.TaoChung("DAI_HOC", soThietBi, gio));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void UC13_TaoChung_MaNhomRong_NemGiaTriCauHinhKhongHopLe(string maNhom)
    {
        var loi = Assert.Throws<NghiepVuException>(() => QuyDinhMuon.TaoChung(maNhom, 2, 5));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void UC13_TaoNgoaiLe_DaiHocMayChieu3Gio_CoLoaiVaKhongCoSoThietBi()
    {
        var quyDinh = QuyDinhMuon.TaoNgoaiLe("DAI_HOC", MayChieu, 3);

        Assert.NotEqual(Guid.Empty, quyDinh.MaQuyDinh);
        Assert.Equal("DAI_HOC", quyDinh.MaNhom);
        Assert.Equal(MayChieu, quyDinh.MaLoai);
        Assert.Null(quyDinh.SoThietBiToiDa);
        Assert.Equal(3, quyDinh.ThoiGianMuonToiDaGio);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UC13_TaoNgoaiLe_ThoiGianKhongDuong_NemGiaTriCauHinhKhongHopLe(int gio)
    {
        var loi = Assert.Throws<NghiepVuException>(() => QuyDinhMuon.TaoNgoaiLe("DAI_HOC", MayChieu, gio));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void UC13_TaoNgoaiLe_MaNhomRong_NemGiaTriCauHinhKhongHopLe()
    {
        var loi = Assert.Throws<NghiepVuException>(() => QuyDinhMuon.TaoNgoaiLe(" ", MayChieu, 3));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }
}
