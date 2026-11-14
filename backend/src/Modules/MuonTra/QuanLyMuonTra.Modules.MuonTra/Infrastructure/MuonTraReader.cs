using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

public sealed class MuonTraReader : IMuonTraReader
{
    private readonly DbContext _db;

    public MuonTraReader(DbContext db)
    {
        _db = db;
    }

    public Task<int> DemThietBiDangCamAsync(string maSv, CancellationToken ct)
    {
        return _db.Set<PhieuMuon>()
            .AsNoTracking()
            .Where(p => p.MaSv == maSv && p.TrangThai != TrangThaiPhieuMuon.DaTra)
            .SelectMany(p => p.ChiTiet)
            .CountAsync(c => c.KetQuaKiemTra == null, ct);
    }
}
