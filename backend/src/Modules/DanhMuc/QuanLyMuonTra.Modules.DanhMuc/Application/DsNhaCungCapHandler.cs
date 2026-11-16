using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class DsNhaCungCapHandler
{
    private readonly DbContext _db;

    public DsNhaCungCapHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<KetQuaPhanTrang<NhaCungCapQuanTriDto>> XuLyAsync(int trang, int kichThuoc, CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var goc = _db.Set<NhaCungCap>().AsNoTracking();
        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<NhaCungCapQuanTriDto>(tongSo, trang, kichThuoc);
        }

        var danhSach = await goc
            .OrderBy(n => n.TenNCC)
            .ThenBy(n => n.MaNCC)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .Select(n => new NhaCungCapQuanTriDto(n.MaNCC, n.TenNCC, n.ThongTinLienHe))
            .ToListAsync(ct);

        return new KetQuaPhanTrang<NhaCungCapQuanTriDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
