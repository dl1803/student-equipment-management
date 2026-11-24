using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.SharedKernel;
using static QuanLyMuonTra.IntegrationTests.MuonTra.DocPhieuMuonTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class DocPhieuMuonQuayTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [CanKetNoiTestFact]
    public async Task UC07_DanhSach_ChiPhieuCoThietBiThuocPhongDuocCapVaPhanTrangOnDinh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boXuLy = new DsPhieuMuonQuayHandler(db, new QuyenPhongGia(P1));

        var trang1 = await boXuLy.XuLyAsync(Nv1, null, null, null, 1, 2, Ct);
        var trang2 = await boXuLy.XuLyAsync(Nv1, null, null, null, 2, 2, Ct);
        var trang3 = await boXuLy.XuLyAsync(Nv1, null, null, null, 3, 2, Ct);

        Assert.Equal(5, trang1.TongSo);
        Assert.Equal(new[] { PmA6, PmA2 }, trang1.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(new[] { PmA3, PmA1 }, trang2.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(new[] { PmA5 }, trang3.DanhSach.Select(p => p.MaPhieu).ToArray());
        var dauTien = trang1.DanhSach[0];
        Assert.Equal(SvA, dauTien.SinhVien.MaSV);
        Assert.Equal("Sinh viên D11 A", dauTien.SinhVien.HoTen);
        Assert.Equal("L1", dauTien.SinhVien.Lop);
        Assert.Equal(1, dauTien.SoThietBi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_DanhSach_LocTheoTrangThaiMaSvVaMaPhong()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var chiP1 = new DsPhieuMuonQuayHandler(db, new QuyenPhongGia(P1));
        var caHaiPhong = new DsPhieuMuonQuayHandler(db, new QuyenPhongGia(P1, P2));
        var chiP2 = new DsPhieuMuonQuayHandler(db, new QuyenPhongGia(P2));
        var khongPhong = new DsPhieuMuonQuayHandler(db, new QuyenPhongGia());

        var daTra = await chiP1.XuLyAsync(Nv1, null, "DA_TRA", null, 1, 20, Ct);
        var theoSv = await chiP1.XuLyAsync(Nv1, "  D11-SV-B  ", null, null, 1, 20, Ct);
        var ngoaiQuyen = await chiP1.XuLyAsync(Nv1, null, null, P2, 1, 20, Ct);
        var tatCa = await caHaiPhong.XuLyAsync(Nv1, null, null, null, 1, 20, Ct);
        var phong2 = await caHaiPhong.XuLyAsync(Nv1, null, null, P2, 1, 20, Ct);
        var quyenP2 = await chiP2.XuLyAsync(Nv2, null, null, null, 1, 20, Ct);
        var rong = await khongPhong.XuLyAsync(Nv1, null, null, null, 1, 20, Ct);

        Assert.Equal(new[] { PmA5 }, daTra.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(new[] { PmA2, PmA3 }, theoSv.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(0, ngoaiQuyen.TongSo);
        Assert.Empty(ngoaiQuyen.DanhSach);
        Assert.Equal(new[] { PmA6, PmA2, PmA3, PmA1, PmA5, PmA4 }, tatCa.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(new[] { PmA4 }, phong2.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(new[] { PmA4 }, quyenP2.DanhSach.Select(p => p.MaPhieu).ToArray());
        Assert.Equal(0, rong.TongSo);
    }

    [CanKetNoiTestFact]
    public async Task UC07_DanhSach_TrangThaiLocKhongHopLeTra400()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boXuLy = new DsPhieuMuonQuayHandler(db, new QuyenPhongGia(P1));

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Nv1, null, "CHO_DUYET", null, 1, 20, Ct));

        Assert.Equal(400, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC09_ChiTiet_PhongDuocCap_TraDayDuVaKyAnhSauKhiKiemQuyen()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var anh = new AnhServiceGhiNhan();
        var boXuLy = new ChiTietPhieuMuonQuayHandler(db, new DanhMucReader(db), anh, new QuyenPhongGia(P1));

        var phieu = await boXuLy.XuLyAsync(Nv1, PmA1, Ct);

        Assert.Equal(PmA1, phieu.MaPhieu);
        Assert.Equal(2, phieu.SoThietBi);
        Assert.Equal(4, anh.DaGoi.Count);
        Assert.Equal("MAT", (await boXuLy.XuLyAsync(Nv1, PmA5, Ct)).DsChiTiet.Single(c => c.MaThietBi == "D11-TB-T7").KetQuaKiemTra);
    }

    [CanKetNoiTestFact]
    public async Task UC09_ChiTiet_PhongChuaCapQuyen_Tra403VaKhongKyAnh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var anh = new AnhServiceGhiNhan();
        var chiP2 = new ChiTietPhieuMuonQuayHandler(db, new DanhMucReader(db), anh, new QuyenPhongGia(P2));
        var chiP1 = new ChiTietPhieuMuonQuayHandler(db, new DanhMucReader(db), anh, new QuyenPhongGia(P1));

        var loiA1 = await Assert.ThrowsAsync<LoiNghiepVuException>(() => chiP2.XuLyAsync(Nv2, PmA1, Ct));
        var loiA4 = await Assert.ThrowsAsync<LoiNghiepVuException>(() => chiP1.XuLyAsync(Nv1, PmA4, Ct));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loiA1.MaLoi);
        Assert.Equal(403, loiA1.MaHttp);
        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loiA4.MaLoi);
        Assert.Empty(anh.DaGoi);
    }

    [CanKetNoiTestFact]
    public async Task UC09_ChiTiet_KhongTonTaiTra404_SaiDinhDangTra400()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boXuLy = new ChiTietPhieuMuonQuayHandler(db, new DanhMucReader(db), new AnhServiceGhiNhan(), new QuyenPhongGia(P1));

        var khongCo = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, "PM-261023-999999", Ct));
        var saiDinhDang = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, "qua-han-30-ngay", Ct));

        Assert.Equal(404, khongCo.MaHttp);
        Assert.Equal(400, saiDinhDang.MaHttp);
    }
}
