using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class KiemTraDieuKienGiaoTests
{
    private static readonly DateTimeOffset BayGio = new(2026, 11, 20, 3, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Ngay = new(2026, 11, 20);
    private static readonly QuyDinhNhomDto QuyDinh = new("DAI_HOC", 3, 5, []);

    private static DongHoSoSinhVien HoSo(
        DateOnly? hanThe = null,
        string trangThaiHoc = "DANG_HOC",
        string trangThaiQuyenMuon = "BINH_THUONG")
    {
        return new DongHoSoSinhVien
        {
            MaNhom = "DAI_HOC",
            HanThe = hanThe ?? new DateOnly(2030, 12, 31),
            TrangThaiHoc = trangThaiHoc,
            TrangThaiQuyenMuon = trangThaiQuyenMuon
        };
    }

    [Fact]
    public void GiuCho_ConHan_KhongCoLyDo()
    {
        var lyDo = KiemTraDieuKienGiao.KiemTraGiuCho("YC-1", true, BayGio.AddSeconds(1), BayGio);

        Assert.Null(lyDo);
    }

    [Fact]
    public void GiuCho_DungThoiDiemHetHan_LaHetHan()
    {
        var lyDo = KiemTraDieuKienGiao.KiemTraGiuCho("YC-1", true, BayGio, BayGio);

        Assert.NotNull(lyDo);
        Assert.Equal("GIU_CHO_HET_HAN", lyDo.MaLoi);
    }

    [Fact]
    public void GiuCho_QuaHan1Giay_LaHetHan()
    {
        var lyDo = KiemTraDieuKienGiao.KiemTraGiuCho("YC-1", true, BayGio.AddSeconds(-1), BayGio);

        Assert.NotNull(lyDo);
        Assert.Equal("GIU_CHO_HET_HAN", lyDo.MaLoi);
    }

    [Fact]
    public void GiuCho_KhongCoHan_LaHetHan()
    {
        var lyDo = KiemTraDieuKienGiao.KiemTraGiuCho("YC-1", true, null, BayGio);

        Assert.NotNull(lyDo);
        Assert.Equal("GIU_CHO_HET_HAN", lyDo.MaLoi);
    }

    [Fact]
    public void GiuCho_SaiTrangThai_UuTienTrangThaiKhongHopLe()
    {
        var lyDo = KiemTraDieuKienGiao.KiemTraGiuCho("YC-1", false, BayGio.AddHours(1), BayGio);

        Assert.NotNull(lyDo);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", lyDo.MaLoi);
    }

    [Fact]
    public void SinhVien_HopLe_KhongCoLyDo()
    {
        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(HoSo(), Ngay, QuyDinh, 0, 2);

        Assert.Empty(dsLyDo);
    }

    [Fact]
    public void SinhVien_TheHetHanHomQua_CoLyDo()
    {
        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(HoSo(hanThe: Ngay.AddDays(-1)), Ngay, QuyDinh, 0, 1);

        Assert.Equal("SV_THE_HET_HAN", Assert.Single(dsLyDo).MaLoi);
    }

    [Fact]
    public void SinhVien_TheHetHanDungHomNay_VanHopLe()
    {
        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(HoSo(hanThe: Ngay), Ngay, QuyDinh, 0, 1);

        Assert.Empty(dsLyDo);
    }

    [Fact]
    public void SinhVien_KhongConTheoHoc_CoLyDo()
    {
        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(HoSo(trangThaiHoc: "THOI_HOC"), Ngay, QuyDinh, 0, 1);

        Assert.Equal("SV_KHONG_CON_THEO_HOC", Assert.Single(dsLyDo).MaLoi);
    }

    [Fact]
    public void SinhVien_BiKhoaQuyenMuon_CoLyDo()
    {
        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(HoSo(trangThaiQuyenMuon: "TAM_KHOA"), Ngay, QuyDinh, 0, 1);

        Assert.Equal("SV_BI_KHOA_QUYEN_MUON", Assert.Single(dsLyDo).MaLoi);
    }

    [Fact]
    public void GioiHan_DangCamCongGiaoMoiVuot_CoLyDo()
    {
        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(HoSo(), Ngay, QuyDinh, 2, 2);

        Assert.Equal("VUOT_GIOI_HAN_THIET_BI", Assert.Single(dsLyDo).MaLoi);
    }

    [Fact]
    public void GioiHan_DungBangGioiHan_VanHopLe()
    {
        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(HoSo(), Ngay, QuyDinh, 2, 1);

        Assert.Empty(dsLyDo);
    }

    [Fact]
    public void SinhVien_VuaHetHanTheVuaVuotGioiHan_TraCaHaiLyDo()
    {
        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(HoSo(hanThe: Ngay.AddDays(-1)), Ngay, QuyDinh, 3, 1);

        Assert.Equal(new[] { "SV_THE_HET_HAN", "VUOT_GIOI_HAN_THIET_BI" }, dsLyDo.Select(d => d.MaLoi).ToArray());
    }

    [Fact]
    public void SinhVien_VuaHetHanTheVuaBiKhoa_TraCaHaiLyDo()
    {
        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(
            HoSo(hanThe: Ngay.AddDays(-1), trangThaiQuyenMuon: "TAM_KHOA"),
            Ngay,
            QuyDinh,
            0,
            1);

        Assert.Equal(
            new[] { "SV_THE_HET_HAN", "SV_BI_KHOA_QUYEN_MUON" },
            dsLyDo.Select(d => d.MaLoi).ToArray());
    }

    [Fact]
    public void KhaDung_ThieuSoLuong_CoLyDo()
    {
        var lyDo = KiemTraDieuKienGiao.KiemTraKhaDung("Loại X", 1, 2);

        Assert.NotNull(lyDo);
        Assert.Equal("SO_LUONG_KHA_DUNG_THAY_DOI", lyDo.MaLoi);
    }

    [Fact]
    public void KhaDung_DungBangSoLuongCan_KhongCoLyDo()
    {
        Assert.Null(KiemTraDieuKienGiao.KiemTraKhaDung("Loại X", 2, 2));
    }
}
