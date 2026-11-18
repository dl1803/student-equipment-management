using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class DatLaiMatKhauNhanVienHandler(DbContext db, IPasswordHasher<NguoiDungHeThong> boBam)
{
    public async Task XuLyAsync(Guid ma, string matKhauMoi, CancellationToken ct)
    {
        await using var giaoDich = await db.Database.BeginTransactionAsync(ct);

        var nhanVien = await db.Set<NhanVienQuanLy>().FirstOrDefaultAsync(x => x.MaNguoiDung == ma, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy tài khoản nhân viên.");

        nhanVien.DatLaiMatKhau(boBam.HashPassword(nhanVien, matKhauMoi));
        await db.SaveChangesAsync(ct);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE tai_khoan.refresh_token SET da_thu_hoi = true
            WHERE chu_so_huu_loai = {MaVaiTro.NhanVien} AND chu_so_huu_ma = {ma.ToString()} AND da_thu_hoi = false
            """, ct);

        await giaoDich.CommitAsync(ct);
    }
}

