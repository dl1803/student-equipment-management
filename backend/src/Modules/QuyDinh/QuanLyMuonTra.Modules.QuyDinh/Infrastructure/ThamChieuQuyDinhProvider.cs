using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Domain;

namespace QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

public sealed class ThamChieuQuyDinhProvider : IThamChieuDanhMucProvider
{
    private readonly DbContext _db;

    public ThamChieuQuyDinhProvider(DbContext db)
    {
        _db = db;
    }

    public Task<bool> CoThamChieuThietBiAsync(string maThietBi, CancellationToken ct)
    {
        return Task.FromResult(false);
    }

    public Task<bool> CoThamChieuLoaiAsync(Guid maLoai, CancellationToken ct)
    {
        return _db.Set<QuyDinhMuon>().AnyAsync(q => q.MaLoai == maLoai, ct);
    }
}
