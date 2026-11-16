using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class DsPhongThietBiHandler
{
    private readonly DbContext _db;

    public DsPhongThietBiHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<KetQuaPhanTrang<PhongThietBiDto>> XuLyAsync(int trang, int kichThuoc, CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var goc = _db.Set<PhongThietBi>().AsNoTracking();
        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<PhongThietBiDto>(tongSo, trang, kichThuoc);
        }

        var danhSach = await goc
            .OrderBy(p => p.TenPhong)
            .ThenBy(p => p.MaPhong)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .Select(p => new PhongThietBiDto(p.MaPhong, p.TenPhong, p.CoSo))
            .ToListAsync(ct);

        return new KetQuaPhanTrang<PhongThietBiDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
