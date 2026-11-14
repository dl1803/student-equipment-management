using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class PhieuYeuCauMuonTests
{
    private static readonly DateTimeOffset ThoiDiemDuyet = new(2026, 11, 16, 2, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset HanGiuCho = ThoiDiemDuyet.AddMinutes(30);
    private static readonly Guid MaNv = Guid.Parse("019c0000-0000-7000-8000-0000000000a1");

    private static PhieuYeuCauMuon TaoChoDuyet()
    {
        return new PhieuYeuCauMuon(
            "YC-261116-000001",
            "SV001",
            Guid.Parse("019c0000-0000-7000-8000-000000000001"),
            2,
            ThoiDiemDuyet.AddHours(-1));
    }

    private static PhieuYeuCauMuon TaoDaDuyet()
    {
        var phieu = TaoChoDuyet();
        phieu.Duyet(MaNv, ThoiDiemDuyet, HanGiuCho);
        return phieu;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void KhoiTao_ThoiGianMuonKhongHopLe_NemDuLieuKhongHopLe(int thoiGian)
    {
        var loi = Assert.Throws<NghiepVuException>(() => new PhieuYeuCauMuon(
            "YC-261116-000001",
            "SV001",
            Guid.Parse("019c0000-0000-7000-8000-000000000001"),
            thoiGian,
            ThoiDiemDuyet.AddHours(-1)));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void ThemChiTiet_ChoDuyet_ThemThanhCong()
    {
        var phieu = TaoChoDuyet();
        var maLoai = Guid.Parse("019c0000-0000-7000-8000-000000000011");

        phieu.ThemChiTiet(maLoai, 2);

        Assert.Single(phieu.ChiTiet);
        Assert.Equal(maLoai, phieu.ChiTiet.Single().MaLoai);
        Assert.Equal(2, phieu.ChiTiet.Single().SoLuong);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThemChiTiet_SoLuongKhongHopLe_NemDuLieuKhongHopLe(int soLuong)
    {
        var phieu = TaoChoDuyet();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.ThemChiTiet(Guid.NewGuid(), soLuong));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void ThemChiTiet_DaDuyet_NemTrangThaiKhongHopLe()
    {
        var phieu = TaoDaDuyet();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.ThemChiTiet(Guid.NewGuid(), 1));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void TT01_Duyet_ChoDuyetSangDaDuyetVaGhiDayDuThongTin()
    {
        var phieu = TaoChoDuyet();

        phieu.Duyet(MaNv, ThoiDiemDuyet, HanGiuCho);

        Assert.Equal(TrangThaiYeuCauMuon.DaDuyet, phieu.TrangThai);
        Assert.Equal(MaNv, phieu.MaNvDuyet);
        Assert.Equal(ThoiDiemDuyet, phieu.ThoiDiemDuyet);
        Assert.Equal(HanGiuCho, phieu.HanGiuCho);
    }

    [Fact]
    public void TT02_Duyet_LanHai_NemTrangThaiKhongHopLe()
    {
        var phieu = TaoDaDuyet();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.Duyet(MaNv, ThoiDiemDuyet, HanGiuCho));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(TrangThaiYeuCauMuon.DaDuyet, phieu.TrangThai);
    }

    [Fact]
    public void TT01_Duyet_HanGiuChoKhongSauThoiDiemDuyet_NemDuLieuKhongHopLeVaKhongDoiTrangThai()
    {
        var phieu = TaoChoDuyet();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.Duyet(MaNv, ThoiDiemDuyet, ThoiDiemDuyet));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(TrangThaiYeuCauMuon.ChoDuyet, phieu.TrangThai);
        Assert.Null(phieu.HanGiuCho);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TT03_TuChoi_ThieuLyDo_NemDuLieuKhongHopLeVaKhongDoiTrangThai(string? lyDo)
    {
        var phieu = TaoChoDuyet();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.TuChoi(MaNv, lyDo!));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(TrangThaiYeuCauMuon.ChoDuyet, phieu.TrangThai);
        Assert.Null(phieu.LyDoTuChoi);
    }

    [Fact]
    public void TT04_TuChoi_CoLyDo_SangTuChoiVaKhongGiuCho()
    {
        var phieu = TaoChoDuyet();

        phieu.TuChoi(MaNv, "Không đủ thiết bị");

        Assert.Equal(TrangThaiYeuCauMuon.TuChoi, phieu.TrangThai);
        Assert.Equal("Không đủ thiết bị", phieu.LyDoTuChoi);
        Assert.Equal(MaNv, phieu.MaNvDuyet);
        Assert.Null(phieu.HanGiuCho);
    }

    [Fact]
    public void TT04_TuChoi_TuDaDuyet_NemTrangThaiKhongHopLe()
    {
        var phieu = TaoDaDuyet();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.TuChoi(MaNv, "Lý do"));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void TT01_DanhDauDaNhan_TuDaDuyet_SangDaNhanVaGiuThongTinDuyet()
    {
        var phieu = TaoDaDuyet();

        phieu.DanhDauDaNhan();

        Assert.Equal(TrangThaiYeuCauMuon.DaNhan, phieu.TrangThai);
        Assert.Equal(HanGiuCho, phieu.HanGiuCho);
    }

    [Fact]
    public void TT02_DanhDauDaNhan_TuChoDuyet_NemTrangThaiKhongHopLe()
    {
        var phieu = TaoChoDuyet();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.DanhDauDaNhan());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void TT01_HetHan_TuChoDuyet_SangHetHan()
    {
        var phieu = TaoChoDuyet();

        phieu.HetHan();

        Assert.Equal(TrangThaiYeuCauMuon.HetHan, phieu.TrangThai);
    }

    [Fact]
    public void TT01_HetHan_TuDaDuyet_SangHetHanVaGiuThongTinDuyet()
    {
        var phieu = TaoDaDuyet();

        phieu.HetHan();

        Assert.Equal(TrangThaiYeuCauMuon.HetHan, phieu.TrangThai);
        Assert.Equal(HanGiuCho, phieu.HanGiuCho);
        Assert.Equal(ThoiDiemDuyet, phieu.ThoiDiemDuyet);
        Assert.Equal(MaNv, phieu.MaNvDuyet);
    }

    [Fact]
    public void TT02_HetHan_TuTuChoi_NemTrangThaiKhongHopLe()
    {
        var phieu = TaoChoDuyet();
        phieu.TuChoi(MaNv, "Lý do");

        var loi = Assert.Throws<NghiepVuException>(() => phieu.HetHan());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public void TT01_Huy_TuChoDuyet_SangDaHuy()
    {
        var phieu = TaoChoDuyet();

        phieu.Huy();

        Assert.Equal(TrangThaiYeuCauMuon.DaHuy, phieu.TrangThai);
    }

    [Fact]
    public void TT01_Huy_TuDaDuyet_SangDaHuyVaGiuThongTinDuyet()
    {
        var phieu = TaoDaDuyet();

        phieu.Huy();

        Assert.Equal(TrangThaiYeuCauMuon.DaHuy, phieu.TrangThai);
        Assert.Equal(HanGiuCho, phieu.HanGiuCho);
        Assert.Equal(ThoiDiemDuyet, phieu.ThoiDiemDuyet);
        Assert.Equal(MaNv, phieu.MaNvDuyet);
    }

    [Fact]
    public void TT02_Huy_TuDaNhan_NemTrangThaiKhongHopLe()
    {
        var phieu = TaoDaDuyet();
        phieu.DanhDauDaNhan();

        var loi = Assert.Throws<NghiepVuException>(() => phieu.Huy());

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }
}
