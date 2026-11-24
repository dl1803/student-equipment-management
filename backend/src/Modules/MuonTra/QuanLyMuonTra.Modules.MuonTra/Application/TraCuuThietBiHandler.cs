using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class TraCuuThietBiHandler(DbContext db, IClock dongHo)
{
    public async Task<KetQuaPhanTrang<TraCuuThietBiItemDto>> XuLyAsync(
        string? ten,
        Guid? maLoai,
        Guid? maPhong,
        bool? conHang,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var goc = TraCuuTruyVan.ApDungBoLoc(
            TraCuuTruyVan.LayCapLoaiPhong(db, dongHo.UtcNow.ToUniversalTime()),
            ten,
            maLoai,
            maPhong,
            conHang);

        return await TraCuuTruyVan.LayTrangAsync(goc, trang, kichThuoc, ct);
    }
}
