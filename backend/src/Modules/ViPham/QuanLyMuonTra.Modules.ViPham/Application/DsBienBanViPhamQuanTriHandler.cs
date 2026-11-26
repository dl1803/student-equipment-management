using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed class DsBienBanViPhamQuanTriHandler(DbContext db)
{
    public async Task<KetQuaPhanTrang<BienBanViPhamDto>> XuLyAsync(
        string? maSv,
        string? trangThai,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);
        KiemTraDocViPham.KiemTraTrangThaiBienBan(trangThai);

        var goc = BienBanTruyVan.TruyVanBienBan(db);
        var maSvChuan = maSv?.Trim();
        if (!string.IsNullOrEmpty(maSvChuan))
        {
            goc = goc.Where(d => d.MaSv == maSvChuan);
        }

        if (trangThai is not null)
        {
            goc = goc.Where(d => d.TrangThai == trangThai);
        }

        return await BienBanTruyVan.LayTrangAsync(db, goc, trang, kichThuoc, ct);
    }
}

