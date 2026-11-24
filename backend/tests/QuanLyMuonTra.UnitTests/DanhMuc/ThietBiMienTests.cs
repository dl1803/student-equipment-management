using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class ThietBiMienTests
{
    private static readonly Guid MaLoai = Guid.Parse("019c0000-0000-7000-8000-000000000011");
    private static readonly Guid MaPhong = Guid.Parse("019c0000-0000-7000-8000-000000000001");

    private static ThietBi TaoThietBi() =>
        ThietBi.NhapKho("TB-01", MaLoai, MaPhong, "Laptop 01", null, null, new DateOnly(2026, 1, 5), null, null, null);

    [Fact]
    public void UC12_NhapKho_KhoiTaoSanSangVaChuaNgungSuDung()
    {
        var thietBi = ThietBi.NhapKho(
            "TB-01", MaLoai, MaPhong, "Laptop 01", "Mo ta", "i5", new DateOnly(2026, 1, 5), new DateOnly(2028, 1, 5), "Ke 1", "Tu 2");

        Assert.Equal("TB-01", thietBi.MaThietBi);
        Assert.Equal(TinhTrangThietBi.SanSang, thietBi.TinhTrang);
        Assert.False(thietBi.NgungSuDung);
        Assert.Equal(MaLoai, thietBi.MaLoai);
        Assert.Equal(MaPhong, thietBi.MaPhong);
        Assert.Equal("Mo ta", thietBi.MoTa);
        Assert.Equal("i5", thietBi.ThongSoKyThuat);
        Assert.Equal(new DateOnly(2028, 1, 5), thietBi.ThoiHanBaoHanh);
        Assert.Equal("Ke 1", thietBi.ViTriKe);
        Assert.Equal("Tu 2", thietBi.ViTriTu);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UC12_NhapKho_MaRong_Nem400(string? ma)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => ThietBi.NhapKho(ma!, MaLoai, MaPhong, "Laptop", null, null, new DateOnly(2026, 1, 5), null, null, null));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UC12_NhapKho_TenRong_Nem400(string? ten)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => ThietBi.NhapKho("TB-01", MaLoai, MaPhong, ten!, null, null, new DateOnly(2026, 1, 5), null, null, null));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public void UC12_CapNhatThongTin_DoiPhongLoaiGiuNguyenMaVaTinhTrang()
    {
        var thietBi = TaoThietBi();
        var loaiMoi = Guid.Parse("019c0000-0000-7000-8000-000000000012");
        var phongMoi = Guid.Parse("019c0000-0000-7000-8000-000000000002");

        thietBi.CapNhatThongTin(loaiMoi, phongMoi, "Ten moi", "Mo ta moi", "Thong so", new DateOnly(2026, 2, 1), null, "Ke 9", null);

        Assert.Equal("TB-01", thietBi.MaThietBi);
        Assert.Equal(TinhTrangThietBi.SanSang, thietBi.TinhTrang);
        Assert.Equal(loaiMoi, thietBi.MaLoai);
        Assert.Equal(phongMoi, thietBi.MaPhong);
        Assert.Equal("Ten moi", thietBi.TenThietBi);
        Assert.Equal("Mo ta moi", thietBi.MoTa);
        Assert.Equal(new DateOnly(2026, 2, 1), thietBi.NgayNhapKho);
        Assert.Null(thietBi.ThoiHanBaoHanh);
        Assert.Equal("Ke 9", thietBi.ViTriKe);
        Assert.Null(thietBi.ViTriTu);
    }

    [Fact]
    public void UC12_CapNhatThongTin_TenRong_Nem400VaKhongDoiDuLieu()
    {
        var thietBi = TaoThietBi();

        var loi = Assert.Throws<LoiNghiepVuException>(
            () => thietBi.CapNhatThongTin(MaLoai, MaPhong, "  ", null, null, new DateOnly(2026, 1, 5), null, null, null));

        Assert.Equal(400, loi.MaHttp);
        Assert.Equal("Laptop 01", thietBi.TenThietBi);
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(100_000_000_000_000L)]
    public void UC12_LichSuGiaTri_NgoaiKhoang_Nem400(long giaTri)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => new LichSuGiaTri("TB-01", giaTri, DateTimeOffset.UnixEpoch));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(99_999_999_999_999L)]
    public void UC12_LichSuGiaTri_BienHopLe_Dat(long giaTri)
    {
        var ngay = new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero);

        var lichSu = new LichSuGiaTri("TB-01", giaTri, ngay);

        Assert.NotEqual(Guid.Empty, lichSu.MaLichSu);
        Assert.Equal("TB-01", lichSu.MaThietBi);
        Assert.Equal(giaTri, lichSu.GiaTri);
        Assert.Equal(ngay, lichSu.NgayCapNhat);
    }

    [Theory]
    [InlineData(typeof(ThietBi))]
    [InlineData(typeof(LichSuGiaTri))]
    [InlineData(typeof(ThietBiNhaCungCap))]
    public void UC12_CacThucTheThietBi_LaIAuditable(Type kieu)
    {
        Assert.True(typeof(IAuditable).IsAssignableFrom(kieu));
    }
}
