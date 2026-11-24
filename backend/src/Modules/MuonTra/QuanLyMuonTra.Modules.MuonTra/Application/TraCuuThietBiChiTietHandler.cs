using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class TraCuuThietBiChiTietHandler(DbContext db, IClock dongHo)
{
    public async Task<TraCuuThietBiChiTietDto> XuLyAsync(Guid maLoai, Guid maPhong, CancellationToken ct)
    {
        var thoiDiem = dongHo.UtcNow.ToUniversalTime();

        var dong = await TraCuuTruyVan.LayCapLoaiPhong(db, thoiDiem)
            .FirstOrDefaultAsync(d => d.MaLoai == maLoai && d.MaPhong == maPhong, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy loại thiết bị tại phòng này.");

        var dsThongSo = await db.Database.SqlQuery<string>($"""
                SELECT DISTINCT btrim(thong_so_ky_thuat) AS "Value"
                FROM danh_muc.thiet_bi
                WHERE ma_loai = {maLoai} AND ma_phong = {maPhong}
                  AND thong_so_ky_thuat IS NOT NULL AND btrim(thong_so_ky_thuat) <> ''
                """)
            .OrderBy(x => x)
            .ToListAsync(ct);

        DateTimeOffset? ngayDuKien = null;
        if (dong.SoLuongKhaDung == 0)
        {
            var ketQua = await db.Database.SqlQuery<DongNgayDuKien>($"""
                    SELECT min(pm.ngay_gio_hen_tra) AS "NgayDuKien"
                    FROM muon_tra.phieu_muon pm
                    JOIN muon_tra.chi_tiet_phieu_muon ctpm ON ctpm.ma_phieu = pm.ma_phieu
                    JOIN danh_muc.thiet_bi tb ON tb.ma_thiet_bi = ctpm.ma_thiet_bi
                    WHERE tb.ma_loai = {maLoai} AND tb.ma_phong = {maPhong}
                      AND pm.trang_thai <> 'DA_TRA' AND ctpm.ket_qua_kiem_tra IS NULL
                    """)
                .SingleAsync(ct);
            ngayDuKien = ketQua.NgayDuKien;
        }

        return new TraCuuThietBiChiTietDto(
            dong.MaLoai,
            dong.TenLoai,
            dong.HangSanXuat,
            dong.MaPhong,
            dong.TenPhong,
            dong.CoSo,
            dong.SoLuongKhaDung,
            dong.MoTa,
            dsThongSo,
            [],
            ngayDuKien);
    }
}
