using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

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
        return _db.Database
            .SqlQuery<bool>($"SELECT EXISTS (SELECT 1 FROM quy_dinh.quy_dinh_muon WHERE ma_loai = {maLoai}) AS \"Value\"")
            .SingleAsync(ct);
    }
}
