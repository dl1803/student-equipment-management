using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DsPhieuYeuCauMuonQuayHandler(DbContext db, IDanhMucReader danhMuc, IQuyenPhongService quyenPhong)
{
    private static readonly HashSet<string> TrangThaiHopLe =
    [
        "CHO_DUYET",
        "DA_DUYET",
        "TU_CHOI",
        "DA_HUY",
        "HET_HAN",
        "DA_NHAN"
    ];

    public async Task<KetQuaPhanTrang<PhieuYeuCauMuonQuayDto>> XuLyAsync(
        Guid maNv,
        string? trangThai,
        Guid? maPhong,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        if (trangThai is not null && !TrangThaiHopLe.Contains(trangThai))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Trạng thái lọc không hợp lệ.");
        }

        var dsMaPhong = (await quyenPhong.LayDsMaPhongAsync(maNv, ct)).ToList();
        if (maPhong is not null)
        {
            dsMaPhong = dsMaPhong.Where(p => p == maPhong.Value).ToList();
        }

        if (dsMaPhong.Count == 0)
        {
            return PhanTrang.Rong<PhieuYeuCauMuonQuayDto>(0, trang, kichThuoc);
        }

        return await PhieuYeuCauMuonTruyVan.DsTheoPhongAsync(db, danhMuc, dsMaPhong, trangThai, trang, kichThuoc, ct);
    }
}
