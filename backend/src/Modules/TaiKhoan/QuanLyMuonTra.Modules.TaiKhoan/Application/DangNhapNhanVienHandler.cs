using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed record TokenResponse(string AccessToken, string RefreshToken, string VaiTro);

public sealed class DangNhapNhanVienHandler(
    DbContext db,
    IPasswordHasher<NguoiDungHeThong> boBam,
    IQuyDinhReader quyDinh,
    IPhatHanhAccessToken phatHanhToken,
    IClock dongHo)
{
    public async Task<TokenResponse> XuLyAsync(string tenDangNhap, string matKhau, CancellationToken ct)
    {
        var nguoiDung = await db.Set<NguoiDungHeThong>().FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap, ct)
            ?? throw new LoiNghiepVuException("DANG_NHAP_SAI", "Thông tin đăng nhập không đúng.");

        if (!nguoiDung.DangHoatDong)
        {
            throw new LoiNghiepVuException("TAI_KHOAN_BI_KHOA", "Tài khoản đã bị khóa hoặc tạm khóa đăng nhập.");
        }

        var ketQua = boBam.VerifyHashedPassword(nguoiDung, nguoiDung.MatKhauBam, matKhau);
        if (ketQua == PasswordVerificationResult.Failed)
        {
            var thamSo = await quyDinh.LayThamSoAsync(ct);
            nguoiDung.GhiNhanDangNhapSai(thamSo.SoLanDangNhapSaiToiDa);
            await db.SaveChangesAsync(ct);

            throw new LoiNghiepVuException("DANG_NHAP_SAI", "Thông tin đăng nhập không đúng.");
        }

        nguoiDung.GhiNhanDangNhapDung();
        var (banGhi, tokenGoc) = RefreshToken.TaoMoi(nguoiDung.VaiTro, nguoiDung.MaNguoiDung.ToString(), dongHo.UtcNow);
        db.Set<RefreshToken>().Add(banGhi);
        await db.SaveChangesAsync(ct);

        return new TokenResponse(phatHanhToken.Tao(nguoiDung.MaNguoiDung, nguoiDung.VaiTro), tokenGoc, nguoiDung.VaiTro);
    }
}

