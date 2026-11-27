using QuanLyMuonTra.Modules.BaoTri.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.BaoTri;

public sealed class BaoTriSuaChuaMienTests
{
    private static readonly Guid MaNv = Guid.Parse("019c0000-0000-7000-8000-0000000c1221");
    private static readonly DateTimeOffset Luc = new(2026, 11, 23, 12, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public void UC11_Lap_DinhKy_HopLe_ConMoVaNgayLapLaUtc()
    {
        var banGhi = BaoTriSuaChua.Lap("TB-01", MaNv, LoaiThucHienBaoTri.BaoTriDinhKy, null, Luc);

        Assert.Equal(7, banGhi.MaBaoTri.Version);
        Assert.Equal("TB-01", banGhi.MaThietBi);
        Assert.Equal(MaNv, banGhi.MaNvThucHien);
        Assert.Equal(Luc.ToUniversalTime(), banGhi.NgayLap);
        Assert.Equal(TimeSpan.Zero, banGhi.NgayLap.Offset);
        Assert.Null(banGhi.MaPhieuMuon);
        Assert.Null(banGhi.KetQua);
        Assert.Null(banGhi.NgayHoanThanh);
        Assert.Null(banGhi.NoiDungThucHien);
    }

    [Fact]
    public void UC11_Lap_SuaChua_CoPhieuMuon_GiuMaPhieu()
    {
        var banGhi = BaoTriSuaChua.Lap("TB-01", MaNv, LoaiThucHienBaoTri.SuaChua, "PM-261123-991203", Luc);

        Assert.Equal("PM-261123-991203", banGhi.MaPhieuMuon);
        Assert.Equal(LoaiThucHienBaoTri.SuaChua, banGhi.LoaiThucHien);
    }

    [Fact]
    public void UC11_Lap_SuaChua_KhongPhieuMuon_HopLe()
    {
        var banGhi = BaoTriSuaChua.Lap("TB-01", MaNv, LoaiThucHienBaoTri.SuaChua, null, Luc);

        Assert.Null(banGhi.MaPhieuMuon);
    }

    [Theory]
    [InlineData("", "BAO_TRI_DINH_KY", null)]
    [InlineData("  ", "SUA_CHUA", null)]
    [InlineData("TB-01", "BAO_TRI_DINH_KY", "PM-261123-991203")]
    [InlineData("TB-01", "SUA_CHUA", "  ")]
    public void UC11_Lap_DauVaoSai_Nem400(string maThietBi, string loai, string? maPhieu)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => BaoTriSuaChua.Lap(maThietBi, MaNv, LoaiThucHienBaoTriMa.ToLoai(loai), maPhieu, Luc));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public void UC11_Lap_ThieuNhanVien_Nem400()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(
            () => BaoTriSuaChua.Lap("TB-01", Guid.Empty, LoaiThucHienBaoTri.BaoTriDinhKy, null, Luc));

