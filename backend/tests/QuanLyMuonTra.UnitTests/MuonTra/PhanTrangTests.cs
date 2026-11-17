using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class PhanTrangTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 20)]
    [InlineData(5, 100)]
    public void KiemTra_GiaTriHopLe_KhongNemLoi(int trang, int kichThuoc)
    {
        PhanTrang.KiemTra(trang, kichThuoc);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(1, -5)]
    public void KiemTra_NgoaiKhoang_Nem400DuLieuKhongHopLe(int trang, int kichThuoc)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => PhanTrang.KiemTra(trang, kichThuoc));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(3, 7, 14)]
    public void ViTriBatDau_TinhDungTheoTrangVaKichThuoc(int trang, int kichThuoc, long mongDoi)
    {
        Assert.Equal(mongDoi, PhanTrang.ViTriBatDau(trang, kichThuoc));
    }

    [Fact]
    public void Rong_GiuTongSoVaThongTinTrang()
    {
        var ketQua = PhanTrang.Rong<int>(5, 4, 2);

        Assert.Equal(5, ketQua.TongSo);
        Assert.Equal(4, ketQua.Trang);
        Assert.Equal(2, ketQua.KichThuoc);
        Assert.Empty(ketQua.DanhSach);
    }
}
