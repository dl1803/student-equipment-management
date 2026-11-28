using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

public sealed class PortGhiNhanMat(DbContext db, IClock dongHo, GhiDongKetQuaNhanTra ghiDong) : IPortGhiNhanMat
{
    public async Task<bool> GhiNhanMatAsync(Guid maChiTietPhieu, Guid maNvNhanTra, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Ghi nhận mất phải nằm trong giao dịch của thao tác lập biên bản.");
        }

        var maPhieu = await db.Database.SqlQuery<string>($"""
                SELECT ma_phieu AS "Value" FROM muon_tra.chi_tiet_phieu_muon WHERE ma_chi_tiet = {maChiTietPhieu}
                """)
            .SingleOrDefaultAsync(ct);
        if (maPhieu is null)
        {
            return false;
        }

        var phieu = await db.Set<PhieuMuon>()
            .Include(p => p.ChiTiet)
            .SingleAsync(p => p.MaPhieu == maPhieu, ct);
        var chiTiet = phieu.ChiTiet.Single(c => c.MaChiTiet == maChiTietPhieu);
        if (chiTiet.KetQuaKiemTra.HasValue)
        {
            throw new LoiNghiepVuException(
                MaLoi.TrangThaiKhongHopLe,
                $"Thiết bị {chiTiet.MaThietBi} đã có kết quả kiểm tra.");
        }

        await ghiDong.GhiAsync(
            phieu,
            new KetQuaKiemTraYeuCau(chiTiet.MaThietBi, KetQuaKiemTra.Mat, null, null, []),
            maNvNhanTra,
            dongHo.UtcNow.ToUniversalTime(),
            ct);

        try
        {
            phieu.XetDaTra();
        }
        catch (NghiepVuException loi)
        {
            throw DichLoiMienNghiepVu.Chuyen(loi);
        }

        await db.SaveChangesAsync(ct);

        return true;
    }
}
