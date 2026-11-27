using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class QuyTacNhanTraTests
{
    private static readonly Guid Khung = Guid.Parse("019c0000-0000-7000-8000-0000000d14d1");
    private static readonly DateTimeOffset BayGio = new(2026, 11, 26, 3, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(KetQuaKiemTra.NguyenVen)]
    [InlineData(KetQuaKiemTra.Mat)]
    public void KiemTraDauVao_NguyenVenHoacMat_KhongYeuCauMoTaVaKhungPhat(KetQuaKiemTra ketQua)
    {
        QuyTacNhanTra.KiemTraDauVao(ketQua, null, null);
        QuyTacNhanTra.KiemTraDauVao(ketQua, "Ghi chú thêm", null);
    }

    [Theory]
    [InlineData(KetQuaKiemTra.HuHong)]
    [InlineData(KetQuaKiemTra.ThieuPhuKien)]
    public void KiemTraDauVao_HuHongHoacThieuPhuKien_DuMoTaVaKhung_KhongNemLoi(KetQuaKiemTra ketQua)
    {
        QuyTacNhanTra.KiemTraDauVao(ketQua, "Vỡ màn hình", Khung);
    }

    [Theory]
    [InlineData(KetQuaKiemTra.HuHong)]
    [InlineData(KetQuaKiemTra.ThieuPhuKien)]
    public void KiemTraDauVao_HuHongHoacThieuPhuKien_ThieuMoTaHoacKhung_Nem422(KetQuaKiemTra ketQua)
    {
        var cacCa = new (string? MoTa, Guid? MaKhung)[]
        {
            (null, Khung),
            ("", Khung),
            ("   ", Khung),
            ("Vỡ màn hình", null),
            ("Vỡ màn hình", Guid.Empty),
            (null, null)
        };

        foreach (var (moTa, maKhung) in cacCa)
        {
            var loi = Assert.Throws<NghiepVuException>(() => QuyTacNhanTra.KiemTraDauVao(ketQua, moTa, maKhung));

            Assert.Equal("THIEU_MO_TA_HOAC_KHUNG_PHAT", loi.MaLoi);
        }
    }

    [Fact]
    public void NgayGioTraThucTe_Mat_LaNull_BaKetQuaConLaiLaGioHienTai()
    {
        Assert.Null(QuyTacNhanTra.NgayGioTraThucTe(KetQuaKiemTra.Mat, BayGio));
        Assert.Equal(BayGio, QuyTacNhanTra.NgayGioTraThucTe(KetQuaKiemTra.NguyenVen, BayGio));
        Assert.Equal(BayGio, QuyTacNhanTra.NgayGioTraThucTe(KetQuaKiemTra.HuHong, BayGio));
        Assert.Equal(BayGio, QuyTacNhanTra.NgayGioTraThucTe(KetQuaKiemTra.ThieuPhuKien, BayGio));
    }

    [Fact]
    public void KiemTraThietBiThuocPhieu_DungPhieu_KhongNemLoi()
    {
        QuyTacNhanTra.KiemTraThietBiThuocPhieu("PM-261126-914001", "PM-261126-914001", "D14-TB-T1");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("PM-261126-914002")]
    public void KiemTraThietBiThuocPhieu_KhongCoPhieuHoacSaiChuSoHuu_Nem422(string? maPhieuDangHieuLuc)
    {
        var loi = Assert.Throws<NghiepVuException>(
            () => QuyTacNhanTra.KiemTraThietBiThuocPhieu(maPhieuDangHieuLuc, "PM-261126-914001", "D14-TB-T1"));

        Assert.Equal("THIET_BI_KHONG_THUOC_PHIEU", loi.MaLoi);
    }

    [Fact]
    public void MaLoi_HaiMaMoi_MapSang422()
    {
        Assert.Equal(422, MaLoiNghiepVu.LayMaHttp(MaLoi.ThietBiKhongThuocPhieu));
        Assert.Equal(422, MaLoiNghiepVu.LayMaHttp(MaLoi.ThieuMoTaHoacKhungPhat));
    }
}
