using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class GhiKetQuaKiemTraMienTests
{
    private const string MaPhieu = "PM-261126-914001";
    private static readonly Guid Nv = Guid.Parse("019c0000-0000-7000-8000-0000000d14a1");
    private static readonly Guid Khung = Guid.Parse("019c0000-0000-7000-8000-0000000d14d1");
    private static readonly DateTimeOffset BayGio = new(2026, 11, 26, 3, 0, 0, TimeSpan.Zero);

    private static ChiTietPhieuMuon ChiTiet(string maThietBi, KetQuaKiemTra? ketQua = null)
    {
        return new ChiTietPhieuMuon(Guid.NewGuid(), MaPhieu, maThietBi, "Còn tốt", 50000m, ketQua);
    }

    private static PhieuMuon TaoPhieu(params ChiTietPhieuMuon[] chiTiet)
    {
        var ngayGioMuon = new DateTimeOffset(2026, 11, 26, 1, 0, 0, TimeSpan.Zero);
        return new PhieuMuon(MaPhieu, "D14-SV-A", Nv, ngayGioMuon, ngayGioMuon.AddHours(8), chiTiet);
    }

    [Fact]
    public void UC08_GhiKetQua_NguyenVen_GhiNvGioTraVaKhongCoKhungPhat()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1"), ChiTiet("TB-2"));

        var chiTiet = phieu.GhiKetQuaKiemTra("TB-1", KetQuaKiemTra.NguyenVen, null, null, Nv, BayGio);

        Assert.Equal(KetQuaKiemTra.NguyenVen, chiTiet.KetQuaKiemTra);
        Assert.Equal(BayGio, chiTiet.NgayGioTraThucTe);
        Assert.Equal(Nv, chiTiet.MaNvNhanTra);
        Assert.Null(chiTiet.MaKhung);
        Assert.Null(chiTiet.TinhTrangKhiTra);
    }

    [Theory]
    [InlineData(KetQuaKiemTra.HuHong)]
    [InlineData(KetQuaKiemTra.ThieuPhuKien)]
    public void UC08_GhiKetQua_HuHongHoacThieuPhuKien_LuuMoTaDaTrimVaKhungPhat(KetQuaKiemTra ketQua)
    {
        var phieu = TaoPhieu(ChiTiet("TB-1"));

        var chiTiet = phieu.GhiKetQuaKiemTra("TB-1", ketQua, "  Vỡ màn hình  ", Khung, Nv, BayGio);

        Assert.Equal(ketQua, chiTiet.KetQuaKiemTra);
        Assert.Equal("Vỡ màn hình", chiTiet.TinhTrangKhiTra);
        Assert.Equal(Khung, chiTiet.MaKhung);
        Assert.Equal(BayGio, chiTiet.NgayGioTraThucTe);
        Assert.Equal(Nv, chiTiet.MaNvNhanTra);
    }

    [Fact]
    public void UC08_GhiKetQua_Mat_NgayGioTraNullVaVanGhiNv()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1"));

        var chiTiet = phieu.GhiKetQuaKiemTra("TB-1", KetQuaKiemTra.Mat, null, null, Nv, BayGio);

        Assert.Equal(KetQuaKiemTra.Mat, chiTiet.KetQuaKiemTra);
        Assert.Null(chiTiet.NgayGioTraThucTe);
        Assert.Equal(Nv, chiTiet.MaNvNhanTra);
        Assert.Null(chiTiet.MaKhung);
    }

    [Theory]
    [InlineData(KetQuaKiemTra.HuHong)]
    [InlineData(KetQuaKiemTra.ThieuPhuKien)]
    public void UC08_GhiKetQua_ThieuMoTaHoacKhung_Nem422VaKhongDoiChiTiet(KetQuaKiemTra ketQua)
    {
        var phieu = TaoPhieu(ChiTiet("TB-1"));

        var thieuKhung = Assert.Throws<NghiepVuException>(
            () => phieu.GhiKetQuaKiemTra("TB-1", ketQua, "Vỡ", null, Nv, BayGio));
        var thieuMoTa = Assert.Throws<NghiepVuException>(
            () => phieu.GhiKetQuaKiemTra("TB-1", ketQua, "  ", Khung, Nv, BayGio));

        Assert.Equal("THIEU_MO_TA_HOAC_KHUNG_PHAT", thieuKhung.MaLoi);
        Assert.Equal("THIEU_MO_TA_HOAC_KHUNG_PHAT", thieuMoTa.MaLoi);
        var chiTiet = Assert.Single(phieu.ChiTiet);
        Assert.Null(chiTiet.KetQuaKiemTra);
        Assert.Null(chiTiet.MaNvNhanTra);
    }

    [Theory]
    [InlineData(KetQuaKiemTra.NguyenVen)]
    [InlineData(KetQuaKiemTra.Mat)]
    public void UC08_GhiKetQua_NguyenVenHoacMatCoKhungPhat_NemDuLieuKhongHopLeVaKhongDoiChiTiet(KetQuaKiemTra ketQua)
    {
        var phieu = TaoPhieu(ChiTiet("TB-1"));

        var loi = Assert.Throws<NghiepVuException>(
            () => phieu.GhiKetQuaKiemTra("TB-1", ketQua, null, Khung, Nv, BayGio));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        var chiTiet = Assert.Single(phieu.ChiTiet);
        Assert.Null(chiTiet.KetQuaKiemTra);
    }

    [Fact]
    public void UC08_GhiKetQua_ThietBiKhongCoTrongPhieu_Nem422()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1"));

        var loi = Assert.Throws<NghiepVuException>(
            () => phieu.GhiKetQuaKiemTra("TB-KHAC", KetQuaKiemTra.NguyenVen, null, null, Nv, BayGio));

        Assert.Equal("THIET_BI_KHONG_THUOC_PHIEU", loi.MaLoi);
    }

    [Fact]
    public void UC08_GhiKetQua_GhiLaiKetQuaDaXacNhan_Nem422VaGiuKetQuaCu()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1"), ChiTiet("TB-2"));
        phieu.GhiKetQuaKiemTra("TB-1", KetQuaKiemTra.NguyenVen, null, null, Nv, BayGio);

        var loi = Assert.Throws<NghiepVuException>(
            () => phieu.GhiKetQuaKiemTra("TB-1", KetQuaKiemTra.Mat, null, null, Nv, BayGio.AddHours(1)));

        Assert.Equal("THIET_BI_KHONG_THUOC_PHIEU", loi.MaLoi);
        var chiTiet = phieu.ChiTiet.Single(c => c.MaThietBi == "TB-1");
        Assert.Equal(KetQuaKiemTra.NguyenVen, chiTiet.KetQuaKiemTra);
        Assert.Equal(BayGio, chiTiet.NgayGioTraThucTe);
    }

    [Fact]
    public void UC08_TraTungPhan_GiuDangMuonDenKhiMoiDongCoKetQuaKeCaMat()
    {
        var phieu = TaoPhieu(ChiTiet("TB-1"), ChiTiet("TB-2"));

        phieu.GhiKetQuaKiemTra("TB-1", KetQuaKiemTra.NguyenVen, null, null, Nv, BayGio);
        phieu.XetDaTra();
        Assert.Equal(TrangThaiPhieuMuon.DangMuon, phieu.TrangThai);

        phieu.GhiKetQuaKiemTra("TB-2", KetQuaKiemTra.Mat, null, null, Nv, BayGio);
        phieu.XetDaTra();
        Assert.Equal(TrangThaiPhieuMuon.DaTra, phieu.TrangThai);
    }

    [Fact]
    public void KhungPhatLuuTru_VaMaKetQua_DungBonKetQua()
    {
        Assert.Equal(Khung, QuyTacNhanTra.KhungPhatLuuTru(KetQuaKiemTra.HuHong, Khung));
        Assert.Equal(Khung, QuyTacNhanTra.KhungPhatLuuTru(KetQuaKiemTra.ThieuPhuKien, Khung));
        Assert.Null(QuyTacNhanTra.KhungPhatLuuTru(KetQuaKiemTra.NguyenVen, null));
        Assert.Null(QuyTacNhanTra.KhungPhatLuuTru(KetQuaKiemTra.Mat, null));
        Assert.Equal("NGUYEN_VEN", QuyTacNhanTra.MaKetQua(KetQuaKiemTra.NguyenVen));
        Assert.Equal("HU_HONG", QuyTacNhanTra.MaKetQua(KetQuaKiemTra.HuHong));
        Assert.Equal("THIEU_PHU_KIEN", QuyTacNhanTra.MaKetQua(KetQuaKiemTra.ThieuPhuKien));
        Assert.Equal("MAT", QuyTacNhanTra.MaKetQua(KetQuaKiemTra.Mat));
    }
}
