using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class CapNhatGiaTriThietBiHandler
{
    private readonly DbContext _db;
    private readonly IClock _dongHo;

    public CapNhatGiaTriThietBiHandler(DbContext db, IClock dongHo)
    {
        _db = db;
        _dongHo = dongHo;
    }

    public async Task<GiaTriThietBiDto> XuLyAsync(string ma, GiaTriThietBiRequest yeuCau, CancellationToken ct)
    {
        var lichSu = new LichSuGiaTri(ma, yeuCau.GiaTri, _dongHo.UtcNow);

        var giaoDichNgoai = _db.Database.CurrentTransaction;
        await using var giaoDichRieng = giaoDichNgoai is null ? await _db.Database.BeginTransactionAsync(ct) : null;

        if (!await TonTaiAsync(ma, ct))
        {
            throw KhongTimThay();
        }

        _db.Set<LichSuGiaTri>().Add(lichSu);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            if (!await TonTaiAsync(ma, ct))
            {
                throw KhongTimThay();
            }

            throw;
        }

        if (giaoDichRieng is not null)
        {
            await giaoDichRieng.CommitAsync(ct);
        }

        return new GiaTriThietBiDto(ma, yeuCau.GiaTri, lichSu.NgayCapNhat);
    }

    private Task<bool> TonTaiAsync(string ma, CancellationToken ct) =>
        _db.Set<ThietBi>().AsNoTracking().AnyAsync(t => t.MaThietBi == ma, ct);

    private static LoiNghiepVuException KhongTimThay() =>
        new("KHONG_TIM_THAY", "Không tìm thấy thiết bị.");
}
