using QuanLyMuonTra.IntegrationTests.C3;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Application;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

[Collection(C3BoSuuTap.Ten)]
public sealed class DsQuanTriCsdlTests
{
    private const string MaPhongA = "019c0000-0000-7000-8000-000000000001";
    private const string MaPhongB = "019c0000-0000-7000-8000-000000000002";
    private const string MaLoaiLaptop = "019c0000-0000-7000-8000-000000000011";
    private const string MaLoaiMayChieu = "019c0000-0000-7000-8000-000000000012";
    private const string MaNccA = "019c0000-0000-7000-8000-000000000101";
    private const string MaNccB = "019c0000-0000-7000-8000-000000000102";
    private const string MaNccC = "019c0000-0000-7000-8000-000000000103";

    [CanKetNoiTestFact]
    public async Task UC12_DsPhongThietBi_SapTheoTenVaPhanTrangNhatQuan()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new DsPhongThietBiHandler(db);
        var ct = CancellationToken.None;

        var tatCa = await handler.XuLyAsync(1, 100, ct);
        var trang1 = await handler.XuLyAsync(1, 1, ct);
        var trang2 = await handler.XuLyAsync(2, 1, ct);
        var ngoaiKhoang = await handler.XuLyAsync(tatCa.TongSo + 1, 1, ct);

