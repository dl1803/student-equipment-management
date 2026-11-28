using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using static QuanLyMuonTra.IntegrationTests.MuonTra.BanGiaoTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public sealed class DongKetQuaChiTiet
{
    public string? KetQua { get; set; }

    public DateTimeOffset? NgayGioTra { get; set; }

    public Guid? MaNv { get; set; }

    public Guid? MaKhung { get; set; }

    public string? TinhTrangKhiTra { get; set; }
}

internal static class GhiKetQuaKiemTraTestSupport
{
    public const string T2 = "D14-TB-T2";
    public const string T3 = "D14-TB-T3";
    public const string T4 = "D14-TB-T4";

    public static readonly Guid Khung = Guid.Parse("019c0000-0000-7000-8000-0000000d14d1");
    public static readonly DateTimeOffset BayGio = new(2026, 11, 26, 3, 0, 0, TimeSpan.Zero);
    public static readonly byte[] AnhJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];
    public static readonly CancellationToken Ct = CancellationToken.None;

    public const string QuyenP1P2 = """
        INSERT INTO tai_khoan.nhan_vien_phong (ma_nguoi_dung, ma_phong)
        VALUES ('019c0000-0000-7000-8000-0000000d14a1', '019c0000-0000-7000-8000-0000000d14b1'),
               ('019c0000-0000-7000-8000-0000000d14a1', '019c0000-0000-7000-8000-0000000d14b2');
        """;

    public const string QuyenChiP1 = """
        INSERT INTO tai_khoan.nhan_vien_phong (ma_nguoi_dung, ma_phong)
        VALUES ('019c0000-0000-7000-8000-0000000d14a1', '019c0000-0000-7000-8000-0000000d14b1');
        """;

    public const string KhongQuyen = "SELECT 1;";

    public const string XoaD14 = """
        DELETE FROM thong_bao.thong_bao WHERE ma_sv_nhan LIKE 'D14-SV-%';
        DELETE FROM muon_tra.dang_ky_cho WHERE ma_dang_ky LIKE 'DK-261126-915%';
        DELETE FROM muon_tra.chi_tiet_phieu_muon WHERE ma_thiet_bi LIKE 'D14-TB-%';
        DELETE FROM muon_tra.phieu_muon WHERE ma_sv LIKE 'D14-SV-%';
        DELETE FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi LIKE 'D14-TB-%';
        DELETE FROM danh_muc.thiet_bi WHERE ma_thiet_bi LIKE 'D14-TB-%';
        DELETE FROM quy_dinh.khung_phat WHERE ma_khung = '019c0000-0000-7000-8000-0000000d14d1';
        DELETE FROM danh_muc.loai_thiet_bi WHERE ma_loai IN ('019c0000-0000-7000-8000-0000000d14c1', '019c0000-0000-7000-8000-0000000d14c2');
        DELETE FROM tai_khoan.nhan_vien_phong WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000d14a1', '019c0000-0000-7000-8000-0000000d14a2');
        DELETE FROM danh_muc.phong_thiet_bi WHERE ma_phong IN ('019c0000-0000-7000-8000-0000000d14b1', '019c0000-0000-7000-8000-0000000d14b2');
        DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000d14a1', '019c0000-0000-7000-8000-0000000d14a2');
        DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000d14a1', '019c0000-0000-7000-8000-0000000d14a2');
        DELETE FROM sinh_vien.sinh_vien WHERE ma_sv LIKE 'D14-SV-%';
        DELETE FROM sinh_vien.nhom_sinh_vien WHERE ma_nhom = 'D14-NHOM';
        """;

    public static async Task<MoiTruongTichHopMuonTra> TaoMoiTruongHoanTacAsync(string sqlQuyen)
    {
        var moi = await MoiTruongTichHopMuonTra.TaoAsync(Nv1, BayGio, true);
        await moi.NapDuLieuAsync(FileDuLieu, sqlQuyen);
        return moi;
    }

    public static KetQuaKiemTraYeuCau NguyenVen(string maThietBi) =>
        new(maThietBi, KetQuaKiemTra.NguyenVen, null, null, []);

    public static KetQuaKiemTraYeuCau HuHong(string maThietBi, IReadOnlyList<Guid>? dsMaAnh = null) =>
        new(maThietBi, KetQuaKiemTra.HuHong, "Vỡ màn hình", Khung, dsMaAnh ?? []);

    public static KetQuaKiemTraYeuCau ThieuPhuKien(string maThietBi) =>
        new(maThietBi, KetQuaKiemTra.ThieuPhuKien, "Mất sạc", Khung, []);

    public static KetQuaKiemTraYeuCau Mat(string maThietBi) =>
        new(maThietBi, KetQuaKiemTra.Mat, null, null, []);

    public static Task<DongKetQuaChiTiet> DocChiTietAsync(DbContext db, string maThietBi) =>
        db.Database.SqlQuery<DongKetQuaChiTiet>($"""
            SELECT ket_qua_kiem_tra AS "KetQua", ngay_gio_tra_thuc_te AS "NgayGioTra", ma_nv_nhan_tra AS "MaNv",
                   ma_khung AS "MaKhung", tinh_trang_khi_tra AS "TinhTrangKhiTra"
            FROM muon_tra.chi_tiet_phieu_muon
            WHERE ma_thiet_bi = {maThietBi}
            """).SingleAsync();

    public static Task<string> DocTinhTrangThietBiAsync(DbContext db, string maThietBi) =>
        db.Database.SqlQuery<string>($"""
            SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {maThietBi}
            """).SingleAsync();

    public static Task<string> DocTrangThaiPhieuAsync(DbContext db, string maPhieu) =>
        db.Database.SqlQuery<string>($"""
            SELECT trang_thai AS "Value" FROM muon_tra.phieu_muon WHERE ma_phieu = {maPhieu}
            """).SingleAsync();

    public static Task<int> DemAsync(DbContext db, FormattableString cauLenh) =>
        db.Database.SqlQuery<int>(cauLenh).SingleAsync();
}
