using QuanLyMuonTra.Api.MuonTra;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class LapPhieuMuonValidatorTests
{
    private static readonly LapPhieuMuonValidator KiemTra = new();
    private static readonly Guid Phong = Guid.Parse("019c0000-0000-7000-8000-0000000d9b01");
    private static readonly Guid Loai = Guid.Parse("019c0000-0000-7000-8000-0000000d9c01");

    private static ThietBiGiaoRequest ThietBi(string? ma = "TB-01", string? moTa = "Tình trạng tốt", IReadOnlyList<Guid>? dsMaAnh = null)
    {
        return new ThietBiGiaoRequest(ma, moTa, dsMaAnh!);
    }

    private static LapPhieuMuonRequest TheoYeuCau(string? ma = "YC-261121-000001", params ThietBiGiaoRequest[] dsThietBi)
    {
        return new LapPhieuMuonRequest(
            LapPhieuMuonRequest.PhieuYeuCau,
            ma,
            null,
            null,
            null,
            null!,
            dsThietBi.Length == 0 ? [ThietBi()] : dsThietBi);
    }

    private static LapPhieuMuonRequest TrucTiep(
        string? maSv = "SV-01",
        Guid? maPhong = null,
        IReadOnlyList<LoaiVaSoLuongRequest>? dsLoai = null)
    {
        return new LapPhieuMuonRequest(
            LapPhieuMuonRequest.TrucTiep,
            null,
            null,
            maSv,
            maPhong ?? Phong,
            dsLoai ?? [new LoaiVaSoLuongRequest(Loai, 1)],
            [ThietBi()]);
    }

    [Fact]
    public void BaNguonHopLe_QuaKiemTra()
    {
        var theoDangKy = new LapPhieuMuonRequest(LapPhieuMuonRequest.DangKyCho, null, "DK-261121-000001", null, null, null!, [ThietBi()]);

        Assert.True(KiemTra.Validate(TheoYeuCau()).IsValid);
        Assert.True(KiemTra.Validate(theoDangKy).IsValid);
        Assert.True(KiemTra.Validate(TrucTiep()).IsValid);
    }

    [Fact]
    public void ThieuHoacSaiNguon_KhongHopLe()
    {
        var thieu = new LapPhieuMuonRequest(null, null, null, null, null, null!, [ThietBi()]);
        var sai = new LapPhieuMuonRequest("KHAC", null, null, null, null, null!, [ThietBi()]);

        Assert.False(KiemTra.Validate(thieu).IsValid);
        Assert.False(KiemTra.Validate(sai).IsValid);
    }

    [Fact]
    public void MaNguonSaiDinhDang_KhongHopLe()
    {
        Assert.False(KiemTra.Validate(TheoYeuCau("YC-1")).IsValid);
        Assert.False(KiemTra.Validate(TheoYeuCau(null)).IsValid);
        Assert.False(KiemTra.Validate(new LapPhieuMuonRequest(LapPhieuMuonRequest.DangKyCho, null, "DK-1", null, null, null!, [ThietBi()])).IsValid);
    }

    [Fact]
    public void DanhSachThietBiRongHoacNull_KhongHopLe()
    {
        var rong = new LapPhieuMuonRequest(LapPhieuMuonRequest.PhieuYeuCau, "YC-261121-000001", null, null, null, null!, []);
        var khongCo = new LapPhieuMuonRequest(LapPhieuMuonRequest.PhieuYeuCau, "YC-261121-000001", null, null, null, null!, null!);

        Assert.False(KiemTra.Validate(rong).IsValid);
        Assert.False(KiemTra.Validate(khongCo).IsValid);
    }

    [Fact]
    public void MoTaRongHoacChiKhoangTrang_KhongHopLe()
    {
        Assert.False(KiemTra.Validate(TheoYeuCau(dsThietBi: ThietBi(moTa: "   "))).IsValid);
        Assert.False(KiemTra.Validate(TheoYeuCau(dsThietBi: ThietBi(moTa: null))).IsValid);
        Assert.False(KiemTra.Validate(TheoYeuCau(dsThietBi: ThietBi(ma: ""))).IsValid);
    }

    [Fact]
    public void AnhQuaNamHoacTrungHoacRong_KhongHopLe()
    {
        var saoAnh = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();
        var anh = Guid.NewGuid();

        Assert.True(KiemTra.Validate(TheoYeuCau(dsThietBi: ThietBi(dsMaAnh: saoAnh.Take(5).ToList()))).IsValid);
        Assert.False(KiemTra.Validate(TheoYeuCau(dsThietBi: ThietBi(dsMaAnh: saoAnh))).IsValid);
        Assert.False(KiemTra.Validate(TheoYeuCau(dsThietBi: ThietBi(dsMaAnh: [anh, anh]))).IsValid);
        Assert.False(KiemTra.Validate(TheoYeuCau(dsThietBi: ThietBi(dsMaAnh: [Guid.Empty]))).IsValid);
    }

    [Fact]
    public void TrucTiep_ThieuSvPhongHoacLoai_KhongHopLe()
    {
        Assert.False(KiemTra.Validate(TrucTiep(maSv: "")).IsValid);
        Assert.False(KiemTra.Validate(new LapPhieuMuonRequest(LapPhieuMuonRequest.TrucTiep, null, null, "SV-01", null, [new LoaiVaSoLuongRequest(Loai, 1)], [ThietBi()])).IsValid);
        Assert.False(KiemTra.Validate(TrucTiep(dsLoai: [])).IsValid);
    }

    [Fact]
    public void TrucTiep_LoaiTrungHoacSoLuongKhongDuong_KhongHopLe()
    {
        Assert.False(KiemTra.Validate(TrucTiep(dsLoai: [new LoaiVaSoLuongRequest(Loai, 1), new LoaiVaSoLuongRequest(Loai, 2)])).IsValid);
        Assert.False(KiemTra.Validate(TrucTiep(dsLoai: [new LoaiVaSoLuongRequest(Loai, 0)])).IsValid);
    }
}
