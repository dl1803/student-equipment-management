using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.QuyDinh;

public class QuyDinhBaoTriDuLieuThuTests
{
    private const string MaLoaiL1 = "019c0000-0000-7000-8000-0000000d0c01";
    private const string MaLoaiL3 = "019c0000-0000-7000-8000-0000000d0c03";
    private const string MaKhungDaDung = "019c0000-0000-7000-8000-0000000d0d01";
    private const string MaNv = "019c0000-0000-7000-8000-0000000d0a01";
    private const string MaQt = "019c0000-0000-7000-8000-0000000e0a01";

    [CanKetNoiTestFact]
    public async Task UC13_ThamSoHeThong_DongMoiCoMacDinhSrs33VaGioQuayRong()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await ThucThiAsync(gd, "INSERT INTO quy_dinh.tham_so_he_thong (id) VALUES (1) ON CONFLICT (id) DO NOTHING");
        await ThucThiAsync(
            gd,
            "UPDATE quy_dinh.tham_so_he_thong SET so_ngay_khoa = DEFAULT, thoi_gian_giu_cho_phut = DEFAULT, an_han_phut = DEFAULT, so_gio_gia_han_toi_da = DEFAULT, nguong_vi_pham = DEFAULT, so_lan_dang_nhap_sai_toi_da = DEFAULT, gio_mo_cua = DEFAULT, gio_dong_cua = DEFAULT WHERE id = 1");
        await using var db = await TaoContextAsync(gd);

        var thamSo = await db.Set<ThamSoHeThong>().AsNoTracking().SingleAsync();

