using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed class DsNhanVienHandler(DbContext db, NhanVienDtoBuilder boDung)
{
    private const int KichThuocToiDa = 100;

    public async Task<NhanVienTrangDto> XuLyAsync(int trang, int kichThuoc, CancellationToken ct)
    {
        if (trang < 1)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Số trang phải từ 1 trở lên.");
        }

        if (kichThuoc < 1 || kichThuoc > KichThuocToiDa)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", $"Kích thước trang phải từ 1 đến {KichThuocToiDa}.");
        }

        var goc = db.Set<NhanVienQuanLy>().AsNoTracking();
        var tongSo = await goc.CountAsync(ct);
        var batDau = (long)(trang - 1) * kichThuoc;
        if (batDau >= tongSo)
        {
            return new NhanVienTrangDto(tongSo, trang, kichThuoc, []);
        }

        var dong = await goc
            .OrderBy(x => x.TenDangNhap)
            .ThenBy(x => x.MaNguoiDung)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .Select(x => new ThongTinNhanVien(x.MaNguoiDung, x.MaNv, x.TenDangNhap, x.HoTen, x.Email, x.TrangThaiKhoa))
            .ToListAsync(ct);

        var danhSach = await boDung.TaoAsync(dong, ct);

        return new NhanVienTrangDto(tongSo, trang, kichThuoc, danhSach);
    }
}

