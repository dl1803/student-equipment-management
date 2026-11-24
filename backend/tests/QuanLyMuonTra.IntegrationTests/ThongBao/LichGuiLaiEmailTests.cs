using QuanLyMuonTra.Modules.ThongBao.Domain;

namespace QuanLyMuonTra.IntegrationTests.ThongBao;

public sealed class LichGuiLaiEmailTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 5)]
    [InlineData(3, 15)]
    [InlineData(4, 60)]
    public void UC17_GianCachSauLanThu_DungBonKhoang(int soLanThu, int soPhutMongDoi)
    {
        Assert.Equal(TimeSpan.FromMinutes(soPhutMongDoi), LichGuiLaiEmail.GianCachSauLanThu(soLanThu));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(6, true)]
    public void UC17_LoiVinhVien_TuLanThuThuNam(int soLanThu, bool mongDoi)
    {
        Assert.Equal(mongDoi, LichGuiLaiEmail.LoiVinhVien(soLanThu));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(5)]
    public void UC17_GianCachSauLanThu_NgoaiKhoang_NemLoi(int soLanThu)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LichGuiLaiEmail.GianCachSauLanThu(soLanThu));
    }
}

