using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class DsThietBiDenHanBaoTriHandler(DbContext db, IClock dongHo, IQuyenPhongService quyenPhong)
{
    private const string NgayDenHan =
        "(COALESCE((bt.lan_cuoi AT TIME ZONE 'Asia/Ho_Chi_Minh')::date, t.ngay_nhap_kho) + l.chu_ky_bao_tri)";

    private const string Goc = """
        FROM danh_muc.thiet_bi t
        JOIN danh_muc.loai_thiet_bi l ON l.ma_loai = t.ma_loai
        JOIN danh_muc.phong_thiet_bi p ON p.ma_phong = t.ma_phong
        LEFT JOIN LATERAL (
            SELECT max(b.ngay_hoan_thanh) AS lan_cuoi
            FROM bao_tri.bao_tri_sua_chua b
            WHERE b.ma_thiet_bi = t.ma_thiet_bi
              AND b.loai_thuc_hien = 'BAO_TRI_DINH_KY'
              AND b.ngay_hoan_thanh IS NOT NULL) bt ON true
        WHERE t.tinh_trang IN ('SAN_SANG', 'DANG_DUOC_MUON')
          AND t.ma_phong = ANY({0})
          AND {1}::date >=
        """ + " " + NgayDenHan;

    private const string CauDem = "SELECT count(*)::integer AS \"Value\" " + Goc;

    private const string CauTrang = """
        SELECT t.ma_thiet_bi AS "MaThietBi", t.ten_thiet_bi AS "TenThietBi",
               t.ma_loai AS "MaLoai", l.ten_loai AS "TenLoai",
               t.ma_phong AS "MaPhong", p.ten_phong AS "TenPhong",
               t.tinh_trang AS "TinhTrang",
        """ + " " + NgayDenHan + " AS \"NgayDenHan\" " + Goc + " ORDER BY " + NgayDenHan + ", t.ma_thiet_bi OFFSET {2} LIMIT {3}";

    public async Task<KetQuaPhanTrang<ThietBiDenHanBaoTriDto>> XuLyAsync(
        Guid maNv,
        Guid? maPhong,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var dsMaPhong = await PhamViPhongNhanVien.LayAsync(quyenPhong, maNv, maPhong, ct);
        if (dsMaPhong.Length == 0)
        {
            return PhanTrang.Rong<ThietBiDenHanBaoTriDto>(0, trang, kichThuoc);
        }

        var homNay = VnTime.NgayLich(dongHo.UtcNow);
        var tongSo = await db.Database
            .SqlQuery<int>(FormattableStringFactory.Create(CauDem, dsMaPhong, homNay))
            .SingleAsync(ct);

        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<ThietBiDenHanBaoTriDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await db.Database
            .SqlQuery<DongDenHanBaoTri>(FormattableStringFactory.Create(CauTrang, dsMaPhong, homNay, batDau, (long)kichThuoc))
            .ToListAsync(ct);

        var danhSach = cacDong
            .Select(d => new ThietBiDenHanBaoTriDto(
                d.MaThietBi,
                d.TenThietBi,
                d.MaLoai,
                d.TenLoai,
                d.MaPhong,
                d.TenPhong,
                d.TinhTrang,
                d.NgayDenHan))
            .ToList();

        return new KetQuaPhanTrang<ThietBiDenHanBaoTriDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
