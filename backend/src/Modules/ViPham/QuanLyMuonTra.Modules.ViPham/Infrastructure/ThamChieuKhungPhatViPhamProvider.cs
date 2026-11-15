using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;

namespace QuanLyMuonTra.Modules.ViPham.Infrastructure;

public sealed class ThamChieuKhungPhatViPhamProvider(AppDbContext db) : IThamChieuKhungPhatProvider
{
    public Task<bool> CoThamChieuKhungAsync(Guid maKhung, CancellationToken ct) =>
        db.Database.SqlQuery<bool>($"""
            SELECT EXISTS (SELECT 1 FROM vi_pham.chi_tiet_bien_ban WHERE ma_khung = {maKhung}) AS "Value"
            """).SingleAsync(ct);
}

