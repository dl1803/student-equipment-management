using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class YeuCauMuonSvTests
{
    private const string FileDuLieu = "D06-V-du-lieu-thu.sql";
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d6b01");
    private static readonly Guid X = Guid.Parse("019c0000-0000-7000-8000-0000000d6c01");
    private static readonly Guid Y = Guid.Parse("019c0000-0000-7000-8000-0000000d6c02");
    private static readonly Guid W = Guid.Parse("019c0000-0000-7000-8000-0000000d6c03");
    private static readonly Guid Z = Guid.Parse("019c0000-0000-7000-8000-0000000d6c04");
    private static readonly DateTimeOffset BayGio = new(2026, 11, 18, 3, 0, 0, TimeSpan.Zero);
    private static readonly CancellationToken Ct = CancellationToken.None;

    public sealed class YeuCauMuonTestContext(DbContextOptions<YeuCauMuonTestContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyMuonTraConfiguration();
            modelBuilder.ApplyDanhMucConfigurations();
            modelBuilder.ApplyQuyDinhConfigurations();
        }
    }

    private sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }

    private sealed class BoXuLy(YeuCauMuonTestContext db, DateTimeOffset bayGio)
    {
        private readonly DongHoCoDinh _dongHo = new(bayGio);

        public GuiYeuCauMuonHandler Gui => new(
            db,
            _dongHo,
            new DanhMucReader(db),
            new QuyDinhReader(db),
            new MuonTraReader(db),
            new AvailabilityService(db, _dongHo));

        public DsPhieuYeuCauMuonSvHandler Ds => new(db, new DanhMucReader(db));

        public ChiTietPhieuYeuCauMuonSvHandler ChiTiet => new(db, new DanhMucReader(db));
    }

    private static async Task<YeuCauMuonTestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        await gd.NapDuLieuThuAsync(FileDuLieu);
        var options = new DbContextOptionsBuilder<YeuCauMuonTestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new YeuCauMuonTestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich, Ct);
        return db;
    }

    private static IReadOnlyList<LoaiVaSoLuongYeuCau> Dong(Guid maLoai, int soLuong)
    {
        return [new LoaiVaSoLuongYeuCau(maLoai, soLuong)];
    }

    private static Task<int> DemPhieuAsync(DbContext db, string maSv)
    {
        return db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_yeu_cau_muon WHERE ma_sv = {maSv}
            """).SingleAsync();
    }

    [CanKetNoiTestFact]
    public async Task UC02_GuiHopLe_TaoPhieuChoDuyetKhongGiuChoVaKhongGanThietBi()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var phieu = await bo.Gui.XuLyAsync("D06-SV-A", P1, 2, Dong(X, 2), Ct);

        Assert.Matches(@"^YC-[0-9]{6}-[0-9]{6,}$", phieu.MaYeuCau);
        Assert.Equal("D06-SV-A", phieu.MaSV);
        Assert.Equal("CHO_DUYET", phieu.TrangThai);
        Assert.Equal(BayGio, phieu.ThoiGianGui);
        Assert.Equal(2, phieu.ThoiGianMuonDuKienGio);
        Assert.Equal("D06 Phong P1", phieu.TenPhong);
        Assert.Null(phieu.HanGiuCho);
        Assert.Null(phieu.ThoiDiemDuyet);
        Assert.Null(phieu.LyDoTuChoi);
        var dong = Assert.Single(phieu.DsLoaiVaSoLuong);
        Assert.Equal(X, dong.MaLoai);
        Assert.Equal("D06 Loai X", dong.TenLoai);
        Assert.Equal(2, dong.SoLuong);

        Assert.Equal(3, await new AvailabilityService(db, new DongHoCoDinh(BayGio)).TinhKhaDungAsync(P1, X, Ct));
        var soThietBiDangDuocMuon = await db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM danh_muc.thiet_bi
            WHERE ma_loai = {X} AND tinh_trang = 'DANG_DUOC_MUON'
            """).SingleAsync(Ct);
        Assert.Equal(1, soThietBiDangDuocMuon);
    }

    [CanKetNoiTestFact]
    public async Task UC02_NhieuLoaiTrongMotPhieu_GhiDuCacDongChiTiet()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var phieu = await bo.Gui.XuLyAsync(
            "D06-SV-A",
            P1,
            2,
            [new LoaiVaSoLuongYeuCau(X, 1), new LoaiVaSoLuongYeuCau(W, 1)],
            Ct);

        Assert.Equal(new[] { "D06 Loai W", "D06 Loai X" }, phieu.DsLoaiVaSoLuong.Select(d => d.TenLoai).ToArray());
        Assert.All(phieu.DsLoaiVaSoLuong, d => Assert.Equal(1, d.SoLuong));
    }

    [CanKetNoiTestFact]
    public async Task UC02_GuiXong_DocLaiDanhSachVaChiTietThayDungPhieuMoiNhatTruoc()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var phieuCu = await new BoXuLy(db, BayGio).Gui.XuLyAsync("D06-SV-A", P1, 2, Dong(X, 1), Ct);
        var boMoi = new BoXuLy(db, BayGio.AddMinutes(10));
        var phieuMoi = await boMoi.Gui.XuLyAsync("D06-SV-A", P1, 2, Dong(X, 1), Ct);

        var trang1 = await boMoi.Ds.XuLyAsync("D06-SV-A", 1, 1, Ct);
        var trang2 = await boMoi.Ds.XuLyAsync("D06-SV-A", 2, 1, Ct);
        var trang3 = await boMoi.Ds.XuLyAsync("D06-SV-A", 3, 1, Ct);
        var chiTiet = await boMoi.ChiTiet.XuLyAsync("D06-SV-A", phieuMoi.MaYeuCau, Ct);

        Assert.Equal(2, trang1.TongSo);
        Assert.Equal(phieuMoi.MaYeuCau, Assert.Single(trang1.DanhSach).MaYeuCau);
        Assert.Equal(phieuCu.MaYeuCau, Assert.Single(trang2.DanhSach).MaYeuCau);
        Assert.Equal(2, trang3.TongSo);
        Assert.Empty(trang3.DanhSach);
        Assert.Equal(phieuMoi.MaYeuCau, chiTiet.MaYeuCau);
        Assert.Equal(phieuMoi.DsLoaiVaSoLuong, chiTiet.DsLoaiVaSoLuong);
    }

    [CanKetNoiTestFact]
    public async Task UC02E2_KhaDungKhongDu_Tra409VaKhongTaoPhieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var loiY = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-A", P1, 2, Dong(Y, 2), Ct));
        var loiW = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-A", P1, 2, Dong(W, 2), Ct));

        Assert.Equal(MaLoi.SoLuongKhaDungThayDoi, loiY.MaLoi);
        Assert.Equal(409, loiY.MaHttp);
        Assert.Equal(MaLoi.SoLuongKhaDungThayDoi, loiW.MaLoi);
        Assert.Equal(0, await DemPhieuAsync(db, "D06-SV-A"));

        var phieuConDu = await bo.Gui.XuLyAsync("D06-SV-A", P1, 2, Dong(W, 1), Ct);
        Assert.Equal("CHO_DUYET", phieuConDu.TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task UC02A2_VuotGioiHanTongTheoNhom_Tra422VaKhongTaoPhieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var loiMotLoai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-C", P1, 2, Dong(X, 2), Ct));
        var loiNhieuLoai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-C", P1, 2, [new LoaiVaSoLuongYeuCau(X, 1), new LoaiVaSoLuongYeuCau(W, 1)], Ct));

        Assert.Equal(MaLoi.VuotGioiHanThietBi, loiMotLoai.MaLoi);
        Assert.Equal(422, loiMotLoai.MaHttp);
        Assert.Equal(MaLoi.VuotGioiHanThietBi, loiNhieuLoai.MaLoi);
        Assert.Equal(0, await DemPhieuAsync(db, "D06-SV-C"));

        var phieuDu = await bo.Gui.XuLyAsync("D06-SV-C", P1, 2, Dong(X, 1), Ct);
        Assert.Equal("CHO_DUYET", phieuDu.TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task UC02A2_GioiHanTinhTrenTongKhongPhanBietLoai_HaiLoaiMoiLoaiMotThanhCong()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var phieu = await bo.Gui.XuLyAsync(
            "D06-SV-A",
            P1,
            2,
            [new LoaiVaSoLuongYeuCau(X, 1), new LoaiVaSoLuongYeuCau(W, 1)],
            Ct);

        Assert.Equal(2, phieu.DsLoaiVaSoLuong.Sum(d => d.SoLuong));
    }

    [CanKetNoiTestFact]
    public async Task UC02A3_VuotThoiGianMuonToiDa_Tra422VaKhongTaoPhieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var loiNhom = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-A", P1, 6, Dong(X, 1), Ct));
        var loiNgoaiLe = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-A", P1, 3, Dong(Y, 1), Ct));
        var loiNhoNhat = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-A", P1, 3, [new LoaiVaSoLuongYeuCau(X, 1), new LoaiVaSoLuongYeuCau(Y, 1)], Ct));

        Assert.Equal(MaLoi.VuotThoiGianMuonToiDa, loiNhom.MaLoi);
        Assert.Equal(422, loiNhom.MaHttp);
        Assert.Contains("5 giờ", loiNhom.Message);
        Assert.Equal(MaLoi.VuotThoiGianMuonToiDa, loiNgoaiLe.MaLoi);
        Assert.Contains("2 giờ", loiNgoaiLe.Message);
        Assert.Equal(MaLoi.VuotThoiGianMuonToiDa, loiNhoNhat.MaLoi);
        Assert.Equal(0, await DemPhieuAsync(db, "D06-SV-A"));

        var phieuDung = await bo.Gui.XuLyAsync("D06-SV-A", P1, 2, Dong(Y, 1), Ct);
        Assert.Equal("CHO_DUYET", phieuDung.TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task UC02E1_SinhVienKhongDuDieuKien_Tra422TheoTungLyDoVaKhongTaoPhieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);
        (string MaSv, string MaLoiMongDoi)[] cacCa =
        [
            ("D06-SV-HET-HAN", MaLoi.SvTheHetHan),
            ("D06-SV-TOT-NGHIEP", MaLoi.SvKhongConTheoHoc),
            ("D06-SV-TAM-KHOA", MaLoi.SvBiKhoaQuyenMuon)
        ];

        foreach (var (maSv, maLoiMongDoi) in cacCa)
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
                () => bo.Gui.XuLyAsync(maSv, P1, 2, Dong(X, 1), Ct));

            Assert.Equal(maLoiMongDoi, loi.MaLoi);
            Assert.Equal(422, loi.MaHttp);
            Assert.Equal(0, await DemPhieuAsync(db, maSv));
        }

        var phieu = await bo.Gui.XuLyAsync("D06-SV-HOM-NAY", P1, 2, Dong(X, 1), Ct);
        Assert.Equal("CHO_DUYET", phieu.TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task UC02_PhongLoaiKhongHopLeHoacSvKhongCoHoSo_TraLoiVaKhongTaoPhieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var loiPhong = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-A", Guid.NewGuid(), 2, Dong(X, 1), Ct));
        var loiLoaiKhongCo = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-A", P1, 2, Dong(Guid.NewGuid(), 1), Ct));
        var loiLoaiNgung = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-A", P1, 2, Dong(Z, 1), Ct));
        var loiKhongCoHoSo = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Gui.XuLyAsync("D06-SV-KHONG-CO", P1, 2, Dong(X, 1), Ct));

        Assert.Equal(MaLoi.DuLieuKhongHopLe, loiPhong.MaLoi);
        Assert.Equal(400, loiPhong.MaHttp);
        Assert.Equal(MaLoi.DuLieuKhongHopLe, loiLoaiKhongCo.MaLoi);
        Assert.Equal(MaLoi.DuLieuKhongHopLe, loiLoaiNgung.MaLoi);
        Assert.Equal(MaLoi.KhongDuQuyen, loiKhongCoHoSo.MaLoi);
        Assert.Equal(403, loiKhongCoHoSo.MaHttp);
        Assert.Equal(0, await DemPhieuAsync(db, "D06-SV-A"));
    }

    [CanKetNoiTestFact]
    public async Task BR06_DocChiTietPhieuCuaNguoiKhac_Tra403CungPhieuCuaMinhDocDuoc()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.ChiTiet.XuLyAsync("D06-SV-A", "YC-261118-996001", Ct));
        var cuaMinh = await bo.ChiTiet.XuLyAsync("D06-SV-B", "YC-261118-996001", Ct);
        var dsCuaA = await bo.Ds.XuLyAsync("D06-SV-A", 1, 20, Ct);
        var dsCuaB = await bo.Ds.XuLyAsync("D06-SV-B", 1, 20, Ct);

        Assert.Equal(MaLoi.KhongDuQuyen, loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
        Assert.Equal("DA_DUYET", cuaMinh.TrangThai);
        Assert.NotNull(cuaMinh.HanGiuCho);
        var dong = Assert.Single(cuaMinh.DsLoaiVaSoLuong);
        Assert.Equal(W, dong.MaLoai);
        Assert.Equal(2, dong.SoLuong);
        Assert.Equal(0, dsCuaA.TongSo);
        Assert.Equal(1, dsCuaB.TongSo);
    }

    [CanKetNoiTestFact]
    public async Task UC02_ChiTietMaKhongTonTaiHoacSaiDinhDang_Tra404Va400()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var khongTonTai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.ChiTiet.XuLyAsync("D06-SV-A", "YC-261118-999999", Ct));
        var saiDinhDang = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.ChiTiet.XuLyAsync("D06-SV-A", "khong-phai-ma", Ct));
        var trangSai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Ds.XuLyAsync("D06-SV-A", 0, 20, Ct));

        Assert.Equal(MaLoi.KhongTimThay, khongTonTai.MaLoi);
        Assert.Equal(404, khongTonTai.MaHttp);
        Assert.Equal(MaLoi.DuLieuKhongHopLe, saiDinhDang.MaLoi);
        Assert.Equal(400, saiDinhDang.MaHttp);
        Assert.Equal(MaLoi.DuLieuKhongHopLe, trangSai.MaLoi);
    }
}
