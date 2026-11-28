using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.ViPham.Application;

internal static class GiaoDichViPham
{
    public static async Task<T> ChayAsync<T>(DbContext db, Func<Task<T>> viec, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is not null)
        {
            return await viec();
        }

        await using var giaoDich = await db.Database.BeginTransactionAsync(ct);
        var ketQua = await viec();
        await giaoDich.CommitAsync(ct);

        return ketQua;
    }

    public static Task KhoaDongSinhVienAsync(DbContext db, string maSv, CancellationToken ct)
    {
        return db.Database.ExecuteSqlInterpolatedAsync($"""
            SELECT ma_sv FROM sinh_vien.sinh_vien WHERE ma_sv = {maSv} FOR UPDATE
            """, ct);
    }
}

