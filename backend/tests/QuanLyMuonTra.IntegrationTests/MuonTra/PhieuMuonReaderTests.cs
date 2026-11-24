using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using static QuanLyMuonTra.IntegrationTests.MuonTra.DocPhieuMuonTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class PhieuMuonReaderTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [CanKetNoiTestFact]
    public async Task LayPhieuMuon_TheoMaChuoi_TraDungTruongVaNullKhiKhongCo()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boDoc = new PhieuMuonReader(db, new DongHoCoDinh(BayGio));

        var phieu = await boDoc.LayPhieuMuonAsync(PmA1, Ct);
        var khongCo = await boDoc.LayPhieuMuonAsync("PM-261023-999999", Ct);
        var rong = await boDoc.LayPhieuMuonAsync(" ", Ct);

        Assert.NotNull(phieu);
        Assert.Equal(PmA1, phieu.MaPhieu);
        Assert.Equal(SvA, phieu.MaSv);
        Assert.Equal(Nv1, phieu.MaNvLap);
        Assert.Equal("QUA_HAN", phieu.TrangThai);
        Assert.Equal(new DateTimeOffset(2026, 10, 23, 9, 0, 0, TimeSpan.Zero), phieu.NgayGioHenTra);
        Assert.Null(khongCo);
        Assert.Null(rong);
    }

    [CanKetNoiTestFact]
    public async Task LayChiTiet_NguyenVenVaMat_TraDungKetQuaVaNgayTra()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boDoc = new PhieuMuonReader(db, new DongHoCoDinh(BayGio));

        var nguyenVen = await boDoc.LayChiTietAsync(CtA1T2, Ct);
        var mat = await boDoc.LayChiTietAsync(CtA5T7, Ct);
        var chuaTra = await boDoc.LayChiTietAsync(CtA1T1, Ct);
        var khongCo = await boDoc.LayChiTietAsync(Guid.NewGuid(), Ct);

        Assert.NotNull(nguyenVen);
        Assert.Equal("NGUYEN_VEN", nguyenVen.KetQuaKiemTra);
        Assert.Equal(PmA1, nguyenVen.MaPhieu);
        Assert.NotNull(nguyenVen.NgayGioTraThucTe);
        Assert.NotNull(mat);
        Assert.Equal("MAT", mat.KetQuaKiemTra);
        Assert.Null(mat.NgayGioTraThucTe);
        Assert.NotNull(chuaTra);
        Assert.Null(chuaTra.KetQuaKiemTra);
        Assert.Equal(50000m, chuaTra.PhiTreMoiNgay);
        Assert.Null(khongCo);
    }

    [CanKetNoiTestFact]
    public async Task LayChiTietTheoPhieu_SapTheoMaThietBiVaRongKhiKhongCoPhieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boDoc = new PhieuMuonReader(db, new DongHoCoDinh(BayGio));

        var dsChiTiet = await boDoc.LayChiTietTheoPhieuAsync(PmA5, Ct);
        var rong = await boDoc.LayChiTietTheoPhieuAsync("PM-261023-999999", Ct);

        Assert.Equal(new[] { "D11-TB-T6", "D11-TB-T7" }, dsChiTiet.Select(c => c.MaThietBi).ToArray());
        Assert.Empty(rong);
    }

    [CanKetNoiTestFact]
    public async Task LayDsQuaHan30Ngay_ChiLayPhieuTreTren30NgayLichVaConThietBiChuaTra()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boDoc = new PhieuMuonReader(db, new DongHoCoDinh(BayGio));

        var dsPhieu = await boDoc.LayDsQuaHan30NgayAsync(Ct);

        var dsMa = dsPhieu.Select(p => p.MaPhieu).ToList();
        Assert.Contains(PmA4, dsMa);
        Assert.Contains(PmA1, dsMa);
        Assert.Contains(PmA3, dsMa);
        Assert.DoesNotContain(PmA2, dsMa);
        Assert.DoesNotContain(PmA5, dsMa);
        Assert.DoesNotContain(PmA6, dsMa);
        Assert.True(dsMa.IndexOf(PmA4) < dsMa.IndexOf(PmA1));
        Assert.True(dsMa.IndexOf(PmA1) < dsMa.IndexOf(PmA3));
    }
}
