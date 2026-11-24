using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.ViPham.Infrastructure;

public sealed class ThamChieuDanhMucViPhamProvider(DbContext db) : IThamChieuDanhMucProvider
{
    public Task<bool> CoThamChieuThietBiAsync(string maThietBi, CancellationToken ct) =>
        db.Database.SqlQuery<bool>($"""
            SELECT EXISTS (SELECT 1 FROM vi_pham.chi_tiet_bien_ban WHERE ma_thiet_bi = {maThietBi}) AS "Value"
            """).SingleAsync(ct);

    public Task<bool> CoThamChieuLoaiAsync(Guid maLoai, CancellationToken ct) => Task.FromResult(false);
}
