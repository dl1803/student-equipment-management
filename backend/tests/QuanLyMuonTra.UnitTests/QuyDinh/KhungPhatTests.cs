using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.QuyDinh;

public sealed class KhungPhatTests
{
    [Fact]
    public void UC13_TaoKhungPhat_TyLeThapPhan12Phay5_GiuNguyenVaMacDinhDangSuDung()
    {
        var khung = new KhungPhat("Thử 12,5%", 12.50m);

        Assert.NotEqual(Guid.Empty, khung.MaKhung);
        Assert.Equal("Thử 12,5%", khung.TenKhung);
        Assert.Equal(12.5m, khung.TyLePhanTram);
        Assert.True(khung.DangSuDung);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(15)]
    [InlineData(99.99)]
    [InlineData(100)]
    public void UC13_TaoKhungPhat_TyLeTrongKhoangBienHopLe_ChapNhan(double tyLe)
    {
        var khung = new KhungPhat("Khung", (decimal)tyLe);

        Assert.Equal((decimal)tyLe, khung.TyLePhanTram);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.01)]
    [InlineData(101)]
    public void UC13_TaoKhungPhat_TyLeNgoaiKhoang0Den100_NemGiaTriCauHinhKhongHopLe(double tyLe)
    {
        var loi = Assert.Throws<NghiepVuException>(() => new KhungPhat("Khung", (decimal)tyLe));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Theory]
    [InlineData(12.555)]
    [InlineData(0.001)]
    public void UC13_TaoKhungPhat_TyLeQuaHaiChuSoThapPhan_NemGiaTriCauHinhKhongHopLe(double tyLe)
    {
        var loi = Assert.Throws<NghiepVuException>(() => new KhungPhat("Khung", (decimal)tyLe));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UC13_TaoKhungPhat_TenRongHoacToanKhoangTrang_NemGiaTriCauHinhKhongHopLe(string ten)
    {
        var loi = Assert.Throws<NghiepVuException>(() => new KhungPhat(ten, 15m));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void UC13_CapNhat_GiaTriHopLe_DoiTenVaTyLeVaGiuNguyenMaKhung()
    {
        var khung = new KhungPhat("Nhẹ", 15m);
        var ma = khung.MaKhung;

        khung.CapNhat("Nhẹ mới", 17.25m);

        Assert.Equal(ma, khung.MaKhung);
        Assert.Equal("Nhẹ mới", khung.TenKhung);
        Assert.Equal(17.25m, khung.TyLePhanTram);
    }

    [Fact]
    public void UC13_CapNhat_GiaTriSai_NemVaKhongDoiGiaTriCu()
    {
        var khung = new KhungPhat("Nhẹ", 15m);

        var loi = Assert.Throws<NghiepVuException>(() => khung.CapNhat("Nhẹ mới", 150m));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal("Nhẹ", khung.TenKhung);
        Assert.Equal(15m, khung.TyLePhanTram);
    }

    [Fact]
    public void UC13_Tat_KhungDangSuDung_SangTatVaGiuNguyenTyLe()
    {
        var khung = new KhungPhat("Vừa", 40m);

        khung.Tat();

        Assert.False(khung.DangSuDung);
        Assert.Equal(40m, khung.TyLePhanTram);
    }

    [Fact]
    public void UC13_Tat_LanHai_VanTat()
    {
        var khung = new KhungPhat("Nặng", 80m);
        khung.Tat();

        khung.Tat();

        Assert.False(khung.DangSuDung);
    }
}
