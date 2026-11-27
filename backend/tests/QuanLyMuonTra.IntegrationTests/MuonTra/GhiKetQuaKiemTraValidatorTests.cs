using QuanLyMuonTra.Api.MuonTra;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class GhiKetQuaKiemTraValidatorTests
{
    private static readonly GhiKetQuaKiemTraValidator KiemTra = new();
    private static readonly Guid Khung = Guid.Parse("019c0000-0000-7000-8000-0000000d14d1");

    private static KetQuaKiemTraThietBiRequest Dong(
        string? ma = "TB-01",
        string? ketQua = "NGUYEN_VEN",
        string? moTa = null,
        Guid? khung = null,
        IReadOnlyList<Guid>? dsMaAnh = null)
    {
        return new KetQuaKiemTraThietBiRequest(ma, ketQua, moTa, khung, dsMaAnh!);
    }

    private static GhiKetQuaKiemTraRequest Yeu(params KetQuaKiemTraThietBiRequest[] dong)
    {
        return new GhiKetQuaKiemTraRequest(dong);
    }

    [Fact]
    public void BonKetQuaHopLe_TrongMotLo_DatKiemTra()
    {
        var yeuCau = Yeu(
            Dong("TB-01", "NGUYEN_VEN"),
            Dong("TB-02", "HU_HONG", "Vỡ màn hình", Khung),
            Dong("TB-03", "THIEU_PHU_KIEN", "Mất sạc", Khung),
            Dong("TB-04", "MAT"));

        Assert.True(KiemTra.Validate(yeuCau).IsValid);
    }

    [Fact]
    public void DsKetQuaRongHoacNull_KhongDat()
    {
        Assert.False(KiemTra.Validate(new GhiKetQuaKiemTraRequest([])).IsValid);
        Assert.False(KiemTra.Validate(new GhiKetQuaKiemTraRequest(null!)).IsValid);
    }

    [Fact]
    public void DongNull_KhongDat()
    {
        Assert.False(KiemTra.Validate(new GhiKetQuaKiemTraRequest([null!])).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nguyen_ven")]
    [InlineData("DA_TRA")]
    public void KetQuaKhongThuocBonGiaTri_KhongDat(string? ketQua)
    {
        Assert.False(KiemTra.Validate(Yeu(Dong(ketQua: ketQua))).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MaThietBiTrong_KhongDat(string? ma)
    {
        Assert.False(KiemTra.Validate(Yeu(Dong(ma: ma))).IsValid);
    }

    [Theory]
    [InlineData("NGUYEN_VEN")]
    [InlineData("MAT")]
    public void KhungPhatCoGiaTriVoiNguyenVenHoacMat_KhongDat(string ketQua)
    {
        var ketQuaKiem = KiemTra.Validate(Yeu(Dong(ketQua: ketQua, khung: Khung)));

        Assert.False(ketQuaKiem.IsValid);
        Assert.Contains(ketQuaKiem.Errors, e => e.PropertyName.EndsWith("MaKhungPhat"));
    }

    [Theory]
    [InlineData("HU_HONG")]
    [InlineData("THIEU_PHU_KIEN")]
    public void HuHongHoacThieuPhuKienThieuMoTaHoacKhung_VanQuaValidator_VeDomain422(string ketQua)
    {
        Assert.True(KiemTra.Validate(Yeu(Dong(ketQua: ketQua, moTa: null, khung: null))).IsValid);
    }

    [Fact]
    public void ThietBiTrungTrongLo_KhongDat()
    {
        var yeuCau = Yeu(Dong("TB-01"), Dong("TB-01", "MAT"));

        Assert.False(KiemTra.Validate(yeuCau).IsValid);
    }

    [Fact]
    public void DsMaAnh_TuNamAnhTroXuong_KhongTrungKhongRong_DatKiemTra()
    {
        var nam = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();

        Assert.True(KiemTra.Validate(Yeu(Dong(dsMaAnh: nam))).IsValid);
    }

    [Fact]
    public void DsMaAnh_SauAnhHoacTrungHoacGuidRong_KhongDat()
    {
        var sau = Enumerable.Range(1, 6).Select(_ => Guid.NewGuid()).ToList();
        var trung = new List<Guid> { Khung, Khung };
        var rong = new List<Guid> { Guid.Empty };

        Assert.False(KiemTra.Validate(Yeu(Dong(dsMaAnh: sau))).IsValid);
        Assert.False(KiemTra.Validate(Yeu(Dong(dsMaAnh: trung))).IsValid);
        Assert.False(KiemTra.Validate(Yeu(Dong(dsMaAnh: rong))).IsValid);
    }
}
