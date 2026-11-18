using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class MoKhoaNhanVienHandler(
    DbContext db,
    NhanVienDtoBuilder boDung,
    IBoNhoDemQuyenPhong boNhoDem)
{
    public async Task<NhanVienDto> XuLyAsync(Guid ma, CancellationToken ct)
    {
        await using var giaoDich = await db.Database.BeginTransactionAsync(ct);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {ma} FOR UPDATE", ct);

        var nhanVien = await db.Set<NhanVienQuanLy>().FirstOrDefaultAsync(x => x.MaNguoiDung == ma, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy tài khoản nhân viên.");

        nhanVien.MoKhoa();
        await db.SaveChangesAsync(ct);

        await giaoDich.CommitAsync(ct);

        boNhoDem.Xoa(ma);

        return await boDung.TaoMotAsync(ThongTinNhanVien.Tu(nhanVien), ct);
    }
}

