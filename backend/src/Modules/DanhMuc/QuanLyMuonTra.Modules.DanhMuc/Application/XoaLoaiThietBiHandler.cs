using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class XoaLoaiThietBiHandler
{
    private readonly DbContext _db;
    private readonly IReadOnlyList<IThamChieuDanhMucProvider> _cacProvider;

    public XoaLoaiThietBiHandler(DbContext db, IEnumerable<IThamChieuDanhMucProvider> cacProvider)
    {
        _db = db;
        _cacProvider = cacProvider.ToList();
    }

    public async Task XuLyAsync(Guid ma, CancellationToken ct)
    {
        if (_cacProvider.Count == 0)
        {
            throw new InvalidOperationException(
                "Chưa đăng ký provider tham chiếu danh mục; từ chối xóa để không xóa nhầm loại thiết bị đã có lịch sử.");
        }

        var giaoDichNgoai = _db.Database.CurrentTransaction;
        await using var giaoDichRieng = giaoDichNgoai is null ? await _db.Database.BeginTransactionAsync(ct) : null;

        var loai = await _db.Set<LoaiThietBi>().FirstOrDefaultAsync(l => l.MaLoai == ma, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy loại thiết bị.");

        if (await _db.Set<ThietBi>().AsNoTracking().AnyAsync(t => t.MaLoai == ma, ct))
        {
            throw ConLichSu();
        }

        foreach (var provider in _cacProvider)
        {
            if (await provider.CoThamChieuLoaiAsync(ma, ct))
            {
                throw ConLichSu();
            }
        }

        _db.Set<LoaiThietBi>().Remove(loai);
        await _db.SaveChangesAsync(ct);

        if (giaoDichRieng is not null)
        {
            await giaoDichRieng.CommitAsync(ct);
        }
    }

    private static LoiNghiepVuException ConLichSu() =>
        new("DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG", "Loại thiết bị đã có lịch sử hoặc còn đối tượng phụ thuộc, chỉ được ngưng sử dụng.");
}
