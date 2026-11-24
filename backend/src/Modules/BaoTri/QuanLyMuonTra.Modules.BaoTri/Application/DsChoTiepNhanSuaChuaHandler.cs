using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class DsChoTiepNhanSuaChuaHandler(DbContext db, IQuyenPhongService quyenPhong)
{
    private const string Goc = """
        FROM danh_muc.thiet_bi t
        JOIN danh_muc.phong_thiet_bi p ON p.ma_phong = t.ma_phong
        WHERE t.tinh_trang = 'DANG_BAO_TRI_SUA_CHUA'
          AND t.ma_phong = ANY({0})
          AND NOT EXISTS (
              SELECT 1 FROM bao_tri.bao_tri_sua_chua b
              WHERE b.ma_thiet_bi = t.ma_thiet_bi AND b.ket_qua IS NULL)
        """;

    private const string CauDem = "SELECT count(*)::integer AS \"Value\" " + Goc;

    private const string CauTrang = """
        SELECT t.ma_thiet_bi AS "MaThietBi", t.ten_thiet_bi AS "TenThietBi",
               t.ma_phong AS "MaPhong", p.ten_phong AS "TenPhong",
               (SELECT c.ma_phieu
                FROM muon_tra.chi_tiet_phieu_muon c
                WHERE c.ma_thiet_bi = t.ma_thiet_bi
                  AND c.ket_qua_kiem_tra IN ('HU_HONG', 'THIEU_PHU_KIEN')
                ORDER BY c.ngay_gio_tra_thuc_te DESC, c.ma_chi_tiet DESC
                LIMIT 1) AS "MaPhieuMuon"
        """ + " " + Goc + " ORDER BY t.ma_thiet_bi OFFSET {1} LIMIT {2}";

    public async Task<KetQuaPhanTrang<ThietBiChoTiepNhanSuaChuaDto>> XuLyAsync(
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
            return PhanTrang.Rong<ThietBiChoTiepNhanSuaChuaDto>(0, trang, kichThuoc);
        }

        var tongSo = await db.Database
            .SqlQuery<int>(FormattableStringFactory.Create(CauDem, dsMaPhong))
            .SingleAsync(ct);

        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<ThietBiChoTiepNhanSuaChuaDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await db.Database
            .SqlQuery<DongChoTiepNhanSuaChua>(FormattableStringFactory.Create(CauTrang, dsMaPhong, batDau, (long)kichThuoc))
            .ToListAsync(ct);

        var danhSach = cacDong
            .Select(d => new ThietBiChoTiepNhanSuaChuaDto(d.MaThietBi, d.TenThietBi, d.MaPhong, d.TenPhong, d.MaPhieuMuon))
            .ToList();

        return new KetQuaPhanTrang<ThietBiChoTiepNhanSuaChuaDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
