using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class ChiTietPhieuMuonQuayHandler(
    DbContext db,
    IDanhMucReader danhMuc,
    IAnhService anhService,
    IQuyenPhongService quyenPhong)
{
    public async Task<PhieuMuonDayDuDto> XuLyAsync(Guid maNv, string maPhieu, CancellationToken ct)
    {
        KiemTraDocPhieuMuon.KiemTraMaPhieu(maPhieu);

        var du = await PhieuMuonDoc.TaiAsync(db, danhMuc, maPhieu, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy phiếu mượn.");

        var dsMaPhongDuocCap = (await quyenPhong.LayDsMaPhongAsync(maNv, ct)).ToHashSet();
        if (!du.ThietBi.Values.Any(t => dsMaPhongDuocCap.Contains(t.MaPhong)))
        {
            throw new LoiNghiepVuException(
                MaLoi.PhongChuaCapQuyen,
                "Nhân viên chưa được cấp quyền trên phòng của phiếu mượn này.");
        }

        return await PhieuMuonDoc.DungAsync(du, anhService, ct);
    }
}
