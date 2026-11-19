using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

internal static class ThamChieuThietBi
{
    internal static async Task KiemTraAsync(
        DbContext db,
        Guid maLoai,
        Guid maPhong,
        List<Guid> dsMaNcc,
        CancellationToken ct)
    {
        if (!await db.Set<LoaiThietBi>().AsNoTracking().AnyAsync(l => l.MaLoai == maLoai, ct))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Loại thiết bị không tồn tại.");
        }

        if (!await db.Set<PhongThietBi>().AsNoTracking().AnyAsync(p => p.MaPhong == maPhong, ct))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Phòng thiết bị không tồn tại.");
        }

        if (dsMaNcc.Count == 0)
        {
            return;
        }

        var dsMaNccDuyNhat = dsMaNcc.Distinct().ToList();
        if (dsMaNccDuyNhat.Count != dsMaNcc.Count)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Danh sách nhà cung cấp không được trùng.");
        }

        var soTonTai = await db.Set<NhaCungCap>().AsNoTracking().CountAsync(n => dsMaNccDuyNhat.Contains(n.MaNCC), ct);
        if (soTonTai != dsMaNccDuyNhat.Count)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Nhà cung cấp không tồn tại.");
        }
    }
}
