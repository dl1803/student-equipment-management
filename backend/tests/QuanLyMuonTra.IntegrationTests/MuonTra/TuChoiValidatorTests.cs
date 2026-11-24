using QuanLyMuonTra.Api.MuonTra;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class TuChoiValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UC06_LyDoRongHoacChiKhoangTrang_KhongHopLe(string? lyDo)
    {
        var ketQua = new TuChoiValidator().Validate(new TuChoiRequest(lyDo!));

        Assert.False(ketQua.IsValid);
        Assert.Contains(ketQua.Errors, e => e.PropertyName == "LyDo");
    }

    [Fact]
    public void UC06_LyDoCoNoiDung_HopLe()
    {
        var ketQua = new TuChoiValidator().Validate(new TuChoiRequest("Không đủ điều kiện"));

        Assert.True(ketQua.IsValid);
    }
}
