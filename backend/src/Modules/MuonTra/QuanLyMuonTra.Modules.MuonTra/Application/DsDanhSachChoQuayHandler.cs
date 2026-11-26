using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DsDanhSachChoQuayHandler(DbContext db, IDanhMucReader danhMuc, IQuyenPhongService quyenPhong)
{
    public async Task<KetQuaPhanTrang<DangKyChoQuayDto>> XuLyAsync(
        Guid maNv,
        Guid? maPhong,
        Guid? maLoai,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var dsMaPhong = (await quyenPhong.LayDsMaPhongAsync(maNv, ct)).ToList();
        if (maPhong is not null)
        {
            dsMaPhong = dsMaPhong.Where(p => p == maPhong.Value).ToList();
        }

        if (dsMaPhong.Count == 0)
        {
            return PhanTrang.Rong<DangKyChoQuayDto>(0, trang, kichThuoc);
        }

        return await DangKyChoTruyVan.DsTheoPhongAsync(db, danhMuc, dsMaPhong, maLoai, trang, kichThuoc, ct);
    }
}
