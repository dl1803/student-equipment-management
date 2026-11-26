using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.ViPham.Application;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.ViPham;

public class DocBienBanViPhamTests
{
    [Fact]
    public async Task UC05_Sv_ThayBienBanCuaMinh_ChiTietVaTongTienKhongBiNhanDoi()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var canh = await TaoCanhAsync(db);
        var boXuLy = new DsBienBanViPhamSvHandler(db);

        var trang = await boXuLy.XuLyAsync(canh.MaSv, 1, 20, CancellationToken.None);
        var nguoiKhac = await boXuLy.XuLyAsync("KT14-SV-KHONG-CO", 1, 20, CancellationToken.None);

        Assert.Equal(2, trang.TongSo);
        Assert.Equal(canh.BienBanB, trang.DanhSach[0].MaBienBan);
        Assert.Equal(canh.BienBanA, trang.DanhSach[1].MaBienBan);
        var bienBanB = trang.DanhSach[0];
        Assert.Equal(canh.MaSv, bienBanB.MaSV);
        Assert.Equal(500000L, bienBanB.TongTien);
        var chiTietB = Assert.Single(bienBanB.DsChiTiet);
        Assert.Equal("MAT", chiTietB.LoaiViPham);
        Assert.Null(chiTietB.SoNgayTre);
        Assert.Null(chiTietB.MaKhungPhat);
        var bienBanA = trang.DanhSach[1];
        Assert.Equal(300000L, bienBanA.TongTien);
        Assert.Equal(2, bienBanA.DsChiTiet.Count);
        Assert.Equal(bienBanA.TongTien, bienBanA.DsChiTiet.Sum(c => c.SoTien));
        Assert.Equal(canh.MaKhung, bienBanA.DsChiTiet.Single(c => c.LoaiViPham == "HU_HONG").MaKhungPhat);
        Assert.Equal(2, bienBanA.DsChiTiet.Single(c => c.LoaiViPham == "TRA_TRE").SoNgayTre);
        Assert.Equal(0, nguoiKhac.TongSo);
        Assert.Empty(nguoiKhac.DanhSach);
    }

    [Fact]
    public async Task UC09_Sv_PhanTrangGiuNguyenTongSoVaThuTu()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var canh = await TaoCanhAsync(db);
        var boXuLy = new DsBienBanViPhamSvHandler(db);

        var trang1 = await boXuLy.XuLyAsync(canh.MaSv, 1, 1, CancellationToken.None);
        var trang2 = await boXuLy.XuLyAsync(canh.MaSv, 2, 1, CancellationToken.None);
        var trang3 = await boXuLy.XuLyAsync(canh.MaSv, 3, 1, CancellationToken.None);

        Assert.Equal(2, trang1.TongSo);
        Assert.Equal(canh.BienBanB, Assert.Single(trang1.DanhSach).MaBienBan);
        Assert.Equal(canh.BienBanA, Assert.Single(trang2.DanhSach).MaBienBan);
        Assert.Equal(2, trang3.TongSo);
        Assert.Empty(trang3.DanhSach);
    }

    [Fact]
    public async Task UC09_Quay_ChiThayBienBanCuaPhongDuocCap_TongNoSvVanLaToanHeThong()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var canh = await TaoCanhAsync(db);

        var chiPhongA = await Quay(db, canh.PhongA).XuLyAsync(canh.MaNv, canh.MaSv, null, null, 1, 20, CancellationToken.None);
        var chiPhongB = await Quay(db, canh.PhongB).XuLyAsync(canh.MaNv, canh.MaSv, null, null, 1, 20, CancellationToken.None);
        var caHaiPhong = await Quay(db, canh.PhongA, canh.PhongB).XuLyAsync(canh.MaNv, canh.MaSv, null, null, 1, 20, CancellationToken.None);
        var maPhongNgoaiQuyen = await Quay(db, canh.PhongA).XuLyAsync(canh.MaNv, canh.MaSv, null, canh.PhongB, 1, 20, CancellationToken.None);
        var khongCoQuyen = await Quay(db).XuLyAsync(canh.MaNv, canh.MaSv, null, null, 1, 20, CancellationToken.None);
        var no = await new BienBanChuaNopReader(db).LayDsChuaNopAsync(canh.MaSv, CancellationToken.None);

        Assert.Equal(1, chiPhongA.TongSo);
        Assert.Equal(canh.BienBanA, Assert.Single(chiPhongA.DanhSach).MaBienBan);
        Assert.Equal(1, chiPhongB.TongSo);
        Assert.Equal(canh.BienBanB, Assert.Single(chiPhongB.DanhSach).MaBienBan);
        Assert.Equal(2, caHaiPhong.TongSo);
        Assert.Equal(0, maPhongNgoaiQuyen.TongSo);
        Assert.Empty(maPhongNgoaiQuyen.DanhSach);
        Assert.Equal(0, khongCoQuyen.TongSo);
        Assert.Equal(2, no.Count);
        Assert.Equal(800000L, no.Sum(b => b.TongTien));
    }

    [Fact]
    public async Task UC09_Quay_LocTheoTrangThai_VaTrangThaiLaTraDuLieuKhongHopLe()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var canh = await TaoCanhAsync(db);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE vi_pham.bien_ban_vi_pham SET trang_thai = 'DA_HUY' WHERE ma_bien_ban = {canh.BienBanB}");
        var boXuLy = Quay(db, canh.PhongA, canh.PhongB);

        var chuaNop = await boXuLy.XuLyAsync(canh.MaNv, canh.MaSv, "CHUA_NOP", null, 1, 20, CancellationToken.None);
        var daHuy = await boXuLy.XuLyAsync(canh.MaNv, canh.MaSv, "DA_HUY", null, 1, 20, CancellationToken.None);
        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(canh.MaNv, canh.MaSv, "KHAC", null, 1, 20, CancellationToken.None));

        Assert.Equal(canh.BienBanA, Assert.Single(chuaNop.DanhSach).MaBienBan);
        Assert.Equal(canh.BienBanB, Assert.Single(daHuy.DanhSach).MaBienBan);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    [Fact]
    public async Task UC09_QuanTri_ThayMoiPhong_VaLocTheoMaSv()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var canh = await TaoCanhAsync(db);
        var boXuLy = new DsBienBanViPhamQuanTriHandler(db);

        var cuaSv = await boXuLy.XuLyAsync(canh.MaSv, null, 1, 20, CancellationToken.None);
        var chuaNop = await boXuLy.XuLyAsync($"  {canh.MaSv}  ", "CHUA_NOP", 1, 20, CancellationToken.None);
        var khongCo = await boXuLy.XuLyAsync("KT14-SV-KHONG-CO", null, 1, 20, CancellationToken.None);

        Assert.Equal(2, cuaSv.TongSo);
        Assert.Equal(2, chuaNop.TongSo);
        Assert.Equal(0, khongCo.TongSo);
    }

    [Fact]
    public async Task UC09_Sv_KhongDenNhanKhongCoBienBan_VanNamTrongDanhSachViPham()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var canh = await TaoCanhAsync(db);
        var maYeuCau = $"YC-999999-{canh.Hau}";
        await ViPhamDuLieuThu.TaoYeuCauHetHanAsync(db, maYeuCau, canh.MaSv, canh.PhongA);
        await ViPhamDuLieuThu.ThemViPhamAsync(db, canh.MaSv, "KHONG_DEN_NHAN", new DateTimeOffset(2026, 10, 30, 3, 0, 0, TimeSpan.Zero), null, maYeuCau);

        var viPham = await new DsViPhamSvHandler(db).XuLyAsync(canh.MaSv, 1, 20, CancellationToken.None);
        var bienBan = await new DsBienBanViPhamSvHandler(db).XuLyAsync(canh.MaSv, 1, 20, CancellationToken.None);

        Assert.Equal(4, viPham.TongSo);
        var khongDenNhan = viPham.DanhSach[0];
        Assert.Equal("KHONG_DEN_NHAN", khongDenNhan.LoaiViPham);
        Assert.Null(khongDenNhan.MaBienBan);
        Assert.Null(khongDenNhan.MaDangKy);
        Assert.Equal(maYeuCau, khongDenNhan.MaYeuCau);
        Assert.Equal(2, bienBan.TongSo);
        Assert.DoesNotContain(bienBan.DanhSach, b => b.MaBienBan == khongDenNhan.MaBienBan);
        Assert.Equal(0, (await new DsViPhamSvHandler(db).XuLyAsync("KT14-SV-KHONG-CO", 1, 20, CancellationToken.None)).TongSo);
    }

    [Fact]
    public async Task UC09_PhanTrangKhongHopLe_TraDuLieuKhongHopLe()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();

        var loiTrang = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => new DsBienBanViPhamSvHandler(db).XuLyAsync("KT14-SV", 0, 20, CancellationToken.None));
        var loiKichThuoc = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => new DsViPhamSvHandler(db).XuLyAsync("KT14-SV", 1, 101, CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loiTrang.MaLoi);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loiKichThuoc.MaLoi);
    }

    private static DsBienBanViPhamQuayHandler Quay(AppDbContext db, params Guid[] dsMaPhong) =>
        new(db, new DocPhieuMuonTestSupport.QuyenPhongGia(dsMaPhong));

    private static async Task<Canh> TaoCanhAsync(AppDbContext db)
    {
        var hau = ViPhamDuLieuThu.MaHau();
        var maSv = $"KT14-SV-{hau}";
        var maNv = await ViPhamDuLieuThu.TaoNhanVienAsync(db, hau);
        var phongA = await ViPhamDuLieuThu.TaoPhongAsync(db);
        var phongB = await ViPhamDuLieuThu.TaoPhongAsync(db);
        var maLoai = await ViPhamDuLieuThu.TaoLoaiAsync(db, 50000);
        var maKhung = await ViPhamDuLieuThu.TaoKhungPhatAsync(db, 50);
        await ViPhamDuLieuThu.TaoSinhVienAsync(db, maSv);

        var hanTra = new DateTimeOffset(2026, 10, 10, 9, 0, 0, TimeSpan.Zero);
        var phieuA = $"PM-999991-{hau}";
        var phieuB = $"PM-999992-{hau}";
        var bienBanA = $"BB-999991-{hau}";
        var bienBanB = $"BB-999992-{hau}";
        var thietBiA1 = $"KT14-TB-{hau}-A1";
        var thietBiA2 = $"KT14-TB-{hau}-A2";
        var thietBiB1 = $"KT14-TB-{hau}-B1";

        await ViPhamDuLieuThu.TaoThietBiAsync(db, thietBiA1, maLoai, phongA, 10000000);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, thietBiA2, maLoai, phongA, 10000000);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, thietBiB1, maLoai, phongB, 10000000);
        await ViPhamDuLieuThu.TaoPhieuAsync(db, phieuA, maSv, maNv, hanTra);
        await ViPhamDuLieuThu.TaoPhieuAsync(db, phieuB, maSv, maNv, hanTra);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(db, phieuA, thietBiA1, 50000, null, null, null, null, null);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(db, phieuA, thietBiA2, 50000, null, null, null, null, null);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(db, phieuB, thietBiB1, 50000, null, null, null, null, null);

        await ViPhamDuLieuThu.TaoBienBanAsync(db, bienBanA, phieuA, maNv, 300000, new DateTimeOffset(2026, 10, 20, 3, 0, 0, TimeSpan.Zero));
        await ViPhamDuLieuThu.ThemChiTietBienBanAsync(db, bienBanA, phieuA, thietBiA1, "TRA_TRE", 2, 100000, null);
        await ViPhamDuLieuThu.ThemChiTietBienBanAsync(db, bienBanA, phieuA, thietBiA2, "HU_HONG", null, 200000, maKhung);
        await ViPhamDuLieuThu.TaoBienBanAsync(db, bienBanB, phieuB, maNv, 500000, new DateTimeOffset(2026, 10, 25, 3, 0, 0, TimeSpan.Zero));
        await ViPhamDuLieuThu.ThemChiTietBienBanAsync(db, bienBanB, phieuB, thietBiB1, "MAT", null, 500000, null);

        await ViPhamDuLieuThu.ThemViPhamAsync(db, maSv, "TRA_TRE", new DateTimeOffset(2026, 10, 20, 3, 0, 0, TimeSpan.Zero), bienBanA, null);
        await ViPhamDuLieuThu.ThemViPhamAsync(db, maSv, "HU_HONG", new DateTimeOffset(2026, 10, 20, 3, 0, 1, TimeSpan.Zero), bienBanA, null);
        await ViPhamDuLieuThu.ThemViPhamAsync(db, maSv, "MAT", new DateTimeOffset(2026, 10, 25, 3, 0, 0, TimeSpan.Zero), bienBanB, null);

        return new Canh(hau, maSv, maNv, phongA, phongB, bienBanA, bienBanB, maKhung);
    }

    private sealed record Canh(
        string Hau,
        string MaSv,
        Guid MaNv,
        Guid PhongA,
        Guid PhongB,
        string BienBanA,
        string BienBanB,
        Guid MaKhung);
}

