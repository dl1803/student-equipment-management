using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.BaoTri.Infrastructure;

public sealed class ThamChieuBaoTriProvider : IThamChieuDanhMucProvider
{
    private readonly DbContext _db;

    public ThamChieuBaoTriProvider(DbContext db)
    {
        _db = db;
    }

    public Task<bool> CoThamChieuThietBiAsync(string maThietBi, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maThietBi))
        {
            return Task.FromResult(false);
        }

        var maChuanHoa = maThietBi.Trim();
        return _db.Database
            .SqlQuery<bool>($"SELECT (EXISTS (SELECT 1 FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi = {maChuanHoa}) OR EXISTS (SELECT 1 FROM bao_tri.chi_tiet_thanh_ly WHERE ma_thiet_bi = {maChuanHoa})) AS \"Value\"")
            .SingleAsync(ct);
    }

    public Task<bool> CoThamChieuLoaiAsync(Guid maLoai, CancellationToken ct)
    {
        return Task.FromResult(false);
    }
}
