using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DsPhieuMuonSvHandler(DbContext db)
{
    public async Task<KetQuaPhanTrang<PhieuMuonTomTatDto>> XuLyAsync(string maSv, int trang, int kichThuoc, CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var goc = PhieuMuonTruyVan.DongPhieu(db).Where(d => d.MaSv == maSv);
        var (tongSo, dong) = await PhieuMuonTruyVan.LayTrangAsync(
            goc,
            q => q.OrderByDescending(d => d.NgayGioMuon).ThenBy(d => d.MaPhieu),
            trang,
            kichThuoc,
            ct);

        return new KetQuaPhanTrang<PhieuMuonTomTatDto>(
            tongSo,
            trang,
            kichThuoc,
            dong.Select(PhieuMuonTruyVan.ToTomTat).ToList());
    }
}
