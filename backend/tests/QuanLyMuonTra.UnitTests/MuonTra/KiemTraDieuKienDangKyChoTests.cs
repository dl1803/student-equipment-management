using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class KiemTraDieuKienDangKyChoTests
{
    [Fact]
    public void UC03_QuyenMuonBinhThuong_KhongNemLoi()
    {
        KiemTraDieuKienDangKyCho.KiemTraQuyenMuon("BINH_THUONG");
    }

    [Theory]
    [InlineData("TAM_KHOA")]
    [InlineData("CAM_MUON_DAI_HAN")]
    public void UC03_QuyenMuonBiKhoa_Tra422SvBiKhoaQuyenMuon(string trangThai)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => KiemTraDieuKienDangKyCho.KiemTraQuyenMuon(trangThai));

        Assert.Equal("SV_BI_KHOA_QUYEN_MUON", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Fact]
    public void UC03_KhaDungBangKhong_KhongNemLoi()
    {
        KiemTraDieuKienDangKyCho.KiemTraConHang(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void UC03E1_ConHang_Tra422ConHangKhongDangKyCho(int soKhaDung)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => KiemTraDieuKienDangKyCho.KiemTraConHang(soKhaDung));

        Assert.Equal("CON_HANG_KHONG_DANG_KY_CHO", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Theory]
    [InlineData("DK-261124-000001")]
    [InlineData("DK-261124-1000000")]
    public void UC03A2_MaDungDinhDang_KhongNemLoi(string ma)
    {
        KiemTraDieuKienDangKyCho.KiemTraMaDangKy(ma);
    }

    [Theory]
    [InlineData("")]
    [InlineData("YC-261124-000001")]
    [InlineData("DK-26112-000001")]
    [InlineData("DK-261124-00001")]
    [InlineData("dk-261124-000001")]
    [InlineData("DK-261124-000001\n")]
    public void UC03A2_MaSaiDinhDang_Tra400(string ma)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => KiemTraDieuKienDangKyCho.KiemTraMaDangKy(ma));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }
}
