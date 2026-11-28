using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed class GhiNhanDaNopHandler(
    DbContext db,
    IClock dongHo,
    IQuyenPhongService quyenPhong,
    XetMoKhoaQuyenMuon xetMoKhoa,
    IAuditLogger nhatKy)
{
    public async Task<BienBanViPhamDto> XuLyAsync(Guid maNv, string maBienBan, CancellationToken ct)
    {
        KiemTraDocViPham.KiemTraMaBienBan(maBienBan);

        var bienBan = await BienBanTruyVan.LayDongAsync(db, maBienBan, ct);
        var dsMaPhong = (await quyenPhong.LayDsMaPhongAsync(maNv, ct)).ToArray();
        var trongPhamVi = dsMaPhong.Length > 0
            && await BienBanTruyVan.TruyVanBienBanTheoPhong(db, dsMaPhong)
                .AnyAsync(b => b.MaBienBan == maBienBan, ct);
        if (!trongPhamVi)
        {
            throw new LoiNghiepVuException(
                MaLoi.PhongChuaCapQuyen,
                "Nhân viên chưa được cấp quyền phòng của biên bản này.");
        }

        return await GiaoDichViPham.ChayAsync(db, async () =>
        {
            await GiaoDichViPham.KhoaDongSinhVienAsync(db, bienBan.MaSv, ct);

            var bayGio = dongHo.UtcNow.ToUniversalTime();
            var soDong = await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE vi_pham.bien_ban_vi_pham
                SET trang_thai = 'DA_NOP', thoi_diem_nop = {bayGio}
                WHERE ma_bien_ban = {maBienBan} AND trang_thai = 'CHUA_NOP'
                """, ct);
            if (soDong == 0)
            {
                throw new LoiNghiepVuException(
                    MaLoi.TrangThaiKhongHopLe,
                    $"Biên bản {maBienBan} không còn ở trạng thái CHUA_NOP.");
            }

            await nhatKy.GhiAsync(
                "GHI_NHAN_NOP_BIEN_BAN_VI_PHAM",
                "vi_pham.bien_ban_vi_pham",
                maBienBan,
                new Dictionary<string, object?> { ["TrangThai"] = "CHUA_NOP", ["ThoiDiemNop"] = null },
                new Dictionary<string, object?> { ["TrangThai"] = "DA_NOP", ["ThoiDiemNop"] = bayGio },
                ct);

            await xetMoKhoa.XetAsync(bienBan.MaSv, ct);

            return await BienBanTruyVan.LayMotAsync(db, maBienBan, ct);
        }, ct);
    }
}

