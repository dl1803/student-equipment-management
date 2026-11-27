using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DongBanGiao
{
    public Guid MaChiTiet { get; set; }

    public string MaPhieu { get; set; } = string.Empty;

    public string MaThietBi { get; set; } = string.Empty;

    public string TinhTrangBanDau { get; set; } = string.Empty;

    public string MaSv { get; set; } = string.Empty;

    public DateTimeOffset NgayGioMuon { get; set; }

    public DateTimeOffset NgayGioHenTra { get; set; }
}

internal static class BanGiaoTruyVan
{
    public static Task<DongBanGiao?> TimChiTietDangHieuLucAsync(DbContext db, string maThietBi, CancellationToken ct)
    {
        return db.Database.SqlQuery<DongBanGiao>($"""
                SELECT c.ma_chi_tiet AS "MaChiTiet", c.ma_phieu AS "MaPhieu", c.ma_thiet_bi AS "MaThietBi",
                       c.tinh_trang_ban_dau AS "TinhTrangBanDau", p.ma_sv AS "MaSv",
                       p.ngay_gio_muon AS "NgayGioMuon", p.ngay_gio_hen_tra AS "NgayGioHenTra"
                FROM muon_tra.chi_tiet_phieu_muon c
                JOIN muon_tra.phieu_muon p ON p.ma_phieu = c.ma_phieu
                WHERE c.ket_qua_kiem_tra IS NULL
                """)
            .Where(d => d.MaThietBi == maThietBi)
            .SingleOrDefaultAsync(ct);
    }
}
