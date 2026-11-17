using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

internal static class ThuHoiChuoiRefreshToken
{
    public static Task<int> ThucHienAsync(DbContext db, Guid maBatDau, CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            WITH RECURSIVE chuoi AS (
                SELECT ma, thay_the_boi FROM tai_khoan.refresh_token WHERE ma = {maBatDau}
                UNION
                SELECT r.ma, r.thay_the_boi
                FROM tai_khoan.refresh_token r
                JOIN chuoi c ON r.ma = c.thay_the_boi
            )
            UPDATE tai_khoan.refresh_token
            SET da_thu_hoi = true
            WHERE ma IN (SELECT ma FROM chuoi) AND da_thu_hoi = false
            """, ct);
}

