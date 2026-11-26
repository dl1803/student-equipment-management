using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class DangKyChoTruyVan
{
    private const string TrangThaiDangCho = "DANG_CHO";

    public static async Task<KetQuaPhanTrang<DangKyChoSvDto>> DsTheoSvAsync(
        DbContext db,
        IDanhMucReader danhMuc,
        string maSv,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        var goc = DongDangKy(db).Where(d => d.MaSv == maSv);

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<DangKyChoSvDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await goc
            .OrderByDescending(d => d.ThoiDiemDangKy)
            .ThenBy(d => d.MaDangKy)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        return new KetQuaPhanTrang<DangKyChoSvDto>(tongSo, trang, kichThuoc, await GhepAsync(danhMuc, cacDong, ct));
    }

    public static async Task<DangKyChoSvDto?> LayTheoMaAsync(
        DbContext db,
        IDanhMucReader danhMuc,
        string maDangKy,
        CancellationToken ct)
    {
        var cacDong = await DongDangKy(db).Where(d => d.MaDangKy == maDangKy).ToListAsync(ct);
        if (cacDong.Count == 0)
        {
            return null;
        }

        return (await GhepAsync(danhMuc, cacDong, ct))[0];
    }

    public static Task<DongDangKyCho?> LayDongTheoMaAsync(DbContext db, string maDangKy, CancellationToken ct)
    {
        return DongDangKy(db).Where(d => d.MaDangKy == maDangKy).SingleOrDefaultAsync(ct);
    }

    public static Task<int> DanhSoLaiHangChoAsync(DbContext db, Guid maPhong, Guid maLoai, CancellationToken ct)
    {
        return db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE muon_tra.dang_ky_cho AS d
            SET so_thu_tu_cho = x.so_thu_tu
            FROM (
                SELECT ma_dang_ky, (row_number() OVER (ORDER BY thoi_diem_dang_ky, ma_dang_ky))::integer AS so_thu_tu
                FROM muon_tra.dang_ky_cho
                WHERE ma_phong = {maPhong} AND ma_loai = {maLoai} AND trang_thai = 'DANG_CHO'
            ) AS x
            WHERE d.ma_dang_ky = x.ma_dang_ky AND d.so_thu_tu_cho IS DISTINCT FROM x.so_thu_tu
            """, ct);
    }

    private static IQueryable<DongDangKyCho> DongDangKy(DbContext db)
    {
        return db.Database.SqlQuery<DongDangKyCho>($"""
            SELECT ma_dang_ky AS "MaDangKy", ma_sv AS "MaSv", ma_phong AS "MaPhong", ma_loai AS "MaLoai",
                   thoi_diem_dang_ky AS "ThoiDiemDangKy", so_thu_tu_cho AS "SoThuTuCho",
                   trang_thai AS "TrangThai", han_giu_cho AS "HanGiuCho"
            FROM muon_tra.dang_ky_cho
            """);
    }

    private static async Task<IReadOnlyList<DangKyChoSvDto>> GhepAsync(
        IDanhMucReader danhMuc,
        IReadOnlyList<DongDangKyCho> cacDong,
        CancellationToken ct)
    {
        var tenPhong = new Dictionary<Guid, string>();
        foreach (var maPhong in cacDong.Select(d => d.MaPhong).Distinct())
        {
            tenPhong[maPhong] = (await danhMuc.LayPhongAsync(maPhong, ct))?.TenPhong ?? string.Empty;
        }

        var tenLoai = new Dictionary<Guid, string>();
        foreach (var maLoai in cacDong.Select(d => d.MaLoai).Distinct())
        {
            tenLoai[maLoai] = (await danhMuc.LayLoaiAsync(maLoai, ct))?.TenLoai ?? string.Empty;
        }

        return cacDong
            .Select(d => new DangKyChoSvDto(
                d.MaDangKy,
                d.MaPhong,
                tenPhong[d.MaPhong],
                d.MaLoai,
                tenLoai[d.MaLoai],
                d.ThoiDiemDangKy,
                d.TrangThai == TrangThaiDangCho ? d.SoThuTuCho : null,
                d.TrangThai,
                d.HanGiuCho))
            .ToList();
    }
}
