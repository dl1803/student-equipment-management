using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class PhongLoaiMienTests
{
    [Fact]
    public void UC12_TaoPhong_DuLieuHopLe_SinhMaVaGiuNguyenGiaTri()
    {
        var phong = new PhongThietBi("Phòng A", "Cơ sở 1");

        Assert.NotEqual(Guid.Empty, phong.MaPhong);
        Assert.Equal("Phòng A", phong.TenPhong);
        Assert.Equal("Cơ sở 1", phong.CoSo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UC12_TaoPhong_TenRong_Nem400(string? tenPhong)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => new PhongThietBi(tenPhong!, "Cơ sở 1"));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UC12_TaoPhong_CoSoRong_Nem400(string? coSo)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => new PhongThietBi("Phòng A", coSo!));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public void UC12_SuaPhong_DuLieuHopLe_DoiTenVaCoSoGiuNguyenMa()
    {
        var phong = new PhongThietBi("Phòng A", "Cơ sở 1");
        var ma = phong.MaPhong;

        phong.CapNhat("Phòng B", "Cơ sở 2");

        Assert.Equal(ma, phong.MaPhong);
        Assert.Equal("Phòng B", phong.TenPhong);
        Assert.Equal("Cơ sở 2", phong.CoSo);
    }

    [Fact]
    public void UC12_SuaPhong_TenRong_Nem400VaKhongDoiDuLieu()
    {
        var phong = new PhongThietBi("Phòng A", "Cơ sở 1");

        var loi = Assert.Throws<LoiNghiepVuException>(() => phong.CapNhat(" ", "Cơ sở 2"));

        Assert.Equal(400, loi.MaHttp);
        Assert.Equal("Phòng A", phong.TenPhong);
        Assert.Equal("Cơ sở 1", phong.CoSo);
    }

    [Fact]
    public void UC12_TaoLoai_DuLieuHopLe_MacDinhConSuDung()
    {
        var loai = new LoaiThietBi("Laptop", "Máy tính xách tay", "Dell", 180, 50000);

        Assert.NotEqual(Guid.Empty, loai.MaLoai);
        Assert.Equal("Laptop", loai.TenLoai);
        Assert.Equal("Máy tính xách tay", loai.MoTa);
        Assert.Equal("Dell", loai.HangSanXuat);
        Assert.Equal(180, loai.ChuKyBaoTri);
        Assert.Equal(50000m, loai.PhiTreMoiNgay);
        Assert.False(loai.NgungSuDung);
    }

    [Fact]
    public void UC12_TaoLoai_PhiBangKhong_ChoPhep()
    {
        var loai = new LoaiThietBi("Cáp", null, null, 30, 0);

        Assert.Equal(0m, loai.PhiTreMoiNgay);
    }

    [Theory]
    [InlineData("", 30, 0)]
    [InlineData("   ", 30, 0)]
    [InlineData("Laptop", 0, 0)]
    [InlineData("Laptop", -1, 0)]
    [InlineData("Laptop", 30, -1)]
    public void UC12_TaoLoai_DuLieuSai_Nem400(string tenLoai, int chuKy, long phi)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => new LoaiThietBi(tenLoai, null, null, chuKy, phi));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public void UC12_SuaLoai_KhongDoiPhiVaTrangThaiNgungSuDung()
    {
        var loai = new LoaiThietBi("Laptop", null, null, 180, 50000);

        loai.CapNhat("Laptop mới", "Mô tả", "HP", 90);

        Assert.Equal("Laptop mới", loai.TenLoai);
        Assert.Equal("Mô tả", loai.MoTa);
        Assert.Equal("HP", loai.HangSanXuat);
        Assert.Equal(90, loai.ChuKyBaoTri);
        Assert.Equal(50000m, loai.PhiTreMoiNgay);
        Assert.False(loai.NgungSuDung);
    }

    [Theory]
    [InlineData("", 30)]
    [InlineData("Laptop", 0)]
    public void UC12_SuaLoai_DuLieuSai_Nem400VaKhongDoiDuLieu(string tenLoai, int chuKy)
    {
        var loai = new LoaiThietBi("Laptop", null, null, 180, 50000);

        var loi = Assert.Throws<LoiNghiepVuException>(() => loai.CapNhat(tenLoai, null, null, chuKy));

        Assert.Equal(400, loi.MaHttp);
        Assert.Equal("Laptop", loai.TenLoai);
        Assert.Equal(180, loai.ChuKyBaoTri);
    }
}
