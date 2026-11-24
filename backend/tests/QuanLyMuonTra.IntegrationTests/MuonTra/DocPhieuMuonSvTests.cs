using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.SharedKernel;
using static QuanLyMuonTra.IntegrationTests.MuonTra.DocPhieuMuonTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class DocPhieuMuonSvTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [CanKetNoiTestFact]
    public async Task UC05_DanhSach_ChiPhieuCuaMinhMoiNhatTruocVaPhanTrangOnDinh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boXuLy = new DsPhieuMuonSvHandler(db);

        var trang1 = await boXuLy.XuLyAsync(SvA, 1, 1, Ct);
        var trang2 = await boXuLy.XuLyAsync(SvA, 2, 1, Ct);
        var trang3 = await boXuLy.XuLyAsync(SvA, 3, 1, Ct);
        var svB = await boXuLy.XuLyAsync(SvB, 1, 20, Ct);

        Assert.Equal(2, trang1.TongSo);
        Assert.Equal(PmA6, Assert.Single(trang1.DanhSach).MaPhieu);
        Assert.Equal(PmA1, Assert.Single(trang2.DanhSach).MaPhieu);
        Assert.Empty(trang3.DanhSach);
        Assert.Equal(2, trang3.TongSo);
        Assert.Equal(new[] { PmA2, PmA3 }, svB.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.All(svB.DanhSach, p => Assert.Equal(SvB, p.MaSV));
    }

    [CanKetNoiTestFact]
    public async Task UC05_DanhSach_TomTatDungSoThietBiVaTrangThai()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);

        var ketQua = await new DsPhieuMuonSvHandler(db).XuLyAsync(SvA, 1, 20, Ct);

        var a1 = ketQua.DanhSach.Single(p => p.MaPhieu == PmA1);
        Assert.Equal(2, a1.SoThietBi);
        Assert.Equal("QUA_HAN", a1.TrangThai);
        Assert.Equal(0, a1.SoLanGiaHan);
        Assert.Null(a1.MaYeuCau);
        Assert.Null(a1.MaDangKy);
        Assert.Equal(new DateTimeOffset(2026, 10, 23, 9, 0, 0, TimeSpan.Zero), a1.NgayGioHenTra);
    }

    [CanKetNoiTestFact]
    public async Task UC05_ChiTiet_PhieuCuaMinh_DayDuThietBiAnhKyVaKhongCoBienBan()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var anh = new AnhServiceGhiNhan();
        var boXuLy = new ChiTietPhieuMuonSvHandler(db, new DanhMucReader(db), anh);

        var phieu = await boXuLy.XuLyAsync(SvA, PmA1, Ct);

        Assert.Equal(PmA1, phieu.MaPhieu);
        Assert.Equal(SvA, phieu.MaSV);
        Assert.Equal(2, phieu.SoThietBi);
        Assert.Equal(new[] { "D11-TB-T1", "D11-TB-T2" }, phieu.DsChiTiet.Select(c => c.MaThietBi).ToArray());
        var t1 = phieu.DsChiTiet[0];
        Assert.Equal("T1", t1.TenThietBi);
        Assert.Equal("D11 Loai X", t1.TenLoai);
        Assert.Equal(50000, t1.PhiTreMoiNgay);
        Assert.Equal("Còn tốt", t1.TinhTrangBanDau);
        Assert.Null(t1.KetQuaKiemTra);
        Assert.Null(t1.NgayGioTraThucTe);
        var t2 = phieu.DsChiTiet[1];
        Assert.Equal("NGUYEN_VEN", t2.KetQuaKiemTra);
        Assert.NotNull(t2.NgayGioTraThucTe);
        Assert.Equal(CtA1T1, t1.MaChiTiet);
        Assert.Single(t1.DsAnhBanDau);
        Assert.Contains($"{CtA1T1}/BAN_DAU", t1.DsAnhBanDau[0].Url);
        Assert.Single(t1.DsAnhKhiTra);
        Assert.Contains($"{CtA1T1}/KHI_TRA", t1.DsAnhKhiTra[0].Url);
        Assert.Equal(4, anh.DaGoi.Count);
        Assert.Empty(phieu.DsBienBan);
    }

    [CanKetNoiTestFact]
    public async Task UC05_ChiTiet_ThietBiMat_NgayTraRongVaCoBienBanThat()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boXuLy = new ChiTietPhieuMuonSvHandler(db, new DanhMucReader(db), new AnhServiceGhiNhan());

        var phieu = await boXuLy.XuLyAsync(SvC, PmA5, Ct);

        var mat = phieu.DsChiTiet.Single(c => c.MaThietBi == "D11-TB-T7");
        Assert.Equal("MAT", mat.KetQuaKiemTra);
        Assert.Null(mat.NgayGioTraThucTe);
        Assert.Equal("DA_TRA", phieu.TrangThai);
        Assert.Equal(new[] { "BB-261024-997001", "BB-261025-997002" }, phieu.DsBienBan.Select(b => b.MaBienBan).ToArray());
        var chuaNop = phieu.DsBienBan[0];
        Assert.Equal("CHUA_NOP", chuaNop.TrangThai);
        Assert.Equal(10_100_000, chuaNop.TongTien);
        Assert.Equal(SvC, chuaNop.MaSV);
        Assert.Equal(PmA5, chuaNop.MaPhieuMuon);
        Assert.Null(chuaNop.ThoiDiemNop);
        Assert.Equal(new[] { "TRA_TRE", "MAT" }, chuaNop.DsChiTiet.Select(c => c.LoaiViPham).ToArray());
        Assert.Equal(2, chuaNop.DsChiTiet[0].SoNgayTre);
        Assert.Null(chuaNop.DsChiTiet[1].SoNgayTre);
        Assert.Equal(10_000_000, chuaNop.DsChiTiet[1].SoTien);
        Assert.Equal("DA_HUY", phieu.DsBienBan[1].TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task UC05_ChiTiet_PhieuCuaNguoiKhac_Tra403VaKhongKyAnh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var anh = new AnhServiceGhiNhan();
        var boXuLy = new ChiTietPhieuMuonSvHandler(db, new DanhMucReader(db), anh);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(SvB, PmA1, Ct));

        Assert.Equal("KHONG_DU_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
        Assert.Empty(anh.DaGoi);
    }

    [CanKetNoiTestFact]
    public async Task UC05_ChiTiet_KhongTonTaiTra404_SaiDinhDangTra400()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boXuLy = new ChiTietPhieuMuonSvHandler(db, new DanhMucReader(db), new AnhServiceGhiNhan());

        var khongCo = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(SvA, "PM-261023-999999", Ct));
        var saiDinhDang = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(SvA, "abc", Ct));

        Assert.Equal(404, khongCo.MaHttp);
        Assert.Equal(400, saiDinhDang.MaHttp);
    }
}
