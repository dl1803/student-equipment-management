using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class CapNhatQuyenPhongHandler(
    DbContext db,
    IDanhMucReader danhMuc,
    IBoNhoDemQuyenPhong boNhoDem,
    NhanVienDtoBuilder boDung)
{
    public async Task<NhanVienDto> XuLyAsync(Guid ma, IReadOnlyCollection<Guid> dsMaPhong, CancellationToken ct)
    {
        var dsMoi = dsMaPhong.ToHashSet();

        var tonTai = await db.Set<NhanVienQuanLy>().AsNoTracking().AnyAsync(x => x.MaNguoiDung == ma, ct);
        if (!tonTai)
        {
            throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy tài khoản nhân viên.");
        }

        var dsPhongHopLe = (await danhMuc.DsPhongAsync(ct)).Select(p => p.MaPhong).ToHashSet();
        if (dsMoi.Any(maPhong => !dsPhongHopLe.Contains(maPhong)))
        {
            throw new LoiNghiepVuException("KHONG_TIM_THAY", "Có phòng thiết bị không tồn tại.");
        }

        await using var giaoDich = await db.Database.BeginTransactionAsync(ct);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung = {ma} FOR UPDATE", ct);

        var hienCo = await db.Set<NhanVienPhong>().Where(x => x.MaNguoiDung == ma).ToListAsync(ct);
        var daCo = hienCo.Select(x => x.MaPhong).ToHashSet();
        var canXoa = hienCo.Where(x => !dsMoi.Contains(x.MaPhong)).ToList();
        var canThem = dsMoi.Where(maPhong => !daCo.Contains(maPhong)).Select(maPhong => NhanVienPhong.Tao(ma, maPhong)).ToList();

        db.Set<NhanVienPhong>().RemoveRange(canXoa);
        db.Set<NhanVienPhong>().AddRange(canThem);

        try
        {
            await db.SaveChangesAsync(ct);
            await giaoDich.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new LoiNghiepVuException("XUNG_DOT_CAP_NHAT", "Quyền phòng vừa được thay đổi bởi giao dịch khác, vui lòng tải lại.");
        }

        boNhoDem.Xoa(ma);

        var nhanVien = await db.Set<NhanVienQuanLy>().AsNoTracking().FirstAsync(x => x.MaNguoiDung == ma, ct);

        return await boDung.TaoMotAsync(ThongTinNhanVien.Tu(nhanVien), ct);
    }
}

