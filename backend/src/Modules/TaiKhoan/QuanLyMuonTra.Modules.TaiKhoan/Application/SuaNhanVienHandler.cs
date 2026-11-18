using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class SuaNhanVienHandler(DbContext db, NhanVienDtoBuilder boDung)
{
    public async Task<NhanVienDto> XuLyAsync(Guid ma, string hoTen, string? email, CancellationToken ct)
    {
        var nhanVien = await db.Set<NhanVienQuanLy>().FirstOrDefaultAsync(x => x.MaNguoiDung == ma, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy tài khoản nhân viên.");

        nhanVien.CapNhatThongTin(hoTen, email);
        await db.SaveChangesAsync(ct);

        return await boDung.TaoMotAsync(ThongTinNhanVien.Tu(nhanVien), ct);
    }
}

