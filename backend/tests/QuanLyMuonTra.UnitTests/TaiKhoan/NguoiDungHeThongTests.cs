using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.UnitTests.TaiKhoan;

public sealed class NguoiDungHeThongTests
{
    private static QuanTriVien TaoQuanTri() =>
        QuanTriVien.Tao(Guid.CreateVersion7(), "qt-kiem-thu", "bam", "Quan tri kiem thu", "qt-kiem-thu");

    [Fact]
    public void UC19_NhapSaiDuNguong_TamKhoaDangNhap()
    {
        var quanTri = TaoQuanTri();

        quanTri.GhiNhanDangNhapSai(3);
        quanTri.GhiNhanDangNhapSai(3);
        Assert.True(quanTri.DangHoatDong);
        Assert.Equal(2, quanTri.SoLanDangNhapSai);

        quanTri.GhiNhanDangNhapSai(3);
        Assert.Equal(MaTrangThaiKhoa.TamKhoaDangNhap, quanTri.TrangThaiKhoa);
        Assert.False(quanTri.DangHoatDong);
    }

    [Fact]
    public void UC19_TaiKhoanDaTamKhoa_NhapSaiKhongTangThemBoDem()
    {
        var quanTri = TaoQuanTri();
        quanTri.GhiNhanDangNhapSai(1);

        quanTri.GhiNhanDangNhapSai(1);

        Assert.Equal(1, quanTri.SoLanDangNhapSai);
    }

    [Fact]
    public void UC19_DangNhapDung_DatLaiBoDem()
    {
        var quanTri = TaoQuanTri();
        quanTri.GhiNhanDangNhapSai(5);
        quanTri.GhiNhanDangNhapSai(5);

        quanTri.GhiNhanDangNhapDung();

        Assert.Equal(0, quanTri.SoLanDangNhapSai);
        Assert.True(quanTri.DangHoatDong);
    }

    [Fact]
    public void UC19_VaiTro_KhopMaTrongHopDong()
    {
        Assert.Equal("QUAN_TRI", TaoQuanTri().VaiTro);
    }
}

