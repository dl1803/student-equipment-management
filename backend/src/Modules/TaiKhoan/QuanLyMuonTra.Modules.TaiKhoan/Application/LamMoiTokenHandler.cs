using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class LamMoiTokenHandler(
    DbContext db,
    IPhatHanhAccessToken phatHanhToken,
    IClock dongHo)
{
    public async Task<TokenResponse> XuLyAsync(string refreshToken, CancellationToken ct)
    {
        var bayGio = dongHo.UtcNow;
        var maBam = RefreshToken.Bam(refreshToken);

        var cu = await db.Set<RefreshToken>().AsNoTracking().FirstOrDefaultAsync(x => x.MaBam == maBam, ct)
            ?? throw TokenKhongHopLe();

        if (cu.ThayTheBoi is not null)
        {
            await ThuHoiChuoiRefreshToken.ThucHienAsync(db, cu.Ma, ct);
            throw TokenKhongHopLe();
        }

        if (cu.DaThuHoi || cu.HetHan <= bayGio)
        {
            throw TokenKhongHopLe();
        }

        var (vaiTro, maDinhDanh) = await XacDinhChuSoHuuAsync(cu, ct);
        var (moi, tokenMoiGoc) = RefreshToken.TaoMoi(vaiTro, cu.ChuSoHuuMa, bayGio);

        await using var giaoDich = await db.Database.BeginTransactionAsync(ct);
        db.Set<RefreshToken>().Add(moi);
        await db.SaveChangesAsync(ct);

        // SET LOCAL lock_timeout giới hạn thời gian chờ row lock trong transaction này.
        // Khi nhiều request đồng thời xoay vòng cùng token, chỉ 1 request "thắng" UPDATE;
        // các request "thua" sẽ nhận lock_timeout thay vì đợi vô hạn (đặc biệt quan trọng
        // trên PgBouncer/Supabase vốn không hủy lock wait tự động).
        await db.Database.ExecuteSqlRawAsync("SET LOCAL lock_timeout = '2000ms'", ct);

        int soDongXoay;
        try
        {
            soDongXoay = await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE tai_khoan.refresh_token
                SET da_thu_hoi = true, thay_the_boi = {moi.Ma}
                WHERE ma = {cu.Ma} AND da_thu_hoi = false AND thay_the_boi IS NULL
                """, ct);
        }
        catch (Exception ex) when (ex.InnerException?.GetType().Name == "NpgsqlException"
            && (ex.InnerException.Message.Contains("55P03") || ex.InnerException.Message.Contains("lock_not_available") || ex.InnerException.Message.Contains("lock timeout")))
        {
            await giaoDich.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            throw TokenKhongHopLe();
        }

        if (soDongXoay == 0)
        {
            await giaoDich.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            throw TokenKhongHopLe();
        }

        await giaoDich.CommitAsync(ct);

        return new TokenResponse(phatHanhToken.Tao(maDinhDanh, vaiTro), tokenMoiGoc, vaiTro);
    }

    private static LoiNghiepVuException TokenKhongHopLe() =>
        new("CHUA_XAC_THUC", "Refresh token không hợp lệ, đã hết hạn hoặc đã bị thu hồi.");

    private async Task<(string VaiTro, string MaDinhDanh)> XacDinhChuSoHuuAsync(RefreshToken token, CancellationToken ct)
    {
        if (token.ChuSoHuuLoai == MaVaiTro.SinhVien)
        {
            var tonTai = await db.Database.SqlQuery<int>($"""
                SELECT count(*)::int AS "Value"
                FROM sinh_vien.sinh_vien
                WHERE ma_sv = {token.ChuSoHuuMa}
                """).SingleAsync(ct);

            if (tonTai == 0)
            {
                throw TokenKhongHopLe();
            }

            return (MaVaiTro.SinhVien, token.ChuSoHuuMa);
        }

        if (!Guid.TryParse(token.ChuSoHuuMa, out var maNguoiDung))
        {
            throw TokenKhongHopLe();
        }

        var nguoiDung = await db.Set<NguoiDungHeThong>().AsNoTracking().FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung, ct);
        if (nguoiDung is null || !nguoiDung.DangHoatDong)
        {
            throw TokenKhongHopLe();
        }

        return (nguoiDung.VaiTro, nguoiDung.MaNguoiDung.ToString());
    }
}
