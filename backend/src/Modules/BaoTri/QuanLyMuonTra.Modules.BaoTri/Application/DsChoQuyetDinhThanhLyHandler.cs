using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class DsChoQuyetDinhThanhLyHandler(DbContext db)
{
    private const string Goc = """
        FROM (
            SELECT DISTINCT ON (b.ma_thiet_bi)
                   b.ma_thiet_bi, b.ma_bao_tri, b.noi_dung_thuc_hien, b.ket_qua, b.ngay_hoan_thanh
            FROM bao_tri.bao_tri_sua_chua b
            WHERE b.ket_qua IS NOT NULL
            ORDER BY b.ma_thiet_bi, b.ngay_hoan_thanh DESC, b.ma_bao_tri DESC) g
        JOIN danh_muc.thiet_bi t ON t.ma_thiet_bi = g.ma_thiet_bi
        JOIN danh_muc.phong_thiet_bi p ON p.ma_phong = t.ma_phong
        WHERE g.ket_qua = 'DE_XUAT_THANH_LY'
          AND t.tinh_trang = 'DANG_BAO_TRI_SUA_CHUA'
          AND NOT EXISTS (
              SELECT 1 FROM bao_tri.chi_tiet_thanh_ly c WHERE c.ma_thiet_bi = t.ma_thiet_bi)
        """;

    private const string CauDem = "SELECT count(*)::integer AS \"Value\" " + Goc;

    private const string CauTrang = """
        SELECT t.ma_thiet_bi AS "MaThietBi", t.ten_thiet_bi AS "TenThietBi",
               t.ma_loai AS "MaLoai", t.ma_phong AS "MaPhong", p.ten_phong AS "TenPhong",
               g.ma_bao_tri AS "MaBaoTri", g.noi_dung_thuc_hien AS "NoiDungThucHien",
               g.ngay_hoan_thanh AS "NgayHoanThanh",
               (SELECT v.gia_tri FROM danh_muc.lich_su_gia_tri v
                WHERE v.ma_thiet_bi = t.ma_thiet_bi
                ORDER BY v.ngay_cap_nhat DESC, v.ma_lich_su DESC LIMIT 1)::bigint AS "GiaTriHienHanh"
        """ + " " + Goc + " ORDER BY g.ngay_hoan_thanh, t.ma_thiet_bi OFFSET {0} LIMIT {1}";

    public async Task<KetQuaPhanTrang<ThietBiChoQuyetDinhThanhLyDto>> XuLyAsync(
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var tongSo = await db.Database
            .SqlQuery<int>(FormattableStringFactory.Create(CauDem))
            .SingleAsync(ct);

        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<ThietBiChoQuyetDinhThanhLyDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await db.Database
            .SqlQuery<DongChoQuyetDinhThanhLy>(FormattableStringFactory.Create(CauTrang, batDau, (long)kichThuoc))
            .ToListAsync(ct);

        var danhSach = cacDong
            .Select(d => new ThietBiChoQuyetDinhThanhLyDto(
                d.MaThietBi,
                d.TenThietBi,
                d.MaLoai,
                d.MaPhong,
                d.TenPhong,
                d.MaBaoTri,
                d.NoiDungThucHien,
                d.NgayHoanThanh,
                d.GiaTriHienHanh))
            .ToList();

        return new KetQuaPhanTrang<ThietBiChoQuyetDinhThanhLyDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
