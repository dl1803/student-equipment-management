using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

public sealed class ThamChieuMuonTraProvider : IThamChieuMuonTraProvider, IThamChieuDanhMucProvider, IThamChieuKhungPhatProvider
{
    private readonly DbContext _db;

    public ThamChieuMuonTraProvider(DbContext db)
    {
        _db = db;
    }

    public async Task<bool> CoThamChieuPhongAsync(Guid maPhong, CancellationToken ct)
    {
        if (await _db.Set<PhieuYeuCauMuon>().AnyAsync(p => p.MaPhong == maPhong, ct))
        {
            return true;
        }

        return await _db.Set<DangKyCho>().AnyAsync(d => d.MaPhong == maPhong, ct);
    }

    public async Task<bool> CoThamChieuLoaiAsync(Guid maLoai, CancellationToken ct)
    {
        if (await _db.Set<ChiTietYeuCauMuon>().AnyAsync(c => c.MaLoai == maLoai, ct))
        {
            return true;
        }

        return await _db.Set<DangKyCho>().AnyAsync(d => d.MaLoai == maLoai, ct);
    }

    public Task<bool> CoThamChieuThietBiAsync(string maThietBi, CancellationToken ct)
    {
        return _db.Set<ChiTietPhieuMuon>().AnyAsync(c => c.MaThietBi == maThietBi, ct);
    }

    public Task<bool> CoThamChieuKhungPhatAsync(Guid maKhung, CancellationToken ct)
    {
        return _db.Set<ChiTietPhieuMuon>().AnyAsync(c => c.MaKhung == maKhung, ct);
    }

    public Task<bool> CoThamChieuKhungAsync(Guid maKhung, CancellationToken ct)
    {
        return CoThamChieuKhungPhatAsync(maKhung, ct);
    }
}
