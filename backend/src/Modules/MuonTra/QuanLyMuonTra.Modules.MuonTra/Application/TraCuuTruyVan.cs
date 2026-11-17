using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class TraCuuTruyVan
{
    public static IQueryable<DongTraCuuThietBi> LayCapLoaiPhong(DbContext db, DateTimeOffset thoiDiem)
    {
        return db.Database.SqlQuery<DongTraCuuThietBi>($"""
            SELECT l.ma_loai AS "MaLoai",
                   l.ten_loai AS "TenLoai",
                   l.hang_san_xuat AS "HangSanXuat",
                   l.mo_ta AS "MoTa",
                   p.ma_phong AS "MaPhong",
                   p.ten_phong AS "TenPhong",
                   p.co_so AS "CoSo",
                   GREATEST(0, COALESCE(ss.so_luong, 0) - COALESCE(gc.so_luong, 0) - COALESCE(dk.so_luong, 0))::integer AS "SoLuongKhaDung"
            FROM danh_muc.loai_thiet_bi l
            JOIN danh_muc.phong_thiet_bi p
              ON EXISTS (SELECT 1 FROM danh_muc.thiet_bi tb WHERE tb.ma_loai = l.ma_loai AND tb.ma_phong = p.ma_phong)
            LEFT JOIN (
                SELECT t.ma_phong, t.ma_loai, count(*) AS so_luong
                FROM danh_muc.thiet_bi t
                WHERE t.tinh_trang = 'SAN_SANG' AND t.ngung_su_dung = false
                GROUP BY t.ma_phong, t.ma_loai
            ) ss ON ss.ma_phong = p.ma_phong AND ss.ma_loai = l.ma_loai
            LEFT JOIN (
                SELECT y.ma_phong, c.ma_loai, sum(c.so_luong) AS so_luong
                FROM muon_tra.phieu_yeu_cau_muon y
                JOIN muon_tra.chi_tiet_yeu_cau_muon c ON c.ma_yeu_cau = y.ma_yeu_cau
                WHERE y.trang_thai = 'DA_DUYET' AND y.han_giu_cho > {thoiDiem}
                GROUP BY y.ma_phong, c.ma_loai
            ) gc ON gc.ma_phong = p.ma_phong AND gc.ma_loai = l.ma_loai
            LEFT JOIN (
                SELECT d.ma_phong, d.ma_loai, count(*) AS so_luong
                FROM muon_tra.dang_ky_cho d
                WHERE d.trang_thai = 'DA_THONG_BAO' AND d.han_giu_cho > {thoiDiem}
                GROUP BY d.ma_phong, d.ma_loai
            ) dk ON dk.ma_phong = p.ma_phong AND dk.ma_loai = l.ma_loai
            WHERE l.ngung_su_dung = false
            """);
    }

    public static IQueryable<DongTraCuuThietBi> ApDungBoLoc(
        IQueryable<DongTraCuuThietBi> goc,
        string? ten,
        Guid? maLoai,
        Guid? maPhong,
        bool? conHang)
    {
        if (!string.IsNullOrWhiteSpace(ten))
        {
            var tuKhoa = ten.Trim().ToLowerInvariant();
            goc = goc.Where(d => d.TenLoai.ToLower().Contains(tuKhoa));
        }

        if (maLoai.HasValue)
        {
            var loai = maLoai.Value;
            goc = goc.Where(d => d.MaLoai == loai);
        }

        if (maPhong.HasValue)
        {
            var phong = maPhong.Value;
            goc = goc.Where(d => d.MaPhong == phong);
        }

        if (conHang == true)
        {
            goc = goc.Where(d => d.SoLuongKhaDung > 0);
        }
        else if (conHang == false)
        {
            goc = goc.Where(d => d.SoLuongKhaDung == 0);
        }

        return goc;
    }

    public static async Task<KetQuaPhanTrang<TraCuuThietBiItemDto>> LayTrangAsync(
        IQueryable<DongTraCuuThietBi> goc,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<TraCuuThietBiItemDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await goc
            .OrderBy(d => d.TenLoai)
            .ThenBy(d => d.TenPhong)
            .ThenBy(d => d.MaLoai)
            .ThenBy(d => d.MaPhong)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        var danhSach = cacDong
            .Select(d => new TraCuuThietBiItemDto(
                d.MaLoai,
                d.TenLoai,
                d.HangSanXuat,
                d.MaPhong,
                d.TenPhong,
                d.CoSo,
                d.SoLuongKhaDung))
            .ToList();

        return new KetQuaPhanTrang<TraCuuThietBiItemDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
