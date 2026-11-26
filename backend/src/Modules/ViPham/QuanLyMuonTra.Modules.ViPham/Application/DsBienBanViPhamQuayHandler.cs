using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed class DsBienBanViPhamQuayHandler(DbContext db, IQuyenPhongService quyenPhong)
{
    public async Task<KetQuaPhanTrang<BienBanViPhamDto>> XuLyAsync(
        Guid maNv,
        string? maSv,
        string? trangThai,
        Guid? maPhong,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);
        KiemTraDocViPham.KiemTraTrangThaiBienBan(trangThai);

        var dsMaPhong = (await quyenPhong.LayDsMaPhongAsync(maNv, ct)).ToList();
        if (maPhong is not null)
        {
            dsMaPhong = dsMaPhong.Where(p => p == maPhong.Value).ToList();
        }

        if (dsMaPhong.Count == 0)
        {
            return PhanTrang.Rong<BienBanViPhamDto>(0, trang, kichThuoc);
        }

        var goc = BienBanTruyVan.TruyVanBienBanTheoPhong(db, dsMaPhong.ToArray());
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

