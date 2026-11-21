using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

public sealed class NhanVienEmailReader(DbContext db) : INhanVienEmailReader
{
    public async Task<string?> LayEmailAsync(Guid maNv, CancellationToken ct)
    {
        var email = await db.Database.SqlQuery<string>($"""
                SELECT email AS "Value"
                FROM tai_khoan.nguoi_dung_he_thong
                WHERE ma_nguoi_dung = {maNv} AND email IS NOT NULL
                """)
            .FirstOrDefaultAsync(ct);

        return string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
}

