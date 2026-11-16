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
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE tai_khoan.nguoi_dung_he_thong
                SET so_lan_dang_nhap_sai = so_lan_dang_nhap_sai + 1,
                    trang_thai_khoa = CASE 
                        WHEN so_lan_dang_nhap_sai + 1 >= {thamSo.SoLanDangNhapSaiToiDa} THEN {MaTrangThaiKhoa.TamKhoaDangNhap}
                        ELSE trang_thai_khoa 
                    END
                WHERE ma_nguoi_dung = {nguoiDung.MaNguoiDung} AND trang_thai_khoa = {MaTrangThaiKhoa.HoatDong}
                """, ct);
            db.Entry(nguoiDung).State = EntityState.Detached;

            throw new LoiNghiepVuException("DANG_NHAP_SAI", "Thông tin đăng nhập không đúng.");
        }

        nguoiDung.GhiNhanDangNhapDung();
        db.Entry(nguoiDung).Property(x => x.SoLanDangNhapSai).IsModified = true;
        var (banGhi, tokenGoc) = RefreshToken.TaoMoi(nguoiDung.VaiTro, nguoiDung.MaNguoiDung.ToString(), dongHo.UtcNow);
        db.Set<RefreshToken>().Add(banGhi);
        await db.SaveChangesAsync(ct);

        return new TokenResponse(phatHanhToken.Tao(nguoiDung.MaNguoiDung, nguoiDung.VaiTro), tokenGoc, nguoiDung.VaiTro);
    }
}

