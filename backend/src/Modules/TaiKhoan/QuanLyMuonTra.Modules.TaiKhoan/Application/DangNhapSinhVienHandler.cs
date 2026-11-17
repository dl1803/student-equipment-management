using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class DangNhapSinhVienHandler(
    DbContext db,
    IXacThucSinhVien xacThuc,
    IPhatHanhAccessToken phatHanhToken,
    IClock dongHo)
{
    public async Task<TokenResponse> XuLyAsync(string mssv, string matKhau, CancellationToken ct)
    {
        var maSv = await xacThuc.XacThucAsync(mssv, matKhau, ct)
            ?? throw new LoiNghiepVuException("DANG_NHAP_SAI", "Thông tin đăng nhập không đúng.");

        var (banGhi, tokenGoc) = RefreshToken.TaoMoi(MaVaiTro.SinhVien, maSv, dongHo.UtcNow);
        db.Set<RefreshToken>().Add(banGhi);
        await db.SaveChangesAsync(ct);

        return new TokenResponse(phatHanhToken.Tao(maSv, MaVaiTro.SinhVien), tokenGoc, MaVaiTro.SinhVien);
    }
}

