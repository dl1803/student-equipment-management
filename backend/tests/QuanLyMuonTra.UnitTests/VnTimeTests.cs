using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests;

public class VnTimeTests
{
    [Fact]
    public void NFR09_MotGiayTruocNuaDemVn_VanLaNgayCu()
    {
        var thoiDiem = new DateTimeOffset(2026, 11, 12, 16, 59, 59, TimeSpan.Zero);

        Assert.Equal(new DateOnly(2026, 11, 12), VnTime.NgayLich(thoiDiem));
    }

    [Fact]
    public void NFR09_DungNuaDemVn_SangNgayMoi()
    {
        var thoiDiem = new DateTimeOffset(2026, 11, 12, 17, 0, 0, TimeSpan.Zero);

        Assert.Equal(new DateOnly(2026, 11, 13), VnTime.NgayLich(thoiDiem));
    }

    [Fact]
    public void NFR09_CungThoiDiemODoLechKhac_RaCungNgayLich()
    {
        var utc = new DateTimeOffset(2026, 11, 12, 17, 0, 0, TimeSpan.Zero);
        var gioKhac = utc.ToOffset(TimeSpan.FromHours(-5));

        Assert.Equal(VnTime.NgayLich(utc), VnTime.NgayLich(gioKhac));
    }
}

