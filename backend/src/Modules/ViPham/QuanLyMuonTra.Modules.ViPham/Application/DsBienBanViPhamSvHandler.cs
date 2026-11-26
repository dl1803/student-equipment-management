using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed class DsBienBanViPhamSvHandler(DbContext db)
{
    public async Task<KetQuaPhanTrang<BienBanViPhamDto>> XuLyAsync(
        string maSv,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var goc = BienBanTruyVan.TruyVanBienBan(db).Where(d => d.MaSv == maSv);

        return await BienBanTruyVan.LayTrangAsync(db, goc, trang, kichThuoc, ct);
    }
}

