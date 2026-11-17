using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class DangXuatHandler(DbContext db)
{
    public async Task XuLyAsync(string refreshToken, CancellationToken ct)
    {
        var maBam = RefreshToken.Bam(refreshToken);

        var ma = await db.Set<RefreshToken>()
            .AsNoTracking()
            .Where(x => x.MaBam == maBam)
            .Select(x => (Guid?)x.Ma)
            .FirstOrDefaultAsync(ct)
            ?? throw new LoiNghiepVuException("CHUA_XAC_THUC", "Refresh token không hợp lệ.");

        await ThuHoiChuoiRefreshToken.ThucHienAsync(db, ma, ct);
    }
}

