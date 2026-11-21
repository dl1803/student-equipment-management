using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class AnhThietBiUngDungTests
{
    private const string MaThietBi = "TB-01";
    private static readonly Guid A = Guid.Parse("019c0000-0000-7000-8000-00000000a001");
    private static readonly Guid B = Guid.Parse("019c0000-0000-7000-8000-00000000a002");
    private static readonly Guid C = Guid.Parse("019c0000-0000-7000-8000-00000000a003");

    [Fact]
    public void UC12_TinhThayDoi_GiuNguyenDanhSach_KhongGoKhongGan()
    {
        var ketQua = AnhThietBi.TinhThayDoi([A, B], [B, A]);

        Assert.Empty(ketQua.CanGo);
        Assert.Empty(ketQua.CanGan);
    }

    [Fact]
    public void UC12_TinhThayDoi_ThayMotAnh_GoAnhCuVaGanAnhMoi()
    {
        var ketQua = AnhThietBi.TinhThayDoi([A, B], [B, C]);

        Assert.Equal(new[] { A }, ketQua.CanGo);
        Assert.Equal(new[] { C }, ketQua.CanGan);
    }

    [Fact]
    public void UC12_TinhThayDoi_DanhSachMoiRong_GoTatCa()
    {
        var ketQua = AnhThietBi.TinhThayDoi([A, B], []);

        Assert.Equal(new[] { A, B }, ketQua.CanGo);
        Assert.Empty(ketQua.CanGan);
    }

    [Fact]
    public async Task UC12_GanMoi_KhongCoAnh_KhongGoiDichVuAnh()
    {
        var anh = new AnhServiceGia();

        await AnhThietBi.GanMoiAsync(anh, MaThietBi, null, CancellationToken.None);
        await AnhThietBi.GanMoiAsync(anh, MaThietBi, [], CancellationToken.None);

        Assert.Empty(anh.LichSuGoi);
    }

    [Fact]
    public async Task UC12_GanMoi_CoAnh_GanVaoThietBiVoiGiaiDoanThietBi()
    {
        var anh = new AnhServiceGia();

        await AnhThietBi.GanMoiAsync(anh, MaThietBi, [A, B], CancellationToken.None);

        var lanGan = Assert.Single(anh.CacLanGan);
        Assert.Equal(MaDoiTuongAnh.ThietBi, lanGan.DoiTuongLoai);
        Assert.Equal(MaThietBi, lanGan.DoiTuongId);
        Assert.Equal(MaGiaiDoanAnh.ThietBi, lanGan.GiaiDoan);
        Assert.Equal(new[] { A, B }, lanGan.DsMaAnh);
    }

    [Fact]
    public async Task UC12_Thay_DanhSachNull_GiuNguyenAnhHienCo()
    {
        var anh = new AnhServiceGia();
        anh.DsAnhHienCo.AddRange([A, B]);

        await AnhThietBi.ThayAsync(anh, MaThietBi, null, CancellationToken.None);

        Assert.Empty(anh.LichSuGoi);
    }

    [Fact]
    public async Task UC12_Thay_DanhSachRong_GoHetAnhHienCoKhongGan()
    {
        var anh = new AnhServiceGia();
        anh.DsAnhHienCo.AddRange([A, B]);

        await AnhThietBi.ThayAsync(anh, MaThietBi, [], CancellationToken.None);

        var lanGo = Assert.Single(anh.CacLanGo);
        Assert.Equal(MaThietBi, lanGo.MaThietBi);
        Assert.Equal(new[] { A, B }, lanGo.DsMaAnh);
        Assert.Empty(anh.CacLanGan);
    }

    [Fact]
    public async Task UC12_Thay_ThayMotAnh_GoTruocRoiMoiGanChiAnhMoi()
    {
        var anh = new AnhServiceGia();
        anh.DsAnhHienCo.AddRange([A, B]);

        await AnhThietBi.ThayAsync(anh, MaThietBi, [B, C], CancellationToken.None);

        Assert.Equal(new[] { "GO", "GAN" }, anh.LichSuGoi);
        Assert.Equal(new[] { A }, Assert.Single(anh.CacLanGo).DsMaAnh);
        Assert.Equal(new[] { C }, Assert.Single(anh.CacLanGan).DsMaAnh);
    }

    [Fact]
    public async Task UC12_GoTatCa_ThietBiCoAnh_GoMoiAnhDanhMuc()
    {
        var anh = new AnhServiceGia();
        anh.DsAnhHienCo.AddRange([A, B, C]);

        await AnhThietBi.GoTatCaAsync(anh, MaThietBi, CancellationToken.None);

        Assert.Equal(new[] { A, B, C }, Assert.Single(anh.CacLanGo).DsMaAnh);
    }

    [Fact]
    public async Task UC12_GoTatCa_ThietBiKhongCoAnh_KhongGoiDichVuAnh()
    {
        var anh = new AnhServiceGia();

        await AnhThietBi.GoTatCaAsync(anh, MaThietBi, CancellationToken.None);

        Assert.Empty(anh.LichSuGoi);
    }
}
