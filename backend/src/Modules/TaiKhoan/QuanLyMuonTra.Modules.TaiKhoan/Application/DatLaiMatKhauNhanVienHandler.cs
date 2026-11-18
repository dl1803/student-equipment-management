using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class DatLaiMatKhauNhanVienHandler(DbContext db, IPasswordHasher<NguoiDungHeThong> boBam)
{
    public async Task XuLyAsync(Guid ma, string matKhauMoi, CancellationToken ct)
    {
        var nhanVien = await db.Set<NhanVienQuanLy>().FirstOrDefaultAsync(x => x.MaNguoiDung == ma, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy tài khoản nhân viên.");

        nhanVien.DatLaiMatKhau(boBam.HashPassword(nhanVien, matKhauMoi));
        await db.SaveChangesAsync(ct);
    }
}

