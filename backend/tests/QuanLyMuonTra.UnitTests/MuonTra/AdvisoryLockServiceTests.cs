using QuanLyMuonTra.Modules.MuonTra.Infrastructure;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class AdvisoryLockServiceTests
{
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d3b01");
    private static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d3b02");
    private static readonly Guid X = Guid.Parse("019c0000-0000-7000-8000-0000000d3c01");
    private static readonly Guid Y = Guid.Parse("019c0000-0000-7000-8000-0000000d3c02");

    [Fact]
    public void NFR06_SuyKhoa_CungCapPhongLoai_RaDungKhoaDaGhimCoDinh()
    {
        Assert.Equal(-6798148238500429604L, AdvisoryLockService.SuyKhoa(P1, X));
        Assert.Equal(AdvisoryLockService.SuyKhoa(P1, X), AdvisoryLockService.SuyKhoa(P1, X));
    }

    [Fact]
    public void NFR06_SuyKhoa_DaoThuTuPhongVaLoai_RaKhoaKhac()
    {
        Assert.Equal(7649266431509112273L, AdvisoryLockService.SuyKhoa(X, P1));
        Assert.NotEqual(AdvisoryLockService.SuyKhoa(P1, X), AdvisoryLockService.SuyKhoa(X, P1));
    }

    [Fact]
    public void NFR06_SuyKhoa_BonCapKhacNhau_RaBonKhoaKhacNhau()
    {
        var cacKhoa = new[]
        {
            AdvisoryLockService.SuyKhoa(P1, X),
            AdvisoryLockService.SuyKhoa(P1, Y),
            AdvisoryLockService.SuyKhoa(P2, X),
            AdvisoryLockService.SuyKhoa(P2, Y)
        };

        Assert.Equal(4, cacKhoa.Distinct().Count());
    }

    [Fact]
    public void NFR06_SapXepCacCap_DanhSachXaoTronCoTrung_TangDanTheoPhongRoiLoaiVaBoTrung()
    {
        var ketQua = AdvisoryLockService.SapXepCacCap(new[] { (P2, Y), (P1, Y), (P2, X), (P1, X), (P1, Y) });

        Assert.Equal(new[] { (P1, X), (P1, Y), (P2, X), (P2, Y) }, ketQua.ToArray());
    }

    [Fact]
    public void NFR06_SapXepCacCap_DanhSachRong_RaDanhSachRong()
    {
        Assert.Empty(AdvisoryLockService.SapXepCacCap(Array.Empty<(Guid MaPhong, Guid MaLoai)>()));
    }
}
