using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class KiemTraDieuKienGuiYeuCauTests
{
    private static readonly DateOnly HomNay = new(2026, 11, 18);
    private static readonly Guid LoaiThuong = Guid.Parse("019c0000-0000-7000-8000-0000000d6c01");
    private static readonly Guid LoaiNgoaiLe = Guid.Parse("019c0000-0000-7000-8000-0000000d6c02");
    private static readonly QuyDinhNhomDto QuyDinh = new("D06-NHOM", 2, 5, [new NgoaiLeTheoLoaiDto(LoaiNgoaiLe, 2)]);

    private static DongHoSoSinhVien HoSo(DateOnly hanThe, string trangThaiHoc = "DANG_HOC", string trangThaiQuyenMuon = "BINH_THUONG")
    {
        return new DongHoSoSinhVien
        {
            MaNhom = "D06-NHOM",
            HanThe = hanThe,
            TrangThaiHoc = trangThaiHoc,
            TrangThaiQuyenMuon = trangThaiQuyenMuon
        };
    }

    [Fact]
    public void UC02_HanTheBangHomNay_VanHopLe()
    {
        KiemTraDieuKienGuiYeuCau.KiemTraSinhVien(HoSo(HomNay), HomNay);
    }

    [Fact]
    public void UC02E1_HanTheTruocHomNay_Tra422SvTheHetHan()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraDieuKienGuiYeuCau.KiemTraSinhVien(HoSo(HomNay.AddDays(-1)), HomNay));

        Assert.Equal(MaLoi.SvTheHetHan, loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Theory]
    [InlineData("TOT_NGHIEP")]
    [InlineData("THOI_HOC")]
    [InlineData("DINH_CHI")]
    public void UC02E1_KhongConTheoHoc_Tra422SvKhongConTheoHoc(string trangThaiHoc)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraDieuKienGuiYeuCau.KiemTraSinhVien(HoSo(HomNay, trangThaiHoc), HomNay));

        Assert.Equal(MaLoi.SvKhongConTheoHoc, loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Theory]
    [InlineData("TAM_KHOA")]
    [InlineData("CAM_MUON_DAI_HAN")]
    public void UC02E1_QuyenMuonBiKhoa_Tra422SvBiKhoaQuyenMuon(string trangThaiQuyenMuon)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraDieuKienGuiYeuCau.KiemTraSinhVien(HoSo(HomNay, trangThaiQuyenMuon: trangThaiQuyenMuon), HomNay));

        Assert.Equal(MaLoi.SvBiKhoaQuyenMuon, loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Fact]
    public void UC02E1_TheHetHanVaKhongConTheoHoc_BaoTheHetHanTruoc()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraDieuKienGuiYeuCau.KiemTraSinhVien(HoSo(HomNay.AddDays(-1), "TOT_NGHIEP", "TAM_KHOA"), HomNay));

        Assert.Equal(MaLoi.SvTheHetHan, loi.MaLoi);
    }

    [Fact]
    public void UC02A3_ThoiGianBangMucToiDaNhom_HopLe()
    {
        KiemTraDieuKienGuiYeuCau.KiemTraThoiGianMuon(QuyDinh, [LoaiThuong], 5);
    }

    [Fact]
    public void UC02A3_ThoiGianVuotMucToiDaNhom_Tra422KemMucToiDa()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraDieuKienGuiYeuCau.KiemTraThoiGianMuon(QuyDinh, [LoaiThuong], 6));

        Assert.Equal(MaLoi.VuotThoiGianMuonToiDa, loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
        Assert.Contains("5 giờ", loi.Message);
    }

    [Fact]
    public void UC02A3_LoaiCoNgoaiLeNhoHon_ApDungNgoaiLe()
    {
        KiemTraDieuKienGuiYeuCau.KiemTraThoiGianMuon(QuyDinh, [LoaiNgoaiLe], 2);

        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraDieuKienGuiYeuCau.KiemTraThoiGianMuon(QuyDinh, [LoaiNgoaiLe], 3));

        Assert.Equal(MaLoi.VuotThoiGianMuonToiDa, loi.MaLoi);
        Assert.Contains("2 giờ", loi.Message);
    }

    [Fact]
    public void UC02A3_PhieuNhieuLoai_LayMucNhoNhatTheoCacLoai()
    {
        KiemTraDieuKienGuiYeuCau.KiemTraThoiGianMuon(QuyDinh, [LoaiThuong, LoaiNgoaiLe], 2);

        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraDieuKienGuiYeuCau.KiemTraThoiGianMuon(QuyDinh, [LoaiThuong, LoaiNgoaiLe], 3));

        Assert.Equal(MaLoi.VuotThoiGianMuonToiDa, loi.MaLoi);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    public void UC02A2_DangCamCongYeuCauKhongVuotGioiHan_HopLe(int soDangCam, int soMuonThem)
    {
        KiemTraDieuKienGuiYeuCau.KiemTraGioiHanThietBi(QuyDinh, soDangCam, soMuonThem);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(0, 3)]
    [InlineData(2, 1)]
    public void UC02A2_DangCamCongYeuCauVuotGioiHan_Tra422NeuRoDieuKien(int soDangCam, int soMuonThem)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraDieuKienGuiYeuCau.KiemTraGioiHanThietBi(QuyDinh, soDangCam, soMuonThem));

        Assert.Equal(MaLoi.VuotGioiHanThietBi, loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
        Assert.Contains("giới hạn 2 thiết bị", loi.Message);
    }

    [Theory]
    [InlineData(1, (long)int.MaxValue)]
    [InlineData(0, (long)int.MaxValue + 1)]
    public void UC02A2_SoLuongLon_KhongBiTranSo_Tra422(int soDangCam, long soMuonThem)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => KiemTraDieuKienGuiYeuCau.KiemTraGioiHanThietBi(QuyDinh, soDangCam, soMuonThem));

        Assert.Equal(MaLoi.VuotGioiHanThietBi, loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }
}
