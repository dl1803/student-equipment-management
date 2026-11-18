using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class PhieuYeuCauMuonTruyVan
{
    public static async Task<KetQuaPhanTrang<PhieuYeuCauMuonDto>> DsTheoSvAsync(
        DbContext db,
        IDanhMucReader danhMuc,
        string maSv,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        var goc = DongPhieu(db).Where(d => d.MaSv == maSv);

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<PhieuYeuCauMuonDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await goc
            .OrderByDescending(d => d.ThoiGianGui)
            .ThenBy(d => d.MaYeuCau)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        var danhSach = await GhepAsync(db, danhMuc, cacDong, ct);

        return new KetQuaPhanTrang<PhieuYeuCauMuonDto>(tongSo, trang, kichThuoc, danhSach);
    }

    public static async Task<PhieuYeuCauMuonDto?> LayTheoMaAsync(
        DbContext db,
        IDanhMucReader danhMuc,
        string maYeuCau,
        CancellationToken ct)
    {
        var cacDong = await DongPhieu(db).Where(d => d.MaYeuCau == maYeuCau).ToListAsync(ct);
        if (cacDong.Count == 0)
        {
            return null;
        }

        var danhSach = await GhepAsync(db, danhMuc, cacDong, ct);

        return danhSach[0];
    }

    private static IQueryable<DongPhieuYeuCauMuon> DongPhieu(DbContext db)
    {
        return db.Database.SqlQuery<DongPhieuYeuCauMuon>($"""
            SELECT ma_yeu_cau AS "MaYeuCau", ma_sv AS "MaSv", ma_phong AS "MaPhong", thoi_gian_gui AS "ThoiGianGui",
                   thoi_gian_muon_du_kien_gio AS "ThoiGianMuonDuKienGio", trang_thai AS "TrangThai",
                   ly_do_tu_choi AS "LyDoTuChoi", thoi_diem_duyet AS "ThoiDiemDuyet", han_giu_cho AS "HanGiuCho"
            FROM muon_tra.phieu_yeu_cau_muon
            """);
    }

    private static IQueryable<DongChiTietYeuCauMuon> DongChiTiet(DbContext db)
    {
        return db.Database.SqlQuery<DongChiTietYeuCauMuon>($"""
            SELECT ma_yeu_cau AS "MaYeuCau", ma_loai AS "MaLoai", so_luong AS "SoLuong"
            FROM muon_tra.chi_tiet_yeu_cau_muon
            """);
    }

    private static async Task<IReadOnlyList<PhieuYeuCauMuonDto>> GhepAsync(
        DbContext db,
        IDanhMucReader danhMuc,
        IReadOnlyList<DongPhieuYeuCauMuon> cacPhieu,
        CancellationToken ct)
    {
        var dsMa = cacPhieu.Select(p => p.MaYeuCau).ToList();
        var cacChiTiet = await DongChiTiet(db).Where(c => dsMa.Contains(c.MaYeuCau)).ToListAsync(ct);

        var tenPhong = new Dictionary<Guid, string>();
        foreach (var maPhong in cacPhieu.Select(p => p.MaPhong).Distinct())
        {
            tenPhong[maPhong] = (await danhMuc.LayPhongAsync(maPhong, ct))?.TenPhong ?? string.Empty;
        }

        var tenLoai = new Dictionary<Guid, string>();
        foreach (var maLoai in cacChiTiet.Select(c => c.MaLoai).Distinct())
        {
            tenLoai[maLoai] = (await danhMuc.LayLoaiAsync(maLoai, ct))?.TenLoai ?? string.Empty;
        }

        return cacPhieu
            .Select(p => new PhieuYeuCauMuonDto(
                p.MaYeuCau,
                p.MaSv,
                p.MaPhong,
                tenPhong[p.MaPhong],
                p.ThoiGianGui,
                p.ThoiGianMuonDuKienGio,
                p.TrangThai,
                p.LyDoTuChoi,
                p.ThoiDiemDuyet,
                p.HanGiuCho,
                cacChiTiet
                    .Where(c => c.MaYeuCau == p.MaYeuCau)
                    .Select(c => new LoaiVaSoLuongDto(c.MaLoai, tenLoai[c.MaLoai], c.SoLuong))
                    .OrderBy(d => d.TenLoai, StringComparer.Ordinal)
                    .ThenBy(d => d.MaLoai)
                    .ToList()))
            .ToList();
    }
}
