using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

public sealed class TrangThaiTaiKhoanReader(DbContext db) : ITrangThaiTaiKhoanReader
{
    public Task<string?> LayTrangThaiKhoaAsync(Guid maNguoiDung, CancellationToken ct) =>
        db.Set<NguoiDungHeThong>()
            .AsNoTracking()
            .Where(x => x.MaNguoiDung == maNguoiDung)
            .Select(x => (string?)x.TrangThaiKhoa)
            .FirstOrDefaultAsync(ct);
}

