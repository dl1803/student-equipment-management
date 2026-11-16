using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed record ThongTinToiResponse(
    Guid MaNguoiDung,
    string VaiTro,
    string TenDangNhap,
    string HoTen,
    IReadOnlyList<PhongThietBiDto> DsPhong);

public sealed class LayThongTinToiHandler(
    DbContext db,
    ICurrentUser nguoiDungHienTai,
    IQuyenPhongService quyenPhong,
    IDanhMucReader danhMuc)
{
    public async Task<ThongTinToiResponse> XuLyAsync(CancellationToken ct)
    {
        var ma = nguoiDungHienTai.MaNguoiDung
            ?? throw new LoiNghiepVuException("CHUA_XAC_THUC", "Thiếu thông tin người dùng đăng nhập.");

        var taiKhoan = await db.Set<NguoiDungHeThong>().AsNoTracking().FirstOrDefaultAsync(x => x.MaNguoiDung == ma, ct)
            ?? throw new LoiNghiepVuException("CHUA_XAC_THUC", "Tài khoản không tồn tại.");

        IReadOnlyList<PhongThietBiDto> dsPhong = [];
        if (taiKhoan is NhanVienQuanLy)
        {
            var dsMaPhong = (await quyenPhong.LayDsMaPhongAsync(ma, ct)).ToHashSet();
            if (dsMaPhong.Count > 0)
            {
                dsPhong = (await danhMuc.DsPhongAsync(ct)).Where(p => dsMaPhong.Contains(p.MaPhong)).ToList();
            }
        }

        return new ThongTinToiResponse(taiKhoan.MaNguoiDung, taiKhoan.VaiTro, taiKhoan.TenDangNhap, taiKhoan.HoTen, dsPhong);
    }
}

