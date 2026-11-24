using QuanLyMuonTra.Modules.SinhVien.Contracts;

namespace QuanLyMuonTra.UnitTests.SinhVien;

public sealed class DieuKienMuonSinhVienTests
{
    private static readonly DateOnly HomNay = new(2026, 11, 19);

    private static HoSoSinhVienDto TaoHoSo(
        DateOnly? hanThe = null,
        string trangThaiHoc = "DANG_HOC",
        string trangThaiQuyenMuon = "BINH_THUONG") =>
        new(
            "SV-KT",
            "Sinh Viên Kiểm Thử",
            "L1",
            "K1",
            "DAI_HOC",
            "Đại học",
            hanThe ?? new DateOnly(2027, 12, 31),
            trangThaiHoc,
            trangThaiQuyenMuon);

    [Fact]
    public void BR38_HoSoBinhThuong_KhongCoLyDo()
    {
        var dsLyDo = DieuKienMuonSinhVien.XacDinhLyDoKhongHopLe(TaoHoSo(), HomNay, false);

        Assert.Empty(dsLyDo);
    }

    [Fact]
    public void BR38_HanTheDungNgayHienTai_VanHopLe()
    {
        var dsLyDo = DieuKienMuonSinhVien.XacDinhLyDoKhongHopLe(TaoHoSo(hanThe: HomNay), HomNay, false);

        Assert.Empty(dsLyDo);
    }

    [Fact]
    public void BR38_HanTheTruocNgayHienTai_TheHetHan()
    {
        var dsLyDo = DieuKienMuonSinhVien.XacDinhLyDoKhongHopLe(TaoHoSo(hanThe: HomNay.AddDays(-1)), HomNay, false);

        var lyDo = Assert.Single(dsLyDo);
        Assert.Equal("SV_THE_HET_HAN", lyDo.MaLoi);
        Assert.False(string.IsNullOrWhiteSpace(lyDo.ThongDiep));
    }

    [Theory]
    [InlineData("TOT_NGHIEP")]
    [InlineData("THOI_HOC")]
    [InlineData("DINH_CHI")]
    public void BR38_KhongConDangHoc_KhongConTheoHoc(string trangThaiHoc)
    {
        var dsLyDo = DieuKienMuonSinhVien.XacDinhLyDoKhongHopLe(TaoHoSo(trangThaiHoc: trangThaiHoc), HomNay, false);

        Assert.Equal("SV_KHONG_CON_THEO_HOC", Assert.Single(dsLyDo).MaLoi);
    }

    [Theory]
    [InlineData("TAM_KHOA")]
    [InlineData("CAM_MUON_DAI_HAN")]
    public void BR38_QuyenMuonKhacBinhThuong_BiKhoaQuyenMuon(string trangThaiQuyenMuon)
    {
        var dsLyDo = DieuKienMuonSinhVien.XacDinhLyDoKhongHopLe(TaoHoSo(trangThaiQuyenMuon: trangThaiQuyenMuon), HomNay, false);

        Assert.Equal("SV_BI_KHOA_QUYEN_MUON", Assert.Single(dsLyDo).MaLoi);
    }

    [Fact]
    public void BR38_VuotGioiHan_CoLyDoVuotGioiHan()
    {
        var dsLyDo = DieuKienMuonSinhVien.XacDinhLyDoKhongHopLe(TaoHoSo(), HomNay, true);

        Assert.Equal("VUOT_GIOI_HAN_THIET_BI", Assert.Single(dsLyDo).MaLoi);
    }

    [Fact]
    public void BR38_NhieuLyDo_TheoThuTuCoDinh()
    {
        var hoSo = TaoHoSo(HomNay.AddDays(-1), "THOI_HOC", "TAM_KHOA");

        var dsLyDo = DieuKienMuonSinhVien.XacDinhLyDoKhongHopLe(hoSo, HomNay, true);

        Assert.Equal(
            new[] { "SV_THE_HET_HAN", "SV_KHONG_CON_THEO_HOC", "SV_BI_KHOA_QUYEN_MUON", "VUOT_GIOI_HAN_THIET_BI" },
            dsLyDo.Select(x => x.MaLoi).ToArray());
    }
}

