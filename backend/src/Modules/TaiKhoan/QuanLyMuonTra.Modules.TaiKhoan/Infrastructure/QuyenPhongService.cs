using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuanLyMuonTra.Modules.TaiKhoan.Application;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

public sealed class QuyenPhongService(DbContext db, IMemoryCache boNhoDem) : IQuyenPhongService, IBoNhoDemQuyenPhong
{
    public static readonly TimeSpan ThoiGianDem = TimeSpan.FromSeconds(30);

    public async Task<IReadOnlyCollection<Guid>> LayDsMaPhongAsync(Guid maNguoiDung, CancellationToken ct)
    {
        return await LayHoacTaoAsync(maNguoiDung, ct);
    }

    public async Task EnsureAsync(Guid maNguoiDung, Guid maPhong, CancellationToken ct)
    {
        var dsMaPhong = await LayHoacTaoAsync(maNguoiDung, ct);

        if (!dsMaPhong.Contains(maPhong))
        {
            throw new LoiNghiepVuException("PHONG_CHUA_CAP_QUYEN", "Nhân viên chưa được cấp quyền trên phòng này.");
        }
    }

    public void Xoa(Guid maNguoiDung)
    {
        boNhoDem.Remove(TaoKhoa(maNguoiDung));
    }

    private static string TaoKhoa(Guid maNguoiDung) => $"quyen-phong:{maNguoiDung:N}";

    private async Task<Guid[]> LayHoacTaoAsync(Guid maNguoiDung, CancellationToken ct)
    {
        var ketQua = await boNhoDem.GetOrCreateAsync(TaoKhoa(maNguoiDung), async muc =>
        {
            muc.AbsoluteExpirationRelativeToNow = ThoiGianDem;

            return await db.Set<NhanVienPhong>()
                .AsNoTracking()
                .Where(x => x.MaNguoiDung == maNguoiDung)
                .Select(x => x.MaPhong)
                .ToArrayAsync(ct);
        });

        return ketQua ?? [];
    }
}
