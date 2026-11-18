using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuanLyMuonTra.Modules.TaiKhoan.Application;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

/// <summary>
/// Dịch vụ tra cứu và kiểm tra quyền phòng của nhân viên, hỗ trợ đệm bộ nhớ (in-memory cache).
/// <para>
/// <b>Ghi chú kiến trúc (ADR / K-15):</b> Hiện tại hệ thống triển khai dạng single-instance trên Render free tier,
/// do đó sử dụng <see cref="IMemoryCache"/> với TTL 30 giây và cơ chế xóa đệm trực tiếp qua <see cref="IBoNhoDemQuyenPhong"/>.
/// Khi nâng cấp hệ thống lên môi trường nhiều node (multi-node / distributed replica), cần thay thế bộ đệm cục bộ
/// bằng <c>IDistributedCache</c> (Redis) hoặc kết hợp Redis Pub/Sub để phát tín hiệu xóa đệm đồng bộ giữa các node,
/// tránh rủi ro rò rỉ quyền trong cửa sổ thời gian 30 giây trên các node chưa được xóa đệm.
/// </para>
/// </summary>
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
