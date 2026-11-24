using QuanLyMuonTra.Infrastructure.LuuTru;

namespace QuanLyMuonTra.IntegrationTests.Anh;

public sealed class DinhDangAnhTests
{
    [Fact]
    public void NhanDang_ChuKyJpeg_TraImageJpeg() =>
        Assert.Equal("image/jpeg", DinhDangAnh.NhanDang([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10]));

    [Fact]
    public void NhanDang_ChuKyPng_TraImagePng() =>
        Assert.Equal("image/png", DinhDangAnh.NhanDang([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00]));

    [Fact]
    public void NhanDang_KhungRiffWebp_TraImageWebp() =>
        Assert.Equal(
            "image/webp",
            DinhDangAnh.NhanDang([0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50]));

    [Fact]
    public void NhanDang_KhungRiffKhongPhaiWebp_TraNull() =>
        Assert.Null(DinhDangAnh.NhanDang([0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45]));

    [Fact]
    public void NhanDang_VanBanGiaAnh_TraNull() =>
        Assert.Null(DinhDangAnh.NhanDang("Day khong phai anh"u8));

    [Fact]
    public void NhanDang_Gif_TraNull() =>
        Assert.Null(DinhDangAnh.NhanDang("GIF89a"u8));

    [Fact]
    public void NhanDang_PngBiCat_TraNull() =>
        Assert.Null(DinhDangAnh.NhanDang([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A]));

    [Fact]
    public void NhanDang_RongHoacQuaNgan_TraNull()
    {
        Assert.Null(DinhDangAnh.NhanDang([]));
        Assert.Null(DinhDangAnh.NhanDang([0xFF, 0xD8]));
    }
}

