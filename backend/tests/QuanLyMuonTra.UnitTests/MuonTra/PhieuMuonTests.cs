using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class PhieuMuonTests
{
    private static ChiTietPhieuMuon ChiTiet(string maThietBi, KetQuaKiemTra? ketQua)
    {
        return new ChiTietPhieuMuon(Guid.NewGuid(), "PM-261116-000001", maThietBi, "Tốt", 50000m, ketQua);
    }

    private static PhieuMuon TaoPhieu(params ChiTietPhieuMuon[] chiTiet)
    {
        var ngayGioMuon = new DateTimeOffset(2026, 11, 16, 2, 0, 0, TimeSpan.Zero);
        return new PhieuMuon(
            "PM-261116-000001",
            "SV001",
            Guid.Parse("019c0000-0000-7000-8000-0000000000a1"),
            ngayGioMuon,
            ngayGioMuon.AddHours(2),
            chiTiet);
    }

    [Fact]
    public void TT08_XetDaTra_MotChiTietChuaCoKetQua_GiuNguyenDangMuon()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1", KetQuaKiemTra.NguyenVen), ChiTiet("TB-2", null));

        phieu.XetDaTra();

        Assert.Equal(TrangThaiPhieuMuon.DangMuon, phieu.TrangThai);
    }

    [Fact]
    public void TT08_XetDaTra_TuQuaHanMotChiTietChuaCoKetQua_GiuNguyenQuaHan()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1", KetQuaKiemTra.NguyenVen), ChiTiet("TB-2", null));
        phieu.DanhDauQuaHan();

        phieu.XetDaTra();

        Assert.Equal(TrangThaiPhieuMuon.QuaHan, phieu.TrangThai);
    }

    [Fact]
    public void TT09_XetDaTra_MoiChiTietCoKetQuaKeCaMat_SangDaTra()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1", KetQuaKiemTra.NguyenVen), ChiTiet("TB-2", KetQuaKiemTra.Mat));

        phieu.XetDaTra();

        Assert.Equal(TrangThaiPhieuMuon.DaTra, phieu.TrangThai);
    }

    [Fact]
    public void TT09_XetDaTra_TuQuaHanMoiChiTietCoKetQua_SangDaTra()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1", KetQuaKiemTra.HuHong), ChiTiet("TB-2", KetQuaKiemTra.ThieuPhuKien));
        phieu.DanhDauQuaHan();

        phieu.XetDaTra();

        Assert.Equal(TrangThaiPhieuMuon.DaTra, phieu.TrangThai);
    }

    [Fact]
    public void TT09_XetDaTra_KhongCoChiTiet_GiuNguyenDangMuon()
    {
        var phieu = TaoPhieu();

        phieu.XetDaTra();

        Assert.Equal(TrangThaiPhieuMuon.DangMuon, phieu.TrangThai);
    }

    [Fact]
    public void TT09_XetDaTra_TuDaTra_NemTrangThaiKhongHopLe()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1", KetQuaKiemTra.NguyenVen));
        phieu.XetDaTra();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.XetDaTra());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void TT08_DanhDauQuaHan_TuDangMuon_SangQuaHan()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1", null));

        phieu.DanhDauQuaHan();

        Assert.Equal(TrangThaiPhieuMuon.QuaHan, phieu.TrangThai);
    }

    [Fact]
    public void TT08_DanhDauQuaHan_TuQuaHan_NemTrangThaiKhongHopLe()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1", null));
        phieu.DanhDauQuaHan();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.DanhDauQuaHan());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void TT09_DanhDauQuaHan_TuDaTra_NemTrangThaiKhongHopLe()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1", KetQuaKiemTra.NguyenVen));
        phieu.XetDaTra();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.DanhDauQuaHan());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }
}
