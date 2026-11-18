using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed record ThongTinNhanVien(
    Guid MaNguoiDung,
    string MaNv,
    string TenDangNhap,
    string HoTen,
    string? Email,
    string TrangThaiKhoa)
{
    public static ThongTinNhanVien Tu(NhanVienQuanLy nhanVien) =>
        new(
            nhanVien.MaNguoiDung,
            nhanVien.MaNv,
            nhanVien.TenDangNhap,
            nhanVien.HoTen,
            nhanVien.Email,
            nhanVien.TrangThaiKhoa);
}

public sealed class NhanVienDtoBuilder(DbContext db, IDanhMucReader danhMuc)
{
    public async Task<NhanVienDto> TaoMotAsync(ThongTinNhanVien nhanVien, CancellationToken ct)
    {
        var dsDto = await TaoAsync([nhanVien], ct);

        return dsDto[0];
    }

    public async Task<IReadOnlyList<NhanVienDto>> TaoAsync(IReadOnlyList<ThongTinNhanVien> dsNhanVien, CancellationToken ct)
    {
        if (dsNhanVien.Count == 0)
        {
            return [];
        }

        var dsMaNguoiDung = dsNhanVien.Select(x => x.MaNguoiDung).ToList();
        var dsQuyen = await db.Set<NhanVienPhong>()
            .AsNoTracking()
            .Where(x => dsMaNguoiDung.Contains(x.MaNguoiDung))
            .Select(x => new { x.MaNguoiDung, x.MaPhong })
            .ToListAsync(ct);
        var maPhongTheoNhanVien = dsQuyen
            .GroupBy(x => x.MaNguoiDung)
            .ToDictionary(g => g.Key, g => g.Select(x => x.MaPhong).ToHashSet());
        var tatCaPhong = await danhMuc.DsPhongAsync(ct);

        var ketQua = new List<NhanVienDto>(dsNhanVien.Count);
        foreach (var nhanVien in dsNhanVien)
        {
            var dsPhong = new List<PhongThietBiDto>();
            if (maPhongTheoNhanVien.TryGetValue(nhanVien.MaNguoiDung, out var maPhongDuocCap))
            {
                dsPhong.AddRange(tatCaPhong.Where(p => maPhongDuocCap.Contains(p.MaPhong)));
            }

            ketQua.Add(new NhanVienDto(
                nhanVien.MaNguoiDung,
                nhanVien.MaNv,
                nhanVien.TenDangNhap,
                nhanVien.HoTen,
                nhanVien.Email,
                nhanVien.TrangThaiKhoa,
                dsPhong));
        }

        return ketQua;
    }
}

