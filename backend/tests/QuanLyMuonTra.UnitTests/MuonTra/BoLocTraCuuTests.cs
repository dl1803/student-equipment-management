using QuanLyMuonTra.Modules.MuonTra.Application;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class BoLocTraCuuTests
{
    private static readonly Guid L1 = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
    private static readonly Guid L2 = Guid.Parse("00000000-0000-0000-0000-0000000000a2");
    private static readonly Guid P1 = Guid.Parse("00000000-0000-0000-0000-0000000000b1");
    private static readonly Guid P2 = Guid.Parse("00000000-0000-0000-0000-0000000000b2");

    private static IQueryable<DongTraCuuThietBi> DuLieu()
    {
        return new List<DongTraCuuThietBi>
        {
            Dong(L1, "Máy chiếu", P1, 3),
            Dong(L1, "Máy chiếu", P2, 0),
            Dong(L2, "Laptop Dell", P1, 1),
        }.AsQueryable();
    }

    private static DongTraCuuThietBi Dong(Guid maLoai, string tenLoai, Guid maPhong, int soLuong)
    {
        return new DongTraCuuThietBi
        {
            MaLoai = maLoai,
            TenLoai = tenLoai,
            MaPhong = maPhong,
            TenPhong = "Phòng",
            CoSo = "Cơ sở",
            SoLuongKhaDung = soLuong,
        };
    }

    [Fact]
    public void KhongBoLoc_GiuNguyenTatCa()
    {
        var ketQua = TraCuuTruyVan.ApDungBoLoc(DuLieu(), null, null, null, null).ToList();

        Assert.Equal(3, ketQua.Count);
    }

    [Theory]
    [InlineData("laptop", 1)]
    [InlineData("  LAPTOP  ", 1)]
    [InlineData("máy", 2)]
    [InlineData("khong-co", 0)]
    [InlineData("   ", 3)]
    public void LocTheoTen_ChuaChuoiKhongPhanBietHoaThuong(string ten, int soDong)
    {
        var ketQua = TraCuuTruyVan.ApDungBoLoc(DuLieu(), ten, null, null, null).ToList();

        Assert.Equal(soDong, ketQua.Count);
    }

    [Fact]
    public void LocTheoMaLoaiVaMaPhong_GiaoNhau()
    {
        var ketQua = TraCuuTruyVan.ApDungBoLoc(DuLieu(), null, L1, P2, null).ToList();

        var dong = Assert.Single(ketQua);
        Assert.Equal(L1, dong.MaLoai);
        Assert.Equal(P2, dong.MaPhong);
    }

    [Fact]
    public void ConHangTrue_ChiLayKhaDungLonHonKhong()
    {
        var ketQua = TraCuuTruyVan.ApDungBoLoc(DuLieu(), null, null, null, true).ToList();

        Assert.Equal(2, ketQua.Count);
        Assert.All(ketQua, d => Assert.True(d.SoLuongKhaDung > 0));
    }

    [Fact]
    public void ConHangFalse_ChiLayHetHang()
    {
        var ketQua = TraCuuTruyVan.ApDungBoLoc(DuLieu(), null, null, null, false).ToList();

        var dong = Assert.Single(ketQua);
        Assert.Equal(0, dong.SoLuongKhaDung);
    }
}
