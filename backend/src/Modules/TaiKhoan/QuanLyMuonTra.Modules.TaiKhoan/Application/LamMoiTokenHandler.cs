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
        var (moi, tokenMoiGoc) = RefreshToken.TaoMoi(cu.ChuSoHuuLoai, cu.ChuSoHuuMa, bayGio);

        await using var giaoDich = await db.Database.BeginTransactionAsync(ct);
        db.Set<RefreshToken>().Add(moi);
        await db.SaveChangesAsync(ct);

        var soDongXoay = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE tai_khoan.refresh_token
            SET da_thu_hoi = true, thay_the_boi = {moi.Ma}
            WHERE ma = {cu.Ma} AND da_thu_hoi = false AND thay_the_boi IS NULL
            """, ct);

        if (soDongXoay == 0)
        {
            await giaoDich.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            await ThuHoiChuoiRefreshToken.ThucHienAsync(db, cu.Ma, ct);
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

