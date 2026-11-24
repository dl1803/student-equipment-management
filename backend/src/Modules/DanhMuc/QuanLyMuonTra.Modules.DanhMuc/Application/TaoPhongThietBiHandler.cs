using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class TaoPhongThietBiHandler
{
    private readonly DbContext _db;

    public TaoPhongThietBiHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<PhongThietBiDto> XuLyAsync(PhongThietBiRequest yeuCau, CancellationToken ct)
    {
        var phong = new PhongThietBi(yeuCau.TenPhong, yeuCau.CoSo);
        _db.Set<PhongThietBi>().Add(phong);
        await _db.SaveChangesAsync(ct);

        return new PhongThietBiDto(phong.MaPhong, phong.TenPhong, phong.CoSo);
    }
}
