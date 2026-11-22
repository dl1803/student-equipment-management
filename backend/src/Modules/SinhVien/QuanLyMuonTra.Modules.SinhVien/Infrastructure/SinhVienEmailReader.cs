using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.SinhVien.Contracts;

namespace QuanLyMuonTra.Modules.SinhVien.Infrastructure;

public sealed class SinhVienEmailReader(DbContext db) : ISinhVienEmailReader
{
    public async Task<string?> LayEmailAsync(string maSv, CancellationToken ct)
    {
        var email = await db.Database.SqlQuery<string>($"""
                SELECT email AS "Value"
                FROM sinh_vien.sinh_vien
                WHERE ma_sv = {maSv}
                """)
            .FirstOrDefaultAsync(ct);

        return string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
}

