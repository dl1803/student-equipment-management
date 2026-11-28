using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed class HuyBienBanHandler(DbContext db, XetMoKhoaQuyenMuon xetMoKhoa, IAuditLogger nhatKy)
{
    public async Task<BienBanViPhamDto> XuLyAsync(string maBienBan, CancellationToken ct)
    {
        KiemTraDocViPham.KiemTraMaBienBan(maBienBan);

        var maSv = (await BienBanTruyVan.LayDongAsync(db, maBienBan, ct)).MaSv;

        return await GiaoDichViPham.ChayAsync(db, async () =>
        {
            await GiaoDichViPham.KhoaDongSinhVienAsync(db, maSv, ct);

            var truoc = await BienBanTruyVan.LayDongAsync(db, maBienBan, ct);
            var soDong = await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE vi_pham.bien_ban_vi_pham
                SET trang_thai = 'DA_HUY'
                WHERE ma_bien_ban = {maBienBan} AND trang_thai IN ('CHUA_NOP', 'DA_NOP')
                """, ct);
            if (soDong == 0)
            {
                throw new LoiNghiepVuException(
                    MaLoi.TrangThaiKhongHopLe,
                    $"Biên bản {maBienBan} đang ở trạng thái {truoc.TrangThai}, không thể hủy.");
            }

            await nhatKy.GhiAsync(
                "HUY_BIEN_BAN_VI_PHAM",
                "vi_pham.bien_ban_vi_pham",
                maBienBan,
                new Dictionary<string, object?>
                {
                    ["TrangThai"] = truoc.TrangThai,
                    ["TongTien"] = truoc.TongTien,
                    ["ThoiDiemNop"] = truoc.ThoiDiemNop
                },
                new Dictionary<string, object?> { ["TrangThai"] = "DA_HUY" },
                ct);

            await xetMoKhoa.XetAsync(maSv, ct);

            return await BienBanTruyVan.LayMotAsync(db, maBienBan, ct);
        }, ct);
    }
}

