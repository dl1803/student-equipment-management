using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.BaoTri;

public sealed class ThanhLyUngDungTests
{
    private static readonly DateOnly Ngay = new(2026, 11, 23);

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task UC15_DsChoQuyetDinh_TrangHoacKichThuocNgoaiKhoang_Nem400TruocKhiTruyCapCsdl(int trang, int kichThuoc)
    {
        var boXuLy = new DsChoQuyetDinhThanhLyHandler(null!);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(trang, kichThuoc, CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public async Task UC15_LapQuyetDinh_DanhSachRong_Nem400TruocKhiTruyCapCsdl()
    {
        var loi = await ChayAsync([]);

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public async Task UC15_LapQuyetDinh_MaThietBiRongHoacToanKhoangTrang_Nem400()
    {
        var loi = await ChayAsync(["D16-TB-01", "   "]);

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public async Task UC15_LapQuyetDinh_MaLapSauKhiCatKhoangTrang_Nem400()
    {
        var loi = await ChayAsync(["D16-TB-01", " D16-TB-01 "]);

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    private static async Task<LoiNghiepVuException> ChayAsync(IReadOnlyList<string> dsMa)
    {
        var boXuLy = new LapQuyetDinhThanhLyHandler(null!, null!, null!, null!, null!, null!);

        return await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(
            Guid.NewGuid(),
            new LapQuyetDinhThanhLyRequest(Ngay, dsMa),
            CancellationToken.None));
    }
}
