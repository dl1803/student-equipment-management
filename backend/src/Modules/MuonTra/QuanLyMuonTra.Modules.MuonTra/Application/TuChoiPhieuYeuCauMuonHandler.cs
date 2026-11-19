using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class TuChoiPhieuYeuCauMuonHandler(
    DbContext db,
    IDanhMucReader danhMuc,
    IQuyenPhongService quyenPhong,
    IAuditLogger nhatKy,
    IThongBaoDuyetOutbox thongBao)
{
    public async Task<PhieuYeuCauMuonQuayDto> XuLyAsync(Guid maNv, string maYeuCau, string lyDo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Từ chối phải kèm lý do.");
        }

        await PhieuYeuCauMuonQuayTruyCap.LayVaKiemQuyenAsync(db, danhMuc, quyenPhong, maNv, maYeuCau, ct);

        var lyDoDaCat = lyDo.Trim();
        var moGiaoDich = db.Database.CurrentTransaction is null;
        var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var phieu = await db.Set<PhieuYeuCauMuon>().SingleAsync(p => p.MaYeuCau == maYeuCau, ct);

            try
            {
                phieu.TuChoi(maNv, lyDoDaCat);
            }
            catch (NghiepVuException loi)
            {
                throw DichLoiMienNghiepVu.Chuyen(loi);
            }

            await db.SaveChangesAsync(ct);

            await thongBao.GhiAsync(
                new ThongBaoDuyetHoacTuChoi(maYeuCau, phieu.MaSv, false, null, lyDoDaCat),
                ct);

            await nhatKy.GhiAsync(
                "TU_CHOI_YEU_CAU_MUON",
                "muon_tra.phieu_yeu_cau_muon",
                maYeuCau,
                new Dictionary<string, object?> { ["TrangThai"] = "CHO_DUYET" },
                new Dictionary<string, object?>
                {
                    ["TrangThai"] = "TU_CHOI",
                    ["MaNvDuyet"] = maNv,
                    ["LyDoTuChoi"] = lyDoDaCat
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
            ?? throw new InvalidOperationException($"Không đọc lại được phiếu yêu cầu {maYeuCau} vừa cập nhật.");
    }
}
