using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Domain;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

public sealed class TaoKhungPhatHandler
{
    private readonly DbContext _db;

    public TaoKhungPhatHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<KhungPhatDto> XuLyAsync(KhungPhatRequest yeuCau, CancellationToken ct)
    {
        var khung = new KhungPhat(yeuCau.TenKhung, yeuCau.TyLePhanTram);
        _db.Set<KhungPhat>().Add(khung);
        await _db.SaveChangesAsync(ct);

        return new KhungPhatDto(khung.MaKhung, khung.TenKhung, khung.TyLePhanTram, khung.DangSuDung);
    }
}
