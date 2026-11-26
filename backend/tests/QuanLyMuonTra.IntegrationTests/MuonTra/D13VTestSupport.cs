using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.IntegrationTests.ThongBao;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public sealed class DongDangKyKiemTra
{
    public string TrangThai { get; set; } = string.Empty;

    public int? SoThuTuCho { get; set; }

    public DateTimeOffset? HanGiuCho { get; set; }
}

internal static class DuLieuD13
{
    public const string TenTep = "D13-V-du-lieu-thu.sql";
    public const string MaSvF = "D13-SV-F";
    public const string MaSvG = "D13-SV-G";
    public const string MaSvH = "D13-SV-H";
    public const string Dk001 = "DK-261125-913001";
    public const string Dk002 = "DK-261125-913002";
    public const string Dk003 = "DK-261125-913003";
    public const string Dk004 = "DK-261125-913004";
    public const string Dk005 = "DK-261125-913005";
    public const string Dk006 = "DK-261125-913006";
    public const string Dk007 = "DK-261125-913007";
    public const string Dk008 = "DK-261125-913008";
    public const string Dk009 = "DK-261125-913009";
    public static readonly Guid Nv1 = Guid.Parse("019c0000-0000-7000-8000-0000000d13a1");
    public static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d13b1");
    public static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d13b2");
    public static readonly Guid LoaiX = Guid.Parse("019c0000-0000-7000-8000-0000000d13c1");
    public static readonly Guid LoaiY = Guid.Parse("019c0000-0000-7000-8000-0000000d13c2");
    public static readonly Guid LoaiW = Guid.Parse("019c0000-0000-7000-8000-0000000d13c3");
    public static readonly DateTimeOffset BayGio = new(2026, 11, 25, 3, 0, 0, TimeSpan.Zero);

    public const string QuyenNv1P1 = """
        INSERT INTO tai_khoan.nhan_vien_phong (ma_nguoi_dung, ma_phong)
        VALUES ('019c0000-0000-7000-8000-0000000d13a1', '019c0000-0000-7000-8000-0000000d13b1');
        """;

    public const string QuyenNv1P2 = """
        INSERT INTO tai_khoan.nhan_vien_phong (ma_nguoi_dung, ma_phong)
        VALUES ('019c0000-0000-7000-8000-0000000d13a1', '019c0000-0000-7000-8000-0000000d13b2');
        """;

    public const string XoaTatCa = """
        DELETE FROM thong_bao.thong_bao WHERE ma_sv_nhan LIKE 'D13-SV-%';
        DELETE FROM muon_tra.chi_tiet_phieu_muon WHERE ma_thiet_bi LIKE 'D13-TB-%';
        DELETE FROM muon_tra.phieu_muon WHERE ma_sv LIKE 'D13-SV-%';
        DELETE FROM muon_tra.dang_ky_cho WHERE ma_dang_ky LIKE 'DK-261125-913%';
        DELETE FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi LIKE 'D13-TB-%';
        DELETE FROM danh_muc.thiet_bi WHERE ma_thiet_bi LIKE 'D13-TB-%';
        DELETE FROM danh_muc.loai_thiet_bi WHERE ma_loai IN ('019c0000-0000-7000-8000-0000000d13c1', '019c0000-0000-7000-8000-0000000d13c2', '019c0000-0000-7000-8000-0000000d13c3');
        DELETE FROM tai_khoan.nhan_vien_phong WHERE ma_nguoi_dung = '019c0000-0000-7000-8000-0000000d13a1';
        DELETE FROM danh_muc.phong_thiet_bi WHERE ma_phong IN ('019c0000-0000-7000-8000-0000000d13b1', '019c0000-0000-7000-8000-0000000d13b2');
        DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung = '019c0000-0000-7000-8000-0000000d13a1';
        DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = '019c0000-0000-7000-8000-0000000d13a1';
        DELETE FROM sinh_vien.sinh_vien WHERE ma_sv LIKE 'D13-SV-%';
        DELETE FROM quy_dinh.quy_dinh_muon WHERE ma_nhom = 'D13-NHOM';
        DELETE FROM sinh_vien.nhom_sinh_vien WHERE ma_nhom = 'D13-NHOM';
        """;

    public static Task<MoiTruongTichHopMuonTra> TaoMoiTruongAsync(bool hoanTacCuoiTest, Action<IServiceCollection>? tuyChinh = null) =>
        MoiTruongTichHopMuonTra.TaoAsync(Nv1, BayGio, hoanTacCuoiTest, tuyChinh);

    public static Task<DongDangKyKiemTra> DocAsync(DbContext db, string maDangKy) =>
        db.Database.SqlQuery<DongDangKyKiemTra>($"""
            SELECT trang_thai AS "TrangThai", so_thu_tu_cho AS "SoThuTuCho", han_giu_cho AS "HanGiuCho"
            FROM muon_tra.dang_ky_cho
            WHERE ma_dang_ky = {maDangKy}
            """).SingleAsync();

    public static Task<string> TinhTrangThietBiAsync(DbContext db, string maThietBi) =>
        db.Database.SqlQuery<string>($"""
            SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {maThietBi}
            """).SingleAsync();

    public static Task<int> DemAsync(DbContext db, FormattableString cauLenh) =>
        db.Database.SqlQuery<int>(cauLenh).SingleAsync();

    public static Task<int> DemThongBaoSvAsync(DbContext db) =>
        DemAsync(db, $"""
            SELECT count(*)::integer AS "Value" FROM thong_bao.thong_bao WHERE ma_sv_nhan LIKE 'D13-SV-%'
            """);

    public static Task<int> DemViPhamAsync(DbContext db) =>
        DemAsync(db, $"""
            SELECT ((SELECT count(*) FROM vi_pham.vi_pham) + (SELECT count(*) FROM vi_pham.bien_ban_vi_pham))::integer AS "Value"
            """);

    public static Task<List<DongThongBaoKiemTra>> ThongBaoTheoKhoaAsync(DbContext db, string khoa) =>
        db.Database.SqlQuery<DongThongBaoKiemTra>($"""
            SELECT kenh_gui AS "KenhGui", trang_thai_gui AS "TrangThaiGui", noi_dung AS "NoiDung",
                   ma_sv_nhan AS "MaSvNhan", ma_nv_nhan AS "MaNvNhan", so_lan_thu AS "SoLanThu"
            FROM thong_bao.thong_bao
            WHERE khoa_trung_lap = {khoa}
            """).OrderBy(d => d.KenhGui).ToListAsync();
}
