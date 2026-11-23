using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class BienBanPhieuTruyVan
{
    public static async Task<IReadOnlyList<BienBanViPhamDto>> LayTheoPhieuAsync(
        DbContext db,
        string maPhieu,
        string maSv,
        CancellationToken ct)
    {
        var dsBienBan = await db.Database.SqlQuery<DongBienBanPhieu>($"""
                SELECT ma_bien_ban AS "MaBienBan", ma_phieu_muon AS "MaPhieuMuon", ngay_lap AS "NgayLap",
                       tong_tien::bigint AS "TongTien", trang_thai AS "TrangThai", thoi_diem_nop AS "ThoiDiemNop"
                FROM vi_pham.bien_ban_vi_pham
                """)
            .Where(b => b.MaPhieuMuon == maPhieu)
            .OrderBy(b => b.NgayLap)
            .ThenBy(b => b.MaBienBan)
            .ToListAsync(ct);
        if (dsBienBan.Count == 0)
        {
            return [];
        }

        var dsMaBienBan = dsBienBan.Select(b => b.MaBienBan).ToList();
        var dsChiTiet = await db.Database.SqlQuery<DongChiTietBienBan>($"""
                SELECT ma_bien_ban AS "MaBienBan", ma_thiet_bi AS "MaThietBi", loai_vi_pham AS "LoaiViPham",
                       so_ngay_tre AS "SoNgayTre", so_tien::bigint AS "SoTien", ma_khung AS "MaKhung"
                FROM vi_pham.chi_tiet_bien_ban
                """)
            .Where(c => dsMaBienBan.Contains(c.MaBienBan))
            .OrderBy(c => c.MaThietBi)
            .ThenBy(c => c.LoaiViPham)
            .ToListAsync(ct);

        return dsBienBan
            .Select(b => new BienBanViPhamDto(
                b.MaBienBan,
                b.MaPhieuMuon,
                maSv,
                b.NgayLap,
                b.TongTien,
                b.TrangThai,
                b.ThoiDiemNop,
                dsChiTiet
                    .Where(c => c.MaBienBan == b.MaBienBan)
                    .Select(c => new ChiTietBienBanDto(c.MaThietBi, c.LoaiViPham, c.SoNgayTre, c.SoTien, c.MaKhung))
                    .ToList()))
            .ToList();
    }
}
