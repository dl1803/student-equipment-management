using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class DangKyChoSvTests
{
    private const string FileDuLieu = "D12-V-du-lieu-thu.sql";
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d12b1");
    private static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d12b2");
    private static readonly Guid X = Guid.Parse("019c0000-0000-7000-8000-0000000d12c1");
    private static readonly Guid Y = Guid.Parse("019c0000-0000-7000-8000-0000000d12c2");
    private static readonly Guid W = Guid.Parse("019c0000-0000-7000-8000-0000000d12c3");
    private static readonly Guid Z = Guid.Parse("019c0000-0000-7000-8000-0000000d12c4");
    private static readonly DateTimeOffset BayGio = new(2026, 11, 24, 3, 0, 0, TimeSpan.Zero);
    private static readonly CancellationToken Ct = CancellationToken.None;

    public sealed class DangKyChoTestContext(DbContextOptions<DangKyChoTestContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyMuonTraConfiguration();
            modelBuilder.ApplyDanhMucConfigurations();
            modelBuilder.ApplyQuyDinhConfigurations();
        }
    }

    public sealed class DongTrangThai
    {
        public string TrangThai { get; set; } = string.Empty;

        public int? SoThuTuCho { get; set; }

        public DateTimeOffset? HanGiuCho { get; set; }
    }

    private sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }

    private sealed class BoXuLy(DangKyChoTestContext db, DateTimeOffset bayGio)
    {
        private DongHoCoDinh DongHo => new(bayGio);

        public DangKyChoHandler DangKy => new(
            db,
            DongHo,
            new DanhMucReader(db),
            new AvailabilityService(db, DongHo),
            new AdvisoryLockService(db));

        public DsDangKyChoSvHandler Ds => new(db, new DanhMucReader(db));
    }

    private static async Task<DangKyChoTestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        await gd.NapDuLieuThuAsync(FileDuLieu);
        var options = new DbContextOptionsBuilder<DangKyChoTestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new DangKyChoTestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich, Ct);
        return db;
    }

    private static Task<DongTrangThai> DocAsync(DbContext db, string maDangKy)
    {
        return db.Database.SqlQuery<DongTrangThai>($"""
            SELECT trang_thai AS "TrangThai", so_thu_tu_cho AS "SoThuTuCho", han_giu_cho AS "HanGiuCho"
            FROM muon_tra.dang_ky_cho
            WHERE ma_dang_ky = {maDangKy}
            """).SingleAsync(Ct);
    }

    private static Task<int> DemDangKyAsync(DbContext db, string maSv)
    {
        return db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.dang_ky_cho WHERE ma_sv = {maSv}
            """).SingleAsync(Ct);
    }

    [CanKetNoiTestFact]
    public async Task UC03A1_DsCuaSv_SapXepMoiNhatTruocPhanTrangOnDinhVaChiHienSoThuTuKhiDangCho()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var trang1 = await bo.Ds.XuLyAsync("D12-SV-E", 1, 1, Ct);
        var trang2 = await bo.Ds.XuLyAsync("D12-SV-E", 2, 1, Ct);
        var trang3 = await bo.Ds.XuLyAsync("D12-SV-E", 3, 1, Ct);

        Assert.Equal(2, trang1.TongSo);
        var dau = Assert.Single(trang1.DanhSach);
        Assert.Equal("DK-261124-912005", dau.MaDangKy);
        Assert.Equal("DA_THONG_BAO", dau.TrangThai);
        Assert.Null(dau.SoThuTuCho);
        Assert.Equal(new DateTimeOffset(2026, 11, 24, 3, 10, 0, TimeSpan.Zero), dau.HanGiuCho);
        Assert.Equal("D12 Phong P1", dau.TenPhong);
        Assert.Equal("D12 Loai W", dau.TenLoai);
        Assert.Equal("DK-261124-912008", Assert.Single(trang2.DanhSach).MaDangKy);
        Assert.Equal(2, trang3.TongSo);
        Assert.Empty(trang3.DanhSach);
    }

    [CanKetNoiTestFact]
    public async Task UC03A1_DsCuaSv_DangChoCoSoThuTuVaChiThayDangKyCuaMinh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var ketQua = await bo.Ds.XuLyAsync("D12-SV-B", 1, 20, Ct);
        var rong = await bo.Ds.XuLyAsync("D12-SV-G-KHONG-CO", 1, 20, Ct);

        var dong = Assert.Single(ketQua.DanhSach);
        Assert.Equal("DK-261124-912002", dong.MaDangKy);
        Assert.Equal("DANG_CHO", dong.TrangThai);
        Assert.Equal(2, dong.SoThuTuCho);
        Assert.Null(dong.HanGiuCho);
        Assert.Equal(0, rong.TongSo);
        Assert.Empty(rong.DanhSach);
    }

    [CanKetNoiTestFact]
    public async Task UC03A1_DsCuaSvBiKhoaQuyenMuon_VanDocDuoc()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var ketQua = await bo.Ds.XuLyAsync("D12-SV-TAM-KHOA", 1, 20, Ct);

        Assert.Equal("DK-261124-912009", Assert.Single(ketQua.DanhSach).MaDangKy);
    }

    [CanKetNoiTestFact]
    public async Task UC03A1_DsThamSoPhanTrangSai_Tra400()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => bo.Ds.XuLyAsync("D12-SV-A", 0, 20, Ct));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC03_DangKyKhiHetHang_ThemVaoCuoiHangChoVaGhiSoThuTu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var dangKy = await bo.DangKy.XuLyAsync("D12-SV-D", P1, X, Ct);

        Assert.Matches(@"^DK-[0-9]{6}-[0-9]{6,}$", dangKy.MaDangKy);
        Assert.Equal("DANG_CHO", dangKy.TrangThai);
        Assert.Equal(4, dangKy.SoThuTuCho);
        Assert.Equal(BayGio, dangKy.ThoiDiemDangKy);
        Assert.Null(dangKy.HanGiuCho);
        Assert.Equal("D12 Phong P1", dangKy.TenPhong);
        Assert.Equal("D12 Loai X", dangKy.TenLoai);
        var dong = await DocAsync(db, dangKy.MaDangKy);
        Assert.Equal("DANG_CHO", dong.TrangThai);
        Assert.Equal(4, dong.SoThuTuCho);
    }

    [CanKetNoiTestFact]
    public async Task UC03_HangChoTheoTungPhongVaLoai_DocLapNhau()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var tuongTranh = await bo.DangKy.XuLyAsync("D12-SV-D", P2, X, Ct);
        var tiepTheo = await bo.DangKy.XuLyAsync("D12-SV-F", P2, X, Ct);

        Assert.Equal(1, tuongTranh.SoThuTuCho);
        Assert.Equal(2, tiepTheo.SoThuTuCho);
    }

    [CanKetNoiTestFact]
    public async Task UC03E1_ConHang_Tra422VaKhongGhiDangKy()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);
        var truoc = await DemDangKyAsync(db, "D12-SV-D");

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => bo.DangKy.XuLyAsync("D12-SV-D", P1, Y, Ct));

        Assert.Equal("CON_HANG_KHONG_DANG_KY_CHO", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
        Assert.Equal(truoc, await DemDangKyAsync(db, "D12-SV-D"));
    }

    [CanKetNoiTestFact]
    public async Task UC03_GiuChoDaThongBaoConHan_ChanKhaDungVaHetHanThiTraVeConHang()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);

        var trongHan = await new BoXuLy(db, BayGio).DangKy.XuLyAsync("D12-SV-D", P1, W, Ct);
        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => new BoXuLy(db, BayGio.AddMinutes(20)).DangKy.XuLyAsync("D12-SV-A", P1, W, Ct));

        Assert.Equal(3, trongHan.SoThuTuCho);
        Assert.Equal("CON_HANG_KHONG_DANG_KY_CHO", loi.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC03_TamKhoaHoacCamDaiHan_Tra422VaKhongGhiDangKy()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        foreach (var maSv in new[] { "D12-SV-TAM-KHOA", "D12-SV-CAM" })
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => bo.DangKy.XuLyAsync(maSv, P2, X, Ct));

            Assert.Equal("SV_BI_KHOA_QUYEN_MUON", loi.MaLoi);
            Assert.Equal(422, loi.MaHttp);
            Assert.Equal(1, await DemDangKyAsync(db, maSv));
        }
    }

    [CanKetNoiTestFact]
    public async Task UC03_TheHetHanHoacKhongTheoHoc_KhongBiChanBoiDangKyCho()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);

        var theHetHan = await bo.DangKy.XuLyAsync("D12-SV-HET-HAN", P2, X, Ct);
        var totNghiep = await bo.DangKy.XuLyAsync("D12-SV-TOT-NGHIEP", P2, X, Ct);

        Assert.Equal("DANG_CHO", theHetHan.TrangThai);
        Assert.Equal("DANG_CHO", totNghiep.TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task UC03_PhongHoacLoaiKhongHopLe_Tra400VaKhongGhiDangKy()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, BayGio);
        var cacCap = new (Guid MaPhong, Guid MaLoai)[] { (Guid.NewGuid(), X), (P1, Guid.NewGuid()), (P1, Z) };

        foreach (var (maPhong, maLoai) in cacCap)
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => bo.DangKy.XuLyAsync("D12-SV-D", maPhong, maLoai, Ct));

            Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(400, loi.MaHttp);
        }

        Assert.Equal(1, await DemDangKyAsync(db, "D12-SV-D"));
    }
}
