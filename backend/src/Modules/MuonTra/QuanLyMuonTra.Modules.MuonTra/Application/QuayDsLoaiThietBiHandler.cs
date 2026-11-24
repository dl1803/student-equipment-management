using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class QuayDsLoaiThietBiHandler(
    DbContext db,
    IClock dongHo,
    ICurrentUser nguoiDungHienTai,
    IQuyenPhongService quyenPhong)
{
    public async Task<KetQuaPhanTrang<TraCuuThietBiItemDto>> XuLyAsync(
        string? ten,
        Guid? maLoai,
        Guid? maPhong,
        bool? conHang,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var maNguoiDung = nguoiDungHienTai.MaNguoiDung
            ?? throw new LoiNghiepVuException(MaLoi.ChuaXacThuc, "Thiếu thông tin người dùng đăng nhập.");

        var dsMaPhong = (await quyenPhong.LayDsMaPhongAsync(maNguoiDung, ct)).ToList();
        if (dsMaPhong.Count == 0)
        {
            return PhanTrang.Rong<TraCuuThietBiItemDto>(0, trang, kichThuoc);
        }

        var goc = TraCuuTruyVan.ApDungBoLoc(
                TraCuuTruyVan.LayCapLoaiPhong(db, dongHo.UtcNow.ToUniversalTime()),
                ten,
                maLoai,
                maPhong,
                conHang)
            .Where(d => dsMaPhong.Contains(d.MaPhong));

        return await TraCuuTruyVan.LayTrangAsync(goc, trang, kichThuoc, ct);
    }
}
