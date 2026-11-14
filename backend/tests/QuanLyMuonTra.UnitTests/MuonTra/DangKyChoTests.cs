using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class DangKyChoTests
{
    private static readonly DateTimeOffset HanGiuCho = new(2026, 11, 16, 2, 30, 0, TimeSpan.Zero);

    private static DangKyCho TaoDangCho()
    {
        return new DangKyCho(
            "DK-261116-000001",
            "SV001",
            Guid.Parse("019c0000-0000-7000-8000-000000000001"),
            Guid.Parse("019c0000-0000-7000-8000-000000000011"),
            new DateTimeOffset(2026, 11, 16, 1, 0, 0, TimeSpan.Zero),
            1);
    }

    private static DangKyCho TaoDaThongBao()
    {
        var dangKy = TaoDangCho();
        dangKy.ThongBaoDenLuot(HanGiuCho);
        return dangKy;
    }

    [Fact]
    public void TT05_ThongBaoDenLuot_DangChoSangDaThongBaoVaCoHanGiuCho()
    {
        var dangKy = TaoDangCho();

        dangKy.ThongBaoDenLuot(HanGiuCho);

        Assert.Equal(TrangThaiDangKyCho.DaThongBao, dangKy.TrangThai);
        Assert.Equal(HanGiuCho, dangKy.HanGiuCho);
    }

    [Fact]
    public void TT05_ThongBaoDenLuot_LanHai_NemTrangThaiKhongHopLe()
    {
        var dangKy = TaoDaThongBao();

        var loi = Assert.Throws<NghiepVuException>(() => dangKy.ThongBaoDenLuot(HanGiuCho));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void TT06_Roi_TuDaThongBao_SangDaHuy()
    {
        var dangKy = TaoDaThongBao();

        dangKy.Roi();

        Assert.Equal(TrangThaiDangKyCho.DaHuy, dangKy.TrangThai);
    }

    [Fact]
    public void TT06_Roi_TuDangCho_SangDaHuy()
    {
        var dangKy = TaoDangCho();

        dangKy.Roi();

        Assert.Equal(TrangThaiDangKyCho.DaHuy, dangKy.TrangThai);
    }

    [Fact]
    public void TT06_Roi_TuDaHuy_NemTrangThaiKhongHopLe()
    {
        var dangKy = TaoDangCho();
        dangKy.Roi();

        var loi = Assert.Throws<NghiepVuException>(() => dangKy.Roi());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void TT07_HetHan_TuDaThongBao_SangHetHan()
    {
        var dangKy = TaoDaThongBao();

        dangKy.HetHan();

        Assert.Equal(TrangThaiDangKyCho.HetHan, dangKy.TrangThai);
    }

    [Fact]
    public void TT07_HetHan_TuDangCho_NemTrangThaiKhongHopLe()
    {
        var dangKy = TaoDangCho();

        var loi = Assert.Throws<NghiepVuException>(() => dangKy.HetHan());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(TrangThaiDangKyCho.DangCho, dangKy.TrangThai);
    }

    [Fact]
    public void TT05_DanhDauDaNhan_TuDaThongBao_SangDaNhan()
    {
        var dangKy = TaoDaThongBao();

        dangKy.DanhDauDaNhan();

        Assert.Equal(TrangThaiDangKyCho.DaNhan, dangKy.TrangThai);
    }

    [Fact]
    public void TT05_DanhDauDaNhan_TuDangCho_NemTrangThaiKhongHopLe()
    {
        var dangKy = TaoDangCho();

        var loi = Assert.Throws<NghiepVuException>(() => dangKy.DanhDauDaNhan());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }
}
