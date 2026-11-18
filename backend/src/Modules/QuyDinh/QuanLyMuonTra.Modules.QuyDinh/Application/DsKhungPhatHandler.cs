using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Domain;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

public sealed class DsKhungPhatHandler
{
    private readonly DbContext _db;

    public DsKhungPhatHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<KetQuaPhanTrang<KhungPhatDto>> XuLyAsync(
        bool chiDangSuDung,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        IQueryable<KhungPhat> goc = _db.Set<KhungPhat>().AsNoTracking();
        if (chiDangSuDung)
        {
            goc = goc.Where(k => k.DangSuDung);
        }

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<KhungPhatDto>(tongSo, trang, kichThuoc);
        }

        var danhSach = await goc
            .OrderBy(k => k.TyLePhanTram)
            .ThenBy(k => k.MaKhung)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .Select(k => new KhungPhatDto(k.MaKhung, k.TenKhung, k.TyLePhanTram, k.DangSuDung))
            .ToListAsync(ct);

        return new KetQuaPhanTrang<KhungPhatDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
