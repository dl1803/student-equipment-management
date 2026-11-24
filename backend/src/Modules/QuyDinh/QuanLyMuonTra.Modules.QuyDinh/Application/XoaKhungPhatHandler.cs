using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

public sealed class XoaKhungPhatHandler
{
    private readonly DbContext _db;
    private readonly IReadOnlyList<IThamChieuKhungPhatProvider> _cacProvider;

    public XoaKhungPhatHandler(DbContext db, IEnumerable<IThamChieuKhungPhatProvider> cacProvider)
    {
        _db = db;
        _cacProvider = cacProvider.ToList();
    }

    public async Task XuLyAsync(Guid ma, CancellationToken ct)
    {
        if (_cacProvider.Count == 0)
        {
            throw new InvalidOperationException(
                "Chưa đăng ký provider tham chiếu khung phạt; từ chối xóa để không xóa nhầm khung đã dùng.");
        }

        var giaoDichNgoai = _db.Database.CurrentTransaction;
        await using var giaoDich = giaoDichNgoai is null ? await _db.Database.BeginTransactionAsync(ct) : null;

        var khung = await _db.Set<KhungPhat>().FirstOrDefaultAsync(k => k.MaKhung == ma, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy khung phạt.");

        foreach (var provider in _cacProvider)
        {
            if (await provider.CoThamChieuKhungAsync(ma, ct))
            {
                throw new LoiNghiepVuException(
                    "KHUNG_PHAT_DA_DUNG_KHONG_XOA",
                    "Khung phạt đã được sử dụng, chỉ được tắt.");
            }
        }

        _db.Set<KhungPhat>().Remove(khung);
        await _db.SaveChangesAsync(ct);

        if (giaoDich is not null)
        {
            await giaoDich.CommitAsync(ct);
        }
    }
}
