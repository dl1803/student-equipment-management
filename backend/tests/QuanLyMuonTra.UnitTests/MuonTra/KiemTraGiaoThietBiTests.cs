using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class KiemTraGiaoThietBiTests
{
    private static readonly Guid Phong = Guid.Parse("019c0000-0000-7000-8000-0000000d9b01");
    private static readonly Guid LoaiX = Guid.Parse("019c0000-0000-7000-8000-0000000d9c01");
    private static readonly Guid LoaiY = Guid.Parse("019c0000-0000-7000-8000-0000000d9c02");

    private static ThietBiDto ThietBi(
        string tinhTrang = "SAN_SANG",
        bool ngungSuDung = false,
        Guid? maPhong = null,
        Guid? maLoai = null)
    {
        return new ThietBiDto(
            "TB-01",
            maLoai ?? LoaiX,
            maPhong ?? Phong,
            "Thiết bị 01",
            new DateOnly(2026, 1, 1),
            tinhTrang,
            ngungSuDung,
            null,
            null,
            0m);
    }

    [Fact]
    public void BR40_MaKhongTrung_KhongLoi()
    {
        KiemTraGiaoThietBi.KiemTraKhongTrungMa(["TB-01", "TB-02", "TB-03"]);
    }

    [Fact]
    public void BR40_MaTrung_Tra400()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraGiaoThietBi.KiemTraKhongTrungMa(["TB-01", "TB-02", "TB-01"]));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public void BR40_ThietBiHopLe_TraVeChinhThietBi()
    {
        var thietBi = ThietBi();

        var ketQua = KiemTraGiaoThietBi.KiemTraThietBi(thietBi, "TB-01", Phong, [LoaiX, LoaiY]);

        Assert.Same(thietBi, ketQua);
    }

    [Fact]
    public void BR40_KhongTonTai_Tra404()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraGiaoThietBi.KiemTraThietBi(null, "TB-01", Phong, [LoaiX]));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    [Theory]
    [InlineData("DANG_DUOC_MUON", false)]
    [InlineData("DANG_BAO_TRI_SUA_CHUA", false)]
    [InlineData("DA_BAO_MAT", false)]
    [InlineData("SAN_SANG", true)]
    public void BR40_KhongSanSangHoacNgungSuDung_Tra422(string tinhTrang, bool ngungSuDung)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraGiaoThietBi.KiemTraThietBi(ThietBi(tinhTrang, ngungSuDung), "TB-01", Phong, [LoaiX]));

        Assert.Equal("THIET_BI_KHONG_SAN_SANG", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Fact]
    public void BR40_SaiPhong_Tra422()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraGiaoThietBi.KiemTraThietBi(ThietBi(maPhong: Guid.NewGuid()), "TB-01", Phong, [LoaiX]));

        Assert.Equal("THIET_BI_SAI_PHONG", loi.MaLoi);
    }

    [Fact]
    public void BR40_LoaiKhongThuocNguon_Tra422()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraGiaoThietBi.KiemTraThietBi(ThietBi(maLoai: Guid.NewGuid()), "TB-01", Phong, [LoaiX, LoaiY]));

        Assert.Equal("THIET_BI_SAI_LOAI", loi.MaLoi);
    }

    [Fact]
    public void G2A_DuSoLuongTheoLoai_KhongLoi()
    {
        KiemTraGiaoThietBi.KiemTraKhopSoLuong(
            [LoaiX, LoaiY, LoaiX],
            [new LoaiVaSoLuongYeuCau(LoaiX, 2), new LoaiVaSoLuongYeuCau(LoaiY, 1)]);
    }

    [Fact]
    public void G2A_Thieu_Tra400()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => KiemTraGiaoThietBi.KiemTraKhopSoLuong(
            [LoaiX, LoaiY],
            [new LoaiVaSoLuongYeuCau(LoaiX, 2), new LoaiVaSoLuongYeuCau(LoaiY, 1)]));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void G2A_Thua_Tra400()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => KiemTraGiaoThietBi.KiemTraKhopSoLuong(
            [LoaiX, LoaiX, LoaiX, LoaiY],
            [new LoaiVaSoLuongYeuCau(LoaiX, 2), new LoaiVaSoLuongYeuCau(LoaiY, 1)]));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void G2A_ThieuHanMotLoai_Tra400()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => KiemTraGiaoThietBi.KiemTraKhopSoLuong(
            [LoaiX, LoaiX],
            [new LoaiVaSoLuongYeuCau(LoaiX, 2), new LoaiVaSoLuongYeuCau(LoaiY, 1)]));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }
}
