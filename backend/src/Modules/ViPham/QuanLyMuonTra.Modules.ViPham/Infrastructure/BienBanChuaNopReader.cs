using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.Modules.SinhVien.Contracts;

namespace QuanLyMuonTra.Modules.ViPham.Infrastructure;

public sealed class BienBanChuaNopReader(AppDbContext db) : IBienBanChuaNopReader
{
    public async Task<IReadOnlyList<BienBanChuaNopDto>> LayDsChuaNopAsync(string maSv, CancellationToken ct)
    {
        var cacDong = await db.Database.SqlQuery<DongBienBan>($"""
                SELECT bb.ma_bien_ban AS "MaBienBan", bb.tong_tien::bigint AS "TongTien", bb.ngay_lap AS "NgayLap"
                FROM vi_pham.bien_ban_vi_pham bb
                WHERE bb.trang_thai = 'CHUA_NOP'
                  AND EXISTS (
                      SELECT 1 FROM vi_pham.vi_pham vp
                      WHERE vp.ma_bien_ban = bb.ma_bien_ban AND vp.ma_sv = {maSv})
                """)
            .OrderBy(dong => dong.NgayLap)
            .ThenBy(dong => dong.MaBienBan)
            .ToListAsync(ct);

        return cacDong.Select(dong => new BienBanChuaNopDto(dong.MaBienBan, dong.TongTien, dong.NgayLap)).ToList();
    }

    public sealed class DongBienBan
    {
        public string MaBienBan { get; set; } = string.Empty;

        public long TongTien { get; set; }

        public DateTimeOffset NgayLap { get; set; }
    }
}

