using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class KiemTraDocPhieuMuonTests
{
    [Theory]
    [InlineData("PM-261023-000123")]
    [InlineData("PM-261023-1234567")]
    public void KiemTraMaPhieu_DungDinhDang_KhongNemLoi(string maPhieu)
    {
        KiemTraDocPhieuMuon.KiemTraMaPhieu(maPhieu);
    }

    [Theory]
    [InlineData("")]
    [InlineData("pm-261023-000123")]
    [InlineData("PM-26102-000123")]
    [InlineData("PM-261023-12345")]
    [InlineData("YC-261023-000123")]
    [InlineData("qua-han-30-ngay")]
    [InlineData("PM-261023-000123\n")]
    public void KiemTraMaPhieu_SaiDinhDang_Nem400(string maPhieu)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => KiemTraDocPhieuMuon.KiemTraMaPhieu(maPhieu));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("DANG_MUON")]
    [InlineData("QUA_HAN")]
    [InlineData("DA_TRA")]
    public void KiemTraTrangThai_HopLeHoacKhongLoc_KhongNemLoi(string? trangThai)
    {
        KiemTraDocPhieuMuon.KiemTraTrangThai(trangThai);
    }

    [Theory]
    [InlineData("")]
    [InlineData("dang_muon")]
    [InlineData("CHO_DUYET")]
    public void KiemTraTrangThai_KhongThuocTapChoPhep_Nem400(string trangThai)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => KiemTraDocPhieuMuon.KiemTraTrangThai(trangThai));

        Assert.Equal(400, loi.MaHttp);
    }
}