        Assert.Equal(400, loi.MaHttp);
    }

    [Theory]
    [InlineData("BAO_TRI_DINH_KY", true)]
    [InlineData("SUA_CHUA", true)]
    [InlineData("sua_chua", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void UC11_LoaiThucHien_TryParse_ChiNhanDungHaiMa(string? ma, bool hopLe)
    {
        Assert.Equal(hopLe, LoaiThucHienBaoTriMa.TryParse(ma, out _));
    }

    [Fact]
    public void UC11_HoanThanh_HopLe_GhiNoiDungKetQuaVaNgayHoanThanhUtc()
    {
        var banGhi = BaoTriSuaChua.Lap("TB-01", MaNv, LoaiThucHienBaoTri.BaoTriDinhKy, null, Luc);
        var ngayHoanThanh = Luc.AddDays(2);

        banGhi.HoanThanh("  Vệ sinh và kiểm tra tổng thể  ", "DUA_VE_SAN_SANG", ngayHoanThanh);

        Assert.Equal("Vệ sinh và kiểm tra tổng thể", banGhi.NoiDungThucHien);
        Assert.Equal("DUA_VE_SAN_SANG", banGhi.KetQua);
        Assert.Equal(ngayHoanThanh.ToUniversalTime(), banGhi.NgayHoanThanh);
        Assert.Equal(TimeSpan.Zero, banGhi.NgayHoanThanh?.Offset);
    }

    [Fact]
    public void UC11_HoanThanh_DeXuatThanhLy_ChapNhanChoSuaChua()
    {
        var banGhi = BaoTriSuaChua.Lap("TB-01", MaNv, LoaiThucHienBaoTri.SuaChua, null, Luc);

        banGhi.HoanThanh("Hư hỏng nặng, không sửa được", "DE_XUAT_THANH_LY", Luc.AddHours(5));

        Assert.Equal("DE_XUAT_THANH_LY", banGhi.KetQua);
        Assert.NotNull(banGhi.NgayHoanThanh);
    }

    [Fact]
    public void UC11_HoanThanh_NgayBangNgayLap_HopLe()
    {
        var banGhi = BaoTriSuaChua.Lap("TB-01", MaNv, LoaiThucHienBaoTri.BaoTriDinhKy, null, Luc);

        banGhi.HoanThanh("Kiểm tra nhanh", "DUA_VE_SAN_SANG", Luc);

        Assert.Equal(Luc.ToUniversalTime(), banGhi.NgayHoanThanh);
    }

    [Theory]
    [InlineData(null, "DUA_VE_SAN_SANG")]
    [InlineData("", "DUA_VE_SAN_SANG")]
    [InlineData("   ", "DE_XUAT_THANH_LY")]
    [InlineData("Đã kiểm tra", null)]
    [InlineData("Đã kiểm tra", "")]
    [InlineData("Đã kiểm tra", "dua_ve_san_sang")]
    [InlineData("Đã kiểm tra", "KHAC")]
    public void UC11_HoanThanh_NoiDungHoacKetQuaSai_Nem400VaGiuHoSoMo(string? noiDung, string? ketQua)
    {
        var banGhi = BaoTriSuaChua.Lap("TB-01", MaNv, LoaiThucHienBaoTri.BaoTriDinhKy, null, Luc);

        var loi = Assert.Throws<LoiNghiepVuException>(() => banGhi.HoanThanh(noiDung, ketQua, Luc.AddDays(1)));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
        Assert.Null(banGhi.NoiDungThucHien);
        Assert.Null(banGhi.KetQua);
        Assert.Null(banGhi.NgayHoanThanh);
    }

    [Fact]
    public void UC11_HoanThanh_NgaySomHonNgayLap_Nem400VaGiuHoSoMo()
    {
        var banGhi = BaoTriSuaChua.Lap("TB-01", MaNv, LoaiThucHienBaoTri.SuaChua, null, Luc);

        var loi = Assert.Throws<LoiNghiepVuException>(
            () => banGhi.HoanThanh("Đã sửa", "DUA_VE_SAN_SANG", Luc.AddSeconds(-1)));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
        Assert.Null(banGhi.KetQua);
        Assert.Null(banGhi.NgayHoanThanh);
    }

    [Fact]
    public void UC11_HoanThanh_LanHai_Nem409VaKhongGhiDe()
    {
        var banGhi = BaoTriSuaChua.Lap("TB-01", MaNv, LoaiThucHienBaoTri.BaoTriDinhKy, null, Luc);
        banGhi.HoanThanh("Lần một", "DUA_VE_SAN_SANG", Luc.AddDays(1));

        var loi = Assert.Throws<LoiNghiepVuException>(
            () => banGhi.HoanThanh("Lần hai", "DE_XUAT_THANH_LY", Luc.AddDays(2)));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("Lần một", banGhi.NoiDungThucHien);
        Assert.Equal("DUA_VE_SAN_SANG", banGhi.KetQua);
        Assert.Equal(Luc.AddDays(1).ToUniversalTime(), banGhi.NgayHoanThanh);
    }
}
