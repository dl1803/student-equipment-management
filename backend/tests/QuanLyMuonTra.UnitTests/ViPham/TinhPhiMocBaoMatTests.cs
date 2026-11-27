using QuanLyMuonTra.Modules.ViPham.Domain;

namespace QuanLyMuonTra.UnitTests.ViPham;

public class TinhPhiMocBaoMatTests
{
    private static readonly TimeSpan GioVn = TimeSpan.FromHours(7);

    [Theory]
    [InlineData(0, 12, false)]
    [InlineData(29, 12, false)]
    [InlineData(30, 1, false)]
    [InlineData(30, 23, false)]
    [InlineData(31, 0, true)]
    [InlineData(31, 23, true)]
    [InlineData(60, 12, true)]
    public void UC09A3_MocBaoMat_TinhTheoNgayLichVnKeTuHanTra(int soNgayLichSauHan, int gio, bool mongDoi)
    {
        var hanTra = new DateTimeOffset(2026, 10, 1, 17, 0, 0, GioVn);
        var thoiDiem = new DateTimeOffset(2026, 10, 1, gio, 0, 0, GioVn).AddDays(soNgayLichSauHan);

        Assert.Equal(mongDoi, TinhPhi.QuaMocBaoMat(hanTra, thoiDiem));
    }

    [Fact]
    public void UC09A3_MocBaoMat_ThoiDiemUtcQuaNuaDemVn_TinhTheoNgayVn()
    {
        var hanTra = new DateTimeOffset(2026, 10, 1, 17, 0, 0, GioVn);
        var thoiDiemUtc = new DateTimeOffset(2026, 10, 31, 17, 30, 0, TimeSpan.Zero);

        Assert.True(TinhPhi.QuaMocBaoMat(hanTra, thoiDiemUtc));
    }
}

