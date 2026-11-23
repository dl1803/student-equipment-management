using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.SinhVien;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public static class DongBoSinhVienTestSupport
{
    public static readonly DateTimeOffset MocGoc = new(2100, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi())
            .AddSinhVienModule()
            .BuildServiceProvider();

    public static string TaoMa(string tienTo) => $"{tienTo}-{Guid.NewGuid():N}";

    public static Task DatMocAsync(AppDbContext db, DateTimeOffset? moc) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO sinh_vien.dong_bo_trang_thai (id, moc_dong_bo_gan_nhat) VALUES (1, {moc}::timestamptz)
            ON CONFLICT (id) DO UPDATE SET moc_dong_bo_gan_nhat = EXCLUDED.moc_dong_bo_gan_nhat
            """);

    public static Task<int> DemMocAsync(AppDbContext db, DateTimeOffset? moc) =>
        db.Database.SqlQuery<int>($"""
            SELECT count(*)::int AS "Value" FROM sinh_vien.dong_bo_trang_thai
            WHERE id = 1 AND moc_dong_bo_gan_nhat IS NOT DISTINCT FROM {moc}::timestamptz
            """).SingleAsync();

    public static Task ThemNhomTruongAsync(AppDbContext db, string maNhom, string tenNhom) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO truong_gia_lap.nhom_sinh_vien_truong (ma_nhom, ten_nhom) VALUES ({maNhom}, {tenNhom})
            """);

    public static Task ThemSvTruongAsync(
        AppDbContext db,
        string maSv,
        string hoTen,
        DateTimeOffset capNhatLuc,
        string maNhom = "DAI_HOC") =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO truong_gia_lap.sinh_vien_truong
                (ma_sv, mat_khau_bam, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky, cap_nhat_luc)
            VALUES
                ({maSv}, 'bam-kiem-thu-d11', {hoTen}, 'KTPM2024', 'CNPM', 'd11k@truong.local', {maNhom},
                 DATE '2027-12-31', 'DANG_HOC', 'HK1-2026', {capNhatLuc})
            """);

    public static Task ThemSvUngDungAsync(AppDbContext db, string maSv, string hoTen, string trangThaiQuyenMuon) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO sinh_vien.sinh_vien
                (ma_sv, ho_ten, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky, trang_thai_quyen_muon)
            VALUES
                ({maSv}, {hoTen}, 'cu@kiemthu.local', 'DAI_HOC', DATE '2020-01-01', 'DANG_HOC', 'HK-CU', {trangThaiQuyenMuon})
            """);

    public static Task<DongSinhVienUngDung?> DocSvUngDungAsync(AppDbContext db, string maSv) =>
        db.Database.SqlQuery<DongSinhVienUngDung>($"""
                SELECT ho_ten AS "HoTen", ma_nhom AS "MaNhom", han_the::text AS "HanThe", hoc_ky AS "HocKy",
                       trang_thai_quyen_muon AS "TrangThaiQuyenMuon"
                FROM sinh_vien.sinh_vien
                WHERE ma_sv = {maSv}
                """)
            .SingleOrDefaultAsync();

    public sealed class DongSinhVienUngDung
    {
        public string HoTen { get; set; } = string.Empty;

        public string MaNhom { get; set; } = string.Empty;

        public string HanThe { get; set; } = string.Empty;

        public string HocKy { get; set; } = string.Empty;

        public string TrangThaiQuyenMuon { get; set; } = string.Empty;
    }
}

