using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

public sealed class QuyenPhongService(DbContext db) : IQuyenPhongService
{
    public async Task<IReadOnlyCollection<Guid>> LayDsMaPhongAsync(Guid maNguoiDung, CancellationToken ct) =>
        await db.Set<NhanVienPhong>()
            .AsNoTracking()
            .Where(x => x.MaNguoiDung == maNguoiDung)
            .Select(x => x.MaPhong)
            .ToListAsync(ct);

    public async Task EnsureAsync(Guid maNguoiDung, Guid maPhong, CancellationToken ct)
    {
        var coQuyen = await db.Set<NhanVienPhong>()
            .AsNoTracking()
            .AnyAsync(x => x.MaNguoiDung == maNguoiDung && x.MaPhong == maPhong, ct);

        if (!coQuyen)
        {
            throw new LoiNghiepVuException("PHONG_CHUA_CAP_QUYEN", "Nhân viên chưa được cấp quyền trên phòng này.");
        }
    }
}

