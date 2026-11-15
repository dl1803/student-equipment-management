using QuanLyMuonTra.Modules.QuyDinh.Contracts;

namespace QuanLyMuonTra.UnitTests.QuyDinh;

public sealed class QuyDinhNhomDtoTests
{
    private static readonly Guid Laptop = Guid.Parse("019c0000-0000-7000-8000-000000000011");
    private static readonly Guid MayChieu = Guid.Parse("019c0000-0000-7000-8000-000000000012");
    private static readonly Guid ManHinh = Guid.Parse("019c0000-0000-7000-8000-000000000013");

    private static QuyDinhNhomDto DaiHoc(params NgoaiLeTheoLoaiDto[] ngoaiLe)
    {
        return new QuyDinhNhomDto("DAI_HOC", 2, 5, ngoaiLe);
    }

    private static QuyDinhNhomDto CaoHoc(params NgoaiLeTheoLoaiDto[] ngoaiLe)
    {
        return new QuyDinhNhomDto("CAO_HOC", 3, 10, ngoaiLe);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    public void BR24_DaiHocToiDa2_TongDangCamCongMuonThemKhongVuot_KhongVuotGioiHan(int dangCam, int muonThem)
    {
        Assert.False(DaiHoc().VuotGioiHanSoThietBi(dangCam, muonThem));
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    public void BR24_DaiHocToiDa2_TongDangCamCongMuonThemVuot_VuotGioiHan(int dangCam, int muonThem)
    {
        Assert.True(DaiHoc().VuotGioiHanSoThietBi(dangCam, muonThem));
    }

    [Fact]
    public void BR24_CaoHocToiDa3_DangCam2CongMuonThem1_KhongVuotNhungThem2ThiVuot()
    {
        Assert.False(CaoHoc().VuotGioiHanSoThietBi(2, 1));
        Assert.True(CaoHoc().VuotGioiHanSoThietBi(2, 2));
    }

    [Fact]
    public void BR25_KhongCoNgoaiLe_LayMucChungCuaNhom()
    {
        Assert.Equal(5, DaiHoc().ThoiGianToiDaGio([Laptop]));
        Assert.Equal(10, CaoHoc().ThoiGianToiDaGio([Laptop, MayChieu]));
    }

    [Fact]
    public void BR25_LoaiCoNgoaiLeNhoHonMucChung_LayNgoaiLe()
    {
        var quyDinh = DaiHoc(new NgoaiLeTheoLoaiDto(MayChieu, 3));

        Assert.Equal(3, quyDinh.ThoiGianToiDaGio([MayChieu]));
    }

    [Fact]
    public void BR25_PhieuNhieuLoai_LayMucNhoNhatTheoCacLoaiTrongPhieu()
    {
        var quyDinh = DaiHoc(new NgoaiLeTheoLoaiDto(MayChieu, 3));

        Assert.Equal(3, quyDinh.ThoiGianToiDaGio([Laptop, MayChieu]));
        Assert.Equal(5, quyDinh.ThoiGianToiDaGio([Laptop, ManHinh]));
    }

    [Fact]
    public void BR25_NgoaiLeCuaLoaiKhongCoTrongPhieu_KhongAnhHuong()
    {
        var quyDinh = DaiHoc(new NgoaiLeTheoLoaiDto(MayChieu, 3));

        Assert.Equal(5, quyDinh.ThoiGianToiDaGio([Laptop]));
    }

    [Fact]
    public void BR25_NhomUuTien10Gio_NgoaiLe3Gio_ChiLoaiNgoaiLeBiGiam()
    {
        var quyDinh = CaoHoc(new NgoaiLeTheoLoaiDto(MayChieu, 3));

        Assert.Equal(10, quyDinh.ThoiGianToiDaGio([Laptop]));
        Assert.Equal(3, quyDinh.ThoiGianToiDaGio([MayChieu]));
        Assert.Equal(3, quyDinh.ThoiGianToiDaGio([Laptop, MayChieu]));
    }

    [Fact]
    public void BR25_NgoaiLeLonHonMucChung_BiChanBoiMucToiDaCuaNhom()
    {
        var quyDinh = DaiHoc(new NgoaiLeTheoLoaiDto(Laptop, 8));

        Assert.Equal(5, quyDinh.ThoiGianToiDaGio([Laptop]));
        Assert.Equal(5, quyDinh.ThoiGianToiDaGio([Laptop, MayChieu]));
    }

    [Fact]
    public void BR25_DanhSachLoaiNull_NemArgumentNullException()
    {
        var quyDinh = DaiHoc();

        Assert.Throws<ArgumentNullException>(() => quyDinh.ThoiGianToiDaGio(null!));
    }

    [Fact]
    public void BR25_DanhSachLoaiRong_LayMucChungCuaNhom()
    {
        var quyDinh = DaiHoc(new NgoaiLeTheoLoaiDto(MayChieu, 3));

        Assert.Equal(5, quyDinh.ThoiGianToiDaGio([]));
    }

    [Fact]
    public void BR25_LoaiLapLaiTrongPhieu_KhongLamLechKetQua()
    {
        var quyDinh = DaiHoc(new NgoaiLeTheoLoaiDto(MayChieu, 3));

        Assert.Equal(3, quyDinh.ThoiGianToiDaGio([MayChieu, MayChieu, Laptop]));
    }
}
