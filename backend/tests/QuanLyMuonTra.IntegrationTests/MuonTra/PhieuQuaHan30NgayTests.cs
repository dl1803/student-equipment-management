using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using static QuanLyMuonTra.IntegrationTests.MuonTra.DocPhieuMuonTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class PhieuQuaHan30NgayTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private static DsPhieuQuaHan30NgayHandler TaoBoXuLy(DocPhieuTestContext db, params Guid[] dsMaPhong)
    {
        return new DsPhieuQuaHan30NgayHandler(db, new DanhMucReader(db), new QuyenPhongGia(dsMaPhong), new DongHoCoDinh(BayGio));
    }

    [CanKetNoiTestFact]
    public async Task UC09_QuaHan30_ChiLayTreTren30NgayLichVaConThietBiChuaTra()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);

        var ketQua = await TaoBoXuLy(db, P1).XuLyAsync(Nv1, null, 1, 20, Ct);

        Assert.Equal(2, ketQua.TongSo);
        Assert.Equal(new[] { PmA1, PmA3 }, ketQua.DanhSach.Select(p => p.MaPhieu).ToArray());
        var a1 = ketQua.DanhSach[0];
        Assert.Equal(31, a1.SoNgayTre);
        Assert.Equal(SvA, a1.SinhVien.MaSV);
        Assert.Equal("Sinh viên D11 A", a1.SinhVien.HoTen);
        Assert.Equal(new DateTimeOffset(2026, 10, 23, 9, 0, 0, TimeSpan.Zero), a1.NgayGioHenTra);
        var thietBi = Assert.Single(a1.DsThietBiChuaTra);
        Assert.Equal("D11-TB-T1", thietBi.MaThietBi);
        Assert.Equal("T1", thietBi.TenThietBi);
        Assert.Equal(31, ketQua.DanhSach[1].SoNgayTre);
    }

    [CanKetNoiTestFact]
    public async Task UC09_QuaHan30_DungMocNgayLichVN_BienQuaNuaDem()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);

        var ketQua = await TaoBoXuLy(db, P1).XuLyAsync(Nv1, null, 1, 20, Ct);

        Assert.DoesNotContain(ketQua.DanhSach, p => p.MaPhieu == PmA2);
        Assert.DoesNotContain(ketQua.DanhSach, p => p.MaPhieu == PmA5);
        Assert.DoesNotContain(ketQua.DanhSach, p => p.MaPhieu == PmA6);
    }

    [CanKetNoiTestFact]
    public async Task UC09_QuaHan30_PhanTrangOnDinhVaPhamViPhong()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var caHai = TaoBoXuLy(db, P1, P2);

        var trang1 = await caHai.XuLyAsync(Nv1, null, 1, 2, Ct);
        var trang2 = await caHai.XuLyAsync(Nv1, null, 2, 2, Ct);
        var chiP2 = await caHai.XuLyAsync(Nv1, P2, 1, 20, Ct);
        var ngoaiQuyen = await TaoBoXuLy(db, P1).XuLyAsync(Nv1, P2, 1, 20, Ct);
        var quyenP2 = await TaoBoXuLy(db, P2).XuLyAsync(Nv2, null, 1, 20, Ct);

        Assert.Equal(3, trang1.TongSo);
        Assert.Equal(new[] { PmA4, PmA1 }, trang1.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(new[] { PmA3 }, trang2.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(53, trang1.DanhSach[0].SoNgayTre);
        Assert.Equal(new[] { PmA4 }, chiP2.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(0, ngoaiQuyen.TongSo);
        Assert.Equal(new[] { PmA4 }, quyenP2.DanhSach.Select(p => p.MaPhieu).ToArray());
    }

    [CanKetNoiTestFact]
    public async Task UC09_QuaHan30_DaTraHetThietBiThiKhongConTrongDanhSach()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        await db.Database.ExecuteSqlAsync($"""
            UPDATE muon_tra.chi_tiet_phieu_muon
            SET ket_qua_kiem_tra = 'NGUYEN_VEN', tinh_trang_khi_tra = 'Nguyên vẹn',
                ngay_gio_tra_thuc_te = {BayGio}, ma_nv_nhan_tra = {Nv1}
            WHERE ma_chi_tiet = {CtA1T1}
            """, Ct);

        var ketQua = await TaoBoXuLy(db, P1).XuLyAsync(Nv1, null, 1, 20, Ct);

        Assert.Equal(new[] { PmA3 }, ketQua.DanhSach.Select(p => p.MaPhieu).ToArray());
    }
}
