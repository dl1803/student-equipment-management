using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DuyetPhieuYeuCauMuonHandler(
    DbContext db,
    IClock dongHo,
    IDanhMucReader danhMuc,
    IQuyDinhReader quyDinh,
    IMuonTraReader muonTra,
    IAvailabilityService khaDung,
    IAdvisoryLockService khoa,
    IQuyenPhongService quyenPhong,
    IAuditLogger nhatKy,
    IThongBaoDuyetOutbox thongBao)
{
    public async Task<PhieuYeuCauMuonQuayDto> XuLyAsync(Guid maNv, string maYeuCau, CancellationToken ct)
    {
        var phieuTomTat = await PhieuYeuCauMuonQuayTruyCap.LayVaKiemQuyenAsync(db, danhMuc, quyenPhong, maNv, maYeuCau, ct);

        var moGiaoDich = db.Database.CurrentTransaction is null;
        var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync(ct) : null;
        try
        {
            await HoSoSinhVienTruyVan.KhoaAsync(db, phieuTomTat.MaSV, ct);
            await khoa.LockDanhSachPhongLoaiAsync(
                phieuTomTat.DsLoaiVaSoLuong.Select(d => (phieuTomTat.MaPhong, d.MaLoai)),
                ct);

            var phieu = await db.Set<PhieuYeuCauMuon>().SingleAsync(p => p.MaYeuCau == maYeuCau, ct);
            if (phieu.TrangThai != TrangThaiYeuCauMuon.ChoDuyet)
            {
                throw new LoiNghiepVuException(
                    MaLoi.TrangThaiKhongHopLe,
                    $"Phiếu yêu cầu {maYeuCau} không còn ở trạng thái chờ duyệt.");
            }

            var thoiDiem = dongHo.UtcNow.ToUniversalTime();
            var hoSo = await HoSoSinhVienTruyVan.LayAsync(db, phieu.MaSv, ct);
            var quyDinhNhom = await quyDinh.LayQuyDinhNhomAsync(hoSo.MaNhom, ct)
                ?? throw new InvalidOperationException($"Thiếu quy định mượn chung của nhóm {hoSo.MaNhom}.");
            var soDangCam = await muonTra.DemThietBiDangCamAsync(phieu.MaSv, ct);
            var dsKhaDung = await KiemTraDieuKienDuyet.TinhKhaDungAsync(khaDung, phieuTomTat, ct);

            var dsLyDo = KiemTraDieuKienDuyet.TongHopLyDo(
                hoSo,
                VnTime.NgayLich(thoiDiem),
                quyDinhNhom,
                phieuTomTat.DsLoaiVaSoLuong,
                phieu.ThoiGianMuonDuKienGio,
                soDangCam,
                dsKhaDung);
            if (dsLyDo.Count > 0)
            {
                throw new LoiNghiepVuException(dsLyDo[0].MaLoi, dsLyDo[0].ThongDiep);
            }

            var thamSo = await quyDinh.LayThamSoAsync(ct);
            var hanGiuCho = thoiDiem.AddMinutes(thamSo.ThoiGianGiuChoPhut);

            try
            {
                phieu.Duyet(maNv, thoiDiem, hanGiuCho);
            }
            catch (NghiepVuException loi)
            {
                throw DichLoiMienNghiepVu.Chuyen(loi);
            }

            await db.SaveChangesAsync(ct);

            await thongBao.GhiAsync(
                new ThongBaoDuyetHoacTuChoi(maYeuCau, phieu.MaSv, true, hanGiuCho, null),
                ct);

            await nhatKy.GhiAsync(
                "DUYET_YEU_CAU_MUON",
                "muon_tra.phieu_yeu_cau_muon",
                maYeuCau,
                new Dictionary<string, object?> { ["TrangThai"] = "CHO_DUYET" },
                new Dictionary<string, object?>
                {
                    ["TrangThai"] = "DA_DUYET",
                    ["MaNvDuyet"] = maNv,
                    ["ThoiDiemDuyet"] = thoiDiem,
                    ["HanGiuCho"] = hanGiuCho
                },
                ct);

            if (giaoDich is not null)
            {
                await giaoDich.CommitAsync(ct);
            }
        }
        finally
        {
            if (giaoDich is not null)
            {
                await giaoDich.DisposeAsync();
            }
        }

        return await PhieuYeuCauMuonTruyVan.LayQuayTheoMaAsync(db, danhMuc, maYeuCau, ct)
            ?? throw new InvalidOperationException($"Không đọc lại được phiếu yêu cầu {maYeuCau} vừa duyệt.");
    }
}
