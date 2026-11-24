using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

public sealed class AvailabilityService : IAvailabilityService
{
    private readonly DbContext _db;
    private readonly IClock _clock;

    public AvailabilityService(DbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public Task<int> TinhKhaDungAsync(Guid maPhong, Guid maLoai, CancellationToken ct)
    {
        var thoiDiemHienTai = _clock.UtcNow.ToUniversalTime();

        return _db.Database
            .SqlQuery<int>($"""
                SELECT GREATEST(0, (
                    (SELECT count(*)
                     FROM danh_muc.thiet_bi
                     WHERE ma_phong = {maPhong} AND ma_loai = {maLoai}
                       AND tinh_trang = 'SAN_SANG' AND ngung_su_dung = false)
                  - COALESCE((SELECT sum(c.so_luong)
                     FROM muon_tra.phieu_yeu_cau_muon y
                     JOIN muon_tra.chi_tiet_yeu_cau_muon c ON c.ma_yeu_cau = y.ma_yeu_cau
                     WHERE y.ma_phong = {maPhong} AND c.ma_loai = {maLoai}
                       AND y.trang_thai = 'DA_DUYET' AND y.han_giu_cho > {thoiDiemHienTai}), 0)
                  - (SELECT count(*)
                     FROM muon_tra.dang_ky_cho
                     WHERE ma_phong = {maPhong} AND ma_loai = {maLoai}
                       AND trang_thai = 'DA_THONG_BAO' AND han_giu_cho > {thoiDiemHienTai})
                ))::integer AS "Value"
                """)
            .SingleAsync(ct);
    }

    public async Task<IReadOnlyList<ViTriSanSangDto>> DsViTriSanSangAsync(Guid maPhong, Guid maLoai, CancellationToken ct)
    {
        var cacDong = await _db.Database
            .SqlQuery<DongViTriSanSang>($"""
                SELECT vi_tri_ke AS "ViTriKe", vi_tri_tu AS "ViTriTu", count(*)::integer AS "SoThietBiSanSang"
                FROM danh_muc.thiet_bi
                WHERE ma_phong = {maPhong} AND ma_loai = {maLoai}
                  AND tinh_trang = 'SAN_SANG' AND ngung_su_dung = false
                GROUP BY vi_tri_ke, vi_tri_tu
                """)
            .ToListAsync(ct);

        return cacDong
            .OrderBy(d => d.ViTriKe, StringComparer.Ordinal)
            .ThenBy(d => d.ViTriTu, StringComparer.Ordinal)
            .Select(d => new ViTriSanSangDto(d.ViTriKe, d.ViTriTu, d.SoThietBiSanSang))
            .ToList();
    }
}