        var thuTu = tatCa.DanhSach.Select(p => p.MaPhong).ToList();
        Assert.True(tatCa.TongSo >= 2);
        Assert.True(thuTu.IndexOf(Guid.Parse(MaPhongB)) > thuTu.IndexOf(Guid.Parse(MaPhongA)));
        Assert.True(thuTu.IndexOf(Guid.Parse(MaPhongA)) >= 0);
        Assert.Equal(tatCa.TongSo, trang1.TongSo);
        Assert.Equal(1, trang1.Trang);
        Assert.Equal(1, trang1.KichThuoc);
        Assert.Equal(tatCa.DanhSach[0], Assert.Single(trang1.DanhSach));
        Assert.Equal(tatCa.DanhSach[1], Assert.Single(trang2.DanhSach));
        Assert.Equal(tatCa.TongSo, ngoaiKhoang.TongSo);
        Assert.Empty(ngoaiKhoang.DanhSach);
    }

    [CanKetNoiTestFact]
    public async Task UC12_DsLoaiThietBi_LocNgungSuDungChiaHaiTapKhongGiao()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await C3TestSupport.ThucThiAsync(
            gd,
            $"UPDATE danh_muc.loai_thiet_bi SET ngung_su_dung = true WHERE ma_loai = '{MaLoaiMayChieu}'");
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new DsLoaiThietBiHandler(db);
        var ct = CancellationToken.None;

        var tatCa = await handler.XuLyAsync(null, 1, 100, ct);
        var dangDung = await handler.XuLyAsync(false, 1, 100, ct);
        var ngung = await handler.XuLyAsync(true, 1, 100, ct);

        Assert.Equal(tatCa.TongSo, dangDung.TongSo + ngung.TongSo);
        Assert.Contains(dangDung.DanhSach, l => l.MaLoai == Guid.Parse(MaLoaiLaptop));
        Assert.DoesNotContain(dangDung.DanhSach, l => l.MaLoai == Guid.Parse(MaLoaiMayChieu));
        Assert.Contains(ngung.DanhSach, l => l.MaLoai == Guid.Parse(MaLoaiMayChieu));
        var laptop = tatCa.DanhSach.Single(l => l.MaLoai == Guid.Parse(MaLoaiLaptop));
        Assert.Equal("Laptop", laptop.TenLoai);
        Assert.Equal(180, laptop.ChuKyBaoTri);
        Assert.Equal(50000L, laptop.PhiTreMoiNgay);
        Assert.Null(laptop.MoTa);
        Assert.Null(laptop.HangSanXuat);
        Assert.False(laptop.NgungSuDung);
    }

    [CanKetNoiTestFact]
    public async Task UC12_DsNhaCungCap_TraBaNccThuMaNccVietHoaVaPhanTrang()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await C3TestSupport.NapDuLieuThuNccAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new DsNhaCungCapHandler(db);
        var ct = CancellationToken.None;

        var tatCa = await handler.XuLyAsync(1, 100, ct);
        var trang1 = await handler.XuLyAsync(1, 1, ct);
        var trang2 = await handler.XuLyAsync(2, 1, ct);

        var thuTu = tatCa.DanhSach.Select(n => n.MaNCC).ToList();
        var viTriA = thuTu.IndexOf(Guid.Parse(MaNccA));
        Assert.True(viTriA >= 0);
        Assert.True(thuTu.IndexOf(Guid.Parse(MaNccB)) > viTriA);
        Assert.True(thuTu.IndexOf(Guid.Parse(MaNccC)) > thuTu.IndexOf(Guid.Parse(MaNccB)));
        var nccA = tatCa.DanhSach.Single(n => n.MaNCC == Guid.Parse(MaNccA));
        var nccB = tatCa.DanhSach.Single(n => n.MaNCC == Guid.Parse(MaNccB));
        Assert.Equal("Nhà cung cấp thử A", nccA.TenNCC);
        Assert.Equal("lienhe-a@example.test", nccA.ThongTinLienHe);
        Assert.Null(nccB.ThongTinLienHe);
        Assert.True(tatCa.TongSo >= 3);
        Assert.Equal(tatCa.DanhSach[0], Assert.Single(trang1.DanhSach));
        Assert.Equal(tatCa.DanhSach[1], Assert.Single(trang2.DanhSach));
    }

    [CanKetNoiTestFact]
    public async Task UC12_DsThietBi_LocTheoLoaiVaPhong_SapTheoMaVaPhanTrangDung()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new DsThietBiHandler(db);
        var ct = CancellationToken.None;
        var loai = Guid.Parse(MaLoaiLaptop);
        var phong = Guid.Parse(MaPhongA);

        var tatCa = await handler.XuLyAsync(null, loai, phong, null, 1, 100, ct);
        var trang1 = await handler.XuLyAsync(null, loai, phong, null, 1, 2, ct);
        var trang2 = await handler.XuLyAsync(null, loai, phong, null, 2, 2, ct);
        var trang3 = await handler.XuLyAsync(null, loai, phong, null, 3, 2, ct);

        Assert.Equal(4, tatCa.TongSo);
        Assert.Equal(
            new[] { "TEST-LAP-001", "TEST-LAP-002", "TEST-LAP-003", "TEST-LAP-004" },
            tatCa.DanhSach.Select(t => t.MaThietBi));
        Assert.Equal(new[] { "TEST-LAP-001", "TEST-LAP-002" }, trang1.DanhSach.Select(t => t.MaThietBi));
        Assert.Equal(new[] { "TEST-LAP-003", "TEST-LAP-004" }, trang2.DanhSach.Select(t => t.MaThietBi));
        Assert.Equal(4, trang3.TongSo);
        Assert.Equal(3, trang3.Trang);
        Assert.Equal(2, trang3.KichThuoc);
        Assert.Empty(trang3.DanhSach);
    }

    [CanKetNoiTestFact]
    public async Task UC12_DsThietBi_LocTheoTinhTrangVaTenKhongPhanBietHoaThuong()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new DsThietBiHandler(db);
        var ct = CancellationToken.None;

        var sanSang = await handler.XuLyAsync(null, null, Guid.Parse(MaPhongA), "SAN_SANG", 1, 100, ct);
        var theoTen = await handler.XuLyAsync("MÁY CHIẾU THỬ", null, Guid.Parse(MaPhongB), null, 1, 100, ct);

        Assert.Equal(
            new[] { "TEST-LAP-001", "TEST-LAP-002", "TEST-MC-001" },
            sanSang.DanhSach.Select(t => t.MaThietBi));
        Assert.All(sanSang.DanhSach, t => Assert.Equal("SAN_SANG", t.TinhTrang));
        Assert.Equal(new[] { "TEST-MC-101", "TEST-MC-102" }, theoTen.DanhSach.Select(t => t.MaThietBi));
    }

    [CanKetNoiTestFact]
    public async Task UC12_DsThietBi_ChiTietNhaCungCapVaGiaTriHienHanh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await C3TestSupport.NapDuLieuThuNccAsync(gd);
        await C3TestSupport.ThucThiAsync(
            gd,
            "INSERT INTO danh_muc.lich_su_gia_tri (ma_lich_su, ma_thiet_bi, gia_tri, ngay_cap_nhat) VALUES ('019c0000-0000-7000-8000-0000000f0002', 'TEST-LAP-002', 17000000, now() + interval '1 hour')");
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new DsThietBiHandler(db);

        var ketQua = await handler.XuLyAsync(null, null, Guid.Parse(MaPhongA), null, 1, 100, CancellationToken.None);

        var lap001 = ketQua.DanhSach.Single(t => t.MaThietBi == "TEST-LAP-001");
        var lap002 = ketQua.DanhSach.Single(t => t.MaThietBi == "TEST-LAP-002");
        var lap003 = ketQua.DanhSach.Single(t => t.MaThietBi == "TEST-LAP-003");
        var mc001 = ketQua.DanhSach.Single(t => t.MaThietBi == "TEST-MC-001");
        Assert.Equal("Laptop", lap001.TenLoai);
        Assert.Equal("Phòng Thiết bị A", lap001.TenPhong);
        Assert.Equal(new DateOnly(2026, 1, 5), lap001.NgayNhapKho);
        Assert.Null(lap001.ThoiHanBaoHanh);
        Assert.Equal("SAN_SANG", lap001.TinhTrang);
        Assert.Equal(15000000L, lap001.GiaTriHienHanh);
        Assert.Equal(17000000L, lap002.GiaTriHienHanh);
        Assert.Equal(8000000L, mc001.GiaTriHienHanh);
        Assert.Equal(new[] { Guid.Parse(MaNccA), Guid.Parse(MaNccB) }, lap001.DsNhaCungCap.Select(n => n.MaNCC));
        Assert.Equal(new[] { Guid.Parse(MaNccB), Guid.Parse(MaNccC) }, mc001.DsNhaCungCap.Select(n => n.MaNCC));
        Assert.Empty(lap003.DsNhaCungCap);
    }
}