        Assert.Equal(3, thamSo.SoNgayKhoa);
        Assert.Equal(30, thamSo.ThoiGianGiuChoPhut);
        Assert.Equal(15, thamSo.AnHanPhut);
        Assert.Equal(2, thamSo.SoGioGiaHanToiDa);
        Assert.Equal(3, thamSo.NguongViPham);
        Assert.Equal(5, thamSo.SoLanDangNhapSaiToiDa);
        Assert.Null(thamSo.GioMoCua);
        Assert.Null(thamSo.GioDongCua);
    }

    [CanKetNoiTestFact]
    public async Task UC13_SeedBatBuoc_ChayHaiLanKhongTrungVaDungMacDinh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        var duongDan = Path.Combine(AppContext.BaseDirectory, "quy-dinh-bat-buoc.sql");
        var sql = (await File.ReadAllTextAsync(duongDan))
            .Replace("BEGIN;", string.Empty)
            .Replace("COMMIT;", string.Empty);
        await ThucThiAsync(gd, sql);
        await ThucThiAsync(gd, sql);
        await using var db = await TaoContextAsync(gd);

        var khungPhat = await db.Set<KhungPhat>().AsNoTracking().ToListAsync();
        var quyDinhChung = await db.Set<QuyDinhMuon>().AsNoTracking().Where(q => q.MaLoai == null).ToListAsync();

        Assert.Equal(15m, Assert.Single(khungPhat, k => k.TenKhung == "Nhẹ").TyLePhanTram);
        Assert.Equal(40m, Assert.Single(khungPhat, k => k.TenKhung == "Vừa").TyLePhanTram);
        Assert.Equal(80m, Assert.Single(khungPhat, k => k.TenKhung == "Nặng").TyLePhanTram);
        var daiHoc = Assert.Single(quyDinhChung, q => q.MaNhom == "DAI_HOC");
        Assert.Equal(2, daiHoc.SoThietBiToiDa);
        Assert.Equal(5, daiHoc.ThoiGianMuonToiDaGio);
        var caoHoc = Assert.Single(quyDinhChung, q => q.MaNhom == "CAO_HOC");
        Assert.Equal(3, caoHoc.SoThietBiToiDa);
        Assert.Equal(10, caoHoc.ThoiGianMuonToiDaGio);
        var doAn = Assert.Single(quyDinhChung, q => q.MaNhom == "DO_AN_TOT_NGHIEP");
        Assert.Equal(3, doAn.SoThietBiToiDa);
        Assert.Equal(10, doAn.ThoiGianMuonToiDaGio);
    }

    [CanKetNoiTestFact]
    public async Task UC13_DoiPhiTreVaKhungPhat_KhongLamDoiSoTienDaLuuTrongChungTu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync();

        await ThucThiAsync(gd, $"UPDATE danh_muc.loai_thiet_bi SET phi_tre_moi_ngay = 99000 WHERE ma_loai = '{MaLoaiL1}'");
        await ThucThiAsync(gd, $"UPDATE quy_dinh.khung_phat SET ty_le_phan_tram = 55 WHERE ma_khung = '{MaKhungDaDung}'");

        Assert.Equal(50000m, await DocThapPhanAsync(gd, "SELECT phi_tre_moi_ngay FROM muon_tra.chi_tiet_phieu_muon WHERE ma_chi_tiet = '019c0000-0000-7000-8000-0000000d0e01'"));
        Assert.Equal(20000m, await DocThapPhanAsync(gd, "SELECT so_tien FROM vi_pham.chi_tiet_bien_ban WHERE ma_bien_ban = 'BB-261114-990001' AND ma_thiet_bi = 'D02-TB-07'"));
        Assert.Equal(20000m, await DocThapPhanAsync(gd, "SELECT tong_tien FROM vi_pham.bien_ban_vi_pham WHERE ma_bien_ban = 'BB-261114-990001'"));
        Assert.Equal(99000m, await DocThapPhanAsync(gd, $"SELECT phi_tre_moi_ngay FROM danh_muc.loai_thiet_bi WHERE ma_loai = '{MaLoaiL1}'"));
    }

    [CanKetNoiTestFact]
    public async Task UC12_ThamChieuQuyDinh_LoaiCoNgoaiLeThiTrueConLoaiKhacVaThietBiThiFalse()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync();
        await ThucThiAsync(
            gd,
            $"INSERT INTO quy_dinh.quy_dinh_muon (ma_quy_dinh, ma_nhom, ma_loai, so_thiet_bi_toi_da, thoi_gian_muon_toi_da_gio) VALUES ('019c0000-0000-7000-8000-0000000e0b01', 'DAI_HOC', '{MaLoaiL1}', NULL, 3)");
        await using var db = await TaoContextAsync(gd);
        var provider = new ThamChieuQuyDinhProvider(db);
        var ct = CancellationToken.None;

        Assert.True(await provider.CoThamChieuLoaiAsync(Guid.Parse(MaLoaiL1), ct));
        Assert.False(await provider.CoThamChieuLoaiAsync(Guid.Parse(MaLoaiL3), ct));
        Assert.False(await provider.CoThamChieuLoaiAsync(Guid.NewGuid(), ct));
        Assert.False(await provider.CoThamChieuThietBiAsync("D02-TB-01", ct));
    }

    [CanKetNoiTestFact]
    public async Task UC12_ThamChieuBaoTri_BangRongTraFalseTuDbThat_CoBanGhiBaoTriHoacThanhLyThiTrue()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync();
        var provider = new ThamChieuBaoTriProvider(gd.Db);
        var ct = CancellationToken.None;

        Assert.False(await provider.CoThamChieuThietBiAsync("D02-TB-01", ct));
        Assert.False(await provider.CoThamChieuThietBiAsync("D02-TB-02", ct));

        await ThucThiAsync(
            gd,
            $"INSERT INTO bao_tri.bao_tri_sua_chua (ma_bao_tri, ma_thiet_bi, ma_nv_thuc_hien, loai_thuc_hien) VALUES ('019c0000-0000-7000-8000-0000000e0c01', 'D02-TB-01', '{MaNv}', 'BAO_TRI_DINH_KY')");
        await ThucThiAsync(
            gd,
            $"INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten) VALUES ('{MaQt}', 'd03-qt', 'khong-dung-de-dang-nhap', 'Quản trị D03')");
        await ThucThiAsync(gd, $"INSERT INTO tai_khoan.quan_tri_vien (ma_nguoi_dung, ma_qt) VALUES ('{MaQt}', 'D03-QT')");
        await ThucThiAsync(
            gd,
            $"INSERT INTO bao_tri.quyet_dinh_thanh_ly (ma_quyet_dinh, ma_qt_lap, ngay_thanh_ly) VALUES ('QD-261115-990001', '{MaQt}', DATE '2026-11-15')");
        await ThucThiAsync(
            gd,
            "INSERT INTO bao_tri.chi_tiet_thanh_ly (ma_quyet_dinh, ma_thiet_bi) VALUES ('QD-261115-990001', 'D02-TB-02')");

        Assert.True(await provider.CoThamChieuThietBiAsync("D02-TB-01", ct));
        Assert.True(await provider.CoThamChieuThietBiAsync("D02-TB-02", ct));
        Assert.False(await provider.CoThamChieuThietBiAsync("D02-TB-08", ct));
        Assert.False(await provider.CoThamChieuThietBiAsync("KHONG-TON-TAI", ct));
        Assert.False(await provider.CoThamChieuLoaiAsync(Guid.Parse(MaLoaiL1), ct));
    }

    private static async Task ThucThiAsync(GiaoDichTestRollback gd, string sql)
    {
        await using var lenh = new NpgsqlCommand(sql, gd.KetNoi, gd.GiaoDich);
        await lenh.ExecuteNonQueryAsync();
    }

    private static async Task<decimal> DocThapPhanAsync(GiaoDichTestRollback gd, string sql)
    {
        await using var lenh = new NpgsqlCommand(sql, gd.KetNoi, gd.GiaoDich);
        return (decimal)(await lenh.ExecuteScalarAsync())!;
    }

    private static async Task<QuyDinhTestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        var options = new DbContextOptionsBuilder<QuyDinhTestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new QuyDinhTestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich);
        return db;
    }

    public sealed class QuyDinhTestContext(DbContextOptions<QuyDinhTestContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyQuyDinhConfigurations();
        }
    }
}
