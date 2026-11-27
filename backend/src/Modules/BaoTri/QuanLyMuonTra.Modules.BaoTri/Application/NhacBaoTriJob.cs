using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class NhacBaoTriJob(DbContext db, IClock dongHo, IThongBaoOutbox outbox) : IJob
{
    public string MaJob => "J6";

    public string TenJob => "NhacBaoTri";

    public TimeSpan ChuKy => TimeSpan.FromHours(1);

    public async Task<int> ChayAsync(CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "J6 NhacBaoTri phải chạy trong giao dịch đang mở của bộ chạy lịch.");
        }

        var homNay = VnTime.NgayLich(dongHo.UtcNow);

        var dsDenHan = await db.Database.SqlQuery<DongNhacBaoTri>($"""
            SELECT t.ma_thiet_bi AS "MaThietBi", t.ma_phong AS "MaPhong",
                   (COALESCE((bt.lan_cuoi AT TIME ZONE 'Asia/Ho_Chi_Minh')::date, t.ngay_nhap_kho) + l.chu_ky_bao_tri) AS "NgayDenHan"
            FROM danh_muc.thiet_bi t
            JOIN danh_muc.loai_thiet_bi l ON l.ma_loai = t.ma_loai
            LEFT JOIN LATERAL (
                SELECT max(b.ngay_hoan_thanh) AS lan_cuoi
                FROM bao_tri.bao_tri_sua_chua b
                WHERE b.ma_thiet_bi = t.ma_thiet_bi
                  AND b.loai_thuc_hien = 'BAO_TRI_DINH_KY'
                  AND b.ngay_hoan_thanh IS NOT NULL) bt ON true
            WHERE t.tinh_trang IN ('SAN_SANG', 'DANG_DUOC_MUON')
              AND {homNay}::date >=
                  (COALESCE((bt.lan_cuoi AT TIME ZONE 'Asia/Ho_Chi_Minh')::date, t.ngay_nhap_kho) + l.chu_ky_bao_tri)
            ORDER BY "NgayDenHan", t.ma_thiet_bi
            """).ToListAsync(ct);

        var nvTheoPhong = new Dictionary<Guid, IReadOnlyList<Guid>>();
        var soPhanTu = 0;
        foreach (var dong in dsDenHan)
        {
            ct.ThrowIfCancellationRequested();

            if (!nvTheoPhong.TryGetValue(dong.MaPhong, out var dsNv))
            {
                dsNv = await db.Database.SqlQuery<Guid>($"""
                    SELECT ma_nguoi_dung AS "Value" FROM tai_khoan.nhan_vien_phong
                    WHERE ma_phong = {dong.MaPhong}
                    ORDER BY ma_nguoi_dung
                    """).ToListAsync(ct);
                nvTheoPhong[dong.MaPhong] = dsNv;
            }

            var ngayChuan = dong.NgayDenHan.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var ngayHienThi = dong.NgayDenHan.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            foreach (var maNv in dsNv)
            {
                await outbox.GhiAsync(
                    new YeuCauGhiThongBao(
                        MaLoaiThongBao.NhacBaoTri,
                        null,
                        maNv,
                        new Dictionary<string, string>
                        {
                            [ThamSoThongBao.MaThietBi] = dong.MaThietBi,
                            [ThamSoThongBao.NgayDenHan] = ngayHienThi,
                        },
                        "THIET_BI",
                        dong.MaThietBi,
                        $"BAO_TRI:{dong.MaThietBi}:{ngayChuan}:{maNv}"),
                    ct);
                soPhanTu++;
            }
        }

        return soPhanTu;
    }
}
