using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class ChiTietPhieuMuonSvHandler(DbContext db, IDanhMucReader danhMuc, IAnhService anhService)
{
    public async Task<PhieuMuonSvDto> XuLyAsync(string maSv, string maPhieu, CancellationToken ct)
    {
        KiemTraDocPhieuMuon.KiemTraMaPhieu(maPhieu);

        var du = await PhieuMuonDoc.TaiAsync(db, danhMuc, maPhieu, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy phiếu mượn.");

        if (du.Phieu.MaSv != maSv)
        {
            throw new LoiNghiepVuException(MaLoi.KhongDuQuyen, "Không có quyền xem phiếu mượn của người khác.");
        }

        var dayDu = await PhieuMuonDoc.DungAsync(du, anhService, ct);
        var dsBienBan = await BienBanPhieuTruyVan.LayTheoPhieuAsync(db, maPhieu, maSv, ct);

        return new PhieuMuonSvDto(
            dayDu.MaPhieu,
            dayDu.MaSV,
            dayDu.MaYeuCau,
            dayDu.MaDangKy,
            dayDu.NgayGioMuon,
            dayDu.NgayGioHenTra,
            dayDu.SoLanGiaHan,
            dayDu.TrangThai,
            dayDu.SoThietBi,
            dayDu.DsChiTiet,
            dsBienBan);
    }
}
