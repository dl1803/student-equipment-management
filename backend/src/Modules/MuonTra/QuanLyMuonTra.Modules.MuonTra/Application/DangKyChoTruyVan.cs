using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class DangKyChoTruyVan
{
    private const string TrangThaiDangCho = "DANG_CHO";
    private const string TrangThaiDaThongBao = "DA_THONG_BAO";

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

    public static async Task<KetQuaPhanTrang<DangKyChoQuayDto>> DsTheoPhongAsync(
        DbContext db,
        IDanhMucReader danhMuc,
        List<Guid> dsMaPhong,
        Guid? maLoai,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        var goc = DongDangKy(db).Where(d => dsMaPhong.Contains(d.MaPhong)
            && (d.TrangThai == TrangThaiDangCho || d.TrangThai == TrangThaiDaThongBao));
        if (maLoai is not null)
        {
            var maLoaiLoc = maLoai.Value;
            goc = goc.Where(d => d.MaLoai == maLoaiLoc);
        }

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<DangKyChoQuayDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await goc
            .OrderBy(d => d.ThoiDiemDangKy)
            .ThenBy(d => d.MaDangKy)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        return new KetQuaPhanTrang<DangKyChoQuayDto>(tongSo, trang, kichThuoc, await GhepQuayAsync(db, danhMuc, cacDong, ct));
    }

    public static async Task<DangKyChoQuayDto?> LayQuayTheoMaAsync(
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

        return (await GhepQuayAsync(db, danhMuc, cacDong, ct))[0];
    }

    private static async Task<IReadOnlyList<DangKyChoQuayDto>> GhepQuayAsync(
        DbContext db,
        IDanhMucReader danhMuc,
        IReadOnlyList<DongDangKyCho> cacDong,
        CancellationToken ct)
    {
        var danhSach = await GhepAsync(danhMuc, cacDong, ct);
        var dsMaSv = cacDong.Select(d => d.MaSv).Distinct().ToList();
        var cacSv = await db.Database.SqlQuery<DongSinhVienTomTat>($"""
                SELECT ma_sv AS "MaSv", ho_ten AS "HoTen", lop AS "Lop"
                FROM sinh_vien.sinh_vien
                """)
            .Where(s => dsMaSv.Contains(s.MaSv))
            .ToListAsync(ct);
        var theoMa = cacSv.ToDictionary(s => s.MaSv);

        return danhSach
            .Select((d, i) =>
            {
                var maSv = cacDong[i].MaSv;
                return new DangKyChoQuayDto(
                    d.MaDangKy,
                    d.MaPhong,
                    d.TenPhong,
                    d.MaLoai,
                    d.TenLoai,
                    d.ThoiDiemDangKy,
                    d.SoThuTuCho,
                    d.TrangThai,
                    d.HanGiuCho,
                    theoMa.TryGetValue(maSv, out var sv)
                        ? new SinhVienTomTatDto(sv.MaSv, sv.HoTen, sv.Lop)
                        : new SinhVienTomTatDto(maSv, string.Empty, null));
            })
            .ToList();
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
