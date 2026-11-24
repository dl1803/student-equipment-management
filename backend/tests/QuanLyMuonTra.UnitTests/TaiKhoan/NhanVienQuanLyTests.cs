using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.TaiKhoan;

public sealed class NhanVienQuanLyTests
{
    private static NhanVienQuanLy TaoNhanVien(string trangThaiKhoa = MaTrangThaiKhoa.HoatDong) =>
        NhanVienQuanLy.Tao(Guid.CreateVersion7(), "nv-kt", "bam-cu", "Nhan vien kiem thu", "NV-KT", null, trangThaiKhoa);

    [Theory]
    [InlineData(MaTrangThaiKhoa.HoatDong)]
    [InlineData(MaTrangThaiKhoa.TamKhoaDangNhap)]
    public void UC14_Khoa_TuTrangThaiChuaBiKhoa_ChuyenBiKhoa(string trangThaiBanDau)
    {
        var nhanVien = TaoNhanVien(trangThaiBanDau);

        nhanVien.Khoa();

        Assert.Equal(MaTrangThaiKhoa.BiKhoa, nhanVien.TrangThaiKhoa);
    }

    [Fact]
    public void UC14_Khoa_DaBiKhoa_NemTrangThaiKhongHopLe()
    {
        var nhanVien = TaoNhanVien(MaTrangThaiKhoa.BiKhoa);

        var loi = Assert.Throws<LoiNghiepVuException>(() => nhanVien.Khoa());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void UC14_MoKhoa_TuBiKhoa_VeHoatDong()
    {
        var nhanVien = TaoNhanVien(MaTrangThaiKhoa.BiKhoa);

        nhanVien.MoKhoa();

        Assert.Equal(MaTrangThaiKhoa.HoatDong, nhanVien.TrangThaiKhoa);
    }

    [Fact]
    public void UC14_MoKhoa_TuTamKhoaDangNhap_VeHoatDongVaDatLaiBoDem()
    {
        var nhanVien = TaoNhanVien();
        nhanVien.GhiNhanDangNhapSai(1);
        Assert.Equal(MaTrangThaiKhoa.TamKhoaDangNhap, nhanVien.TrangThaiKhoa);
        Assert.Equal(1, nhanVien.SoLanDangNhapSai);

        nhanVien.MoKhoa();

        Assert.Equal(MaTrangThaiKhoa.HoatDong, nhanVien.TrangThaiKhoa);
        Assert.Equal(0, nhanVien.SoLanDangNhapSai);
    }

    [Fact]
    public void UC14_MoKhoa_DangHoatDong_NemTrangThaiKhongHopLe()
    {
        var nhanVien = TaoNhanVien();

        var loi = Assert.Throws<LoiNghiepVuException>(() => nhanVien.MoKhoa());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void UC14_CapNhatThongTin_DoiHoTenVaEmail()
    {
        var nhanVien = TaoNhanVien();

        nhanVien.CapNhatThongTin("Ho Ten Moi", "moi@truong.local");

        Assert.Equal("Ho Ten Moi", nhanVien.HoTen);
        Assert.Equal("moi@truong.local", nhanVien.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UC14_CapNhatThongTin_HoTenRongHoacTrang_NemArgumentException(string? hoTenKhongHopLe)
    {
        var nhanVien = TaoNhanVien();

        Assert.ThrowsAny<ArgumentException>(() => nhanVien.CapNhatThongTin(hoTenKhongHopLe!, "email@test.com"));
    }

    [Theory]
    [InlineData(null, "hoten", "NV01")]
    [InlineData("", "hoten", "NV01")]
    [InlineData("   ", "hoten", "NV01")]
    [InlineData("tendangnhap", null, "NV01")]
    [InlineData("tendangnhap", "", "NV01")]
    [InlineData("tendangnhap", "   ", "NV01")]
    [InlineData("tendangnhap", "hoten", null)]
    [InlineData("tendangnhap", "hoten", "")]
    [InlineData("tendangnhap", "hoten", "   ")]
    public void UC14_Tao_ThongTinRongHoacTrang_NemArgumentException(string? tenDangNhap, string? hoTen, string? maNv)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            NhanVienQuanLy.Tao(Guid.CreateVersion7(), tenDangNhap!, "bam", hoTen!, maNv!));
    }

    [Fact]
    public void UC14_DatLaiMatKhau_ThayMaBam()
    {
        var nhanVien = TaoNhanVien();

        nhanVien.DatLaiMatKhau("bam-moi");

        Assert.Equal("bam-moi", nhanVien.MatKhauBam);
    }
}

