using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class KiemTraPhieuYeuCauMuonHandler(
    DbContext db,
    IClock dongHo,
    IDanhMucReader danhMuc,
    IQuyDinhReader quyDinh,
    IMuonTraReader muonTra,
    IAvailabilityService khaDung,
    IQuyenPhongService quyenPhong)
{
    public async Task<KetQuaKiemTraPhieuDto> XuLyAsync(Guid maNv, string maYeuCau, CancellationToken ct)
    {
        var phieu = await PhieuYeuCauMuonQuayTruyCap.LayVaKiemQuyenAsync(db, danhMuc, quyenPhong, maNv, maYeuCau, ct);
        var dsKhaDung = await KiemTraDieuKienDuyet.TinhKhaDungAsync(khaDung, phieu, ct);

        if (phieu.TrangThai != KiemTraDieuKienDuyet.TrangThaiChoDuyet)
        {
            var lyDoTrangThai = new LyDoDto(
                MaLoi.TrangThaiKhongHopLe,
                $"Phiếu yêu cầu {phieu.MaYeuCau} đang ở trạng thái {phieu.TrangThai}, không thể duyệt.");

            return new KetQuaKiemTraPhieuDto(false, [lyDoTrangThai], dsKhaDung);
        }

        var thoiDiem = dongHo.UtcNow.ToUniversalTime();
        var hoSo = await HoSoSinhVienTruyVan.LayAsync(db, phieu.MaSV, ct);
        var quyDinhNhom = await quyDinh.LayQuyDinhNhomAsync(hoSo.MaNhom, ct)
            ?? throw new InvalidOperationException($"Thiếu quy định mượn chung của nhóm {hoSo.MaNhom}.");
        var soDangCam = await muonTra.DemThietBiDangCamAsync(phieu.MaSV, ct);

        var dsLyDo = KiemTraDieuKienDuyet.TongHopLyDo(
            hoSo,
            VnTime.NgayLich(thoiDiem),
            quyDinhNhom,
            phieu.DsLoaiVaSoLuong,
            phieu.ThoiGianMuonDuKienGio,
            soDangCam,
            dsKhaDung);

        return new KetQuaKiemTraPhieuDto(dsLyDo.Count == 0, dsLyDo, dsKhaDung);
    }
}
