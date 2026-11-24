using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class PhieuMuonTruyVan
{
    public static IQueryable<DongPhieuMuon> DongPhieu(DbContext db)
    {
        return db.Database.SqlQuery<DongPhieuMuon>($"""
            SELECT p.ma_phieu AS "MaPhieu", p.ma_sv AS "MaSv", p.ma_nv_lap AS "MaNvLap",
                   p.ma_yeu_cau AS "MaYeuCau", p.ma_dang_ky AS "MaDangKy",
                   p.ngay_gio_muon AS "NgayGioMuon", p.ngay_gio_hen_tra AS "NgayGioHenTra",
                   p.so_lan_gia_han AS "SoLanGiaHan", p.trang_thai AS "TrangThai",
                   (SELECT count(*)::integer FROM muon_tra.chi_tiet_phieu_muon c WHERE c.ma_phieu = p.ma_phieu) AS "SoThietBi"
            FROM muon_tra.phieu_muon p
            """);
    }

    public static IQueryable<DongChiTietPhieuMuon> DongChiTiet(DbContext db)
    {
        return db.Database.SqlQuery<DongChiTietPhieuMuon>($"""
            SELECT ma_chi_tiet AS "MaChiTiet", ma_phieu AS "MaPhieu", ma_thiet_bi AS "MaThietBi",
                   tinh_trang_ban_dau AS "TinhTrangBanDau", phi_tre_moi_ngay::bigint AS "PhiTreMoiNgay",
                   tinh_trang_khi_tra AS "TinhTrangKhiTra", ket_qua_kiem_tra AS "KetQuaKiemTra",
                   ngay_gio_tra_thuc_te AS "NgayGioTraThucTe", ma_khung AS "MaKhung"
            FROM muon_tra.chi_tiet_phieu_muon
            """);
    }

    public static async Task<(int TongSo, List<T> Dong)> LayTrangAsync<T>(
        IQueryable<T> goc,
        Func<IQueryable<T>, IOrderedQueryable<T>> sapXep,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return (tongSo, new List<T>());
        }

        var dong = await sapXep(goc).Skip((int)batDau).Take(kichThuoc).ToListAsync(ct);

        return (tongSo, dong);
    }

    public static PhieuMuonTomTatDto ToTomTat(DongPhieuMuon d)
    {
        return new PhieuMuonTomTatDto(
            d.MaPhieu,
            d.MaSv,
            d.MaYeuCau,
            d.MaDangKy,
            d.NgayGioMuon,
            d.NgayGioHenTra,
            d.SoLanGiaHan,
            d.TrangThai,
            d.SoThietBi);
    }

    public static IQueryable<DongPhieuMuon> DongPhieuTheoPhong(DbContext db, Guid[] dsMaPhong)
    {
        return db.Database.SqlQuery<DongPhieuMuon>($"""
            SELECT p.ma_phieu AS "MaPhieu", p.ma_sv AS "MaSv", p.ma_nv_lap AS "MaNvLap",
                   p.ma_yeu_cau AS "MaYeuCau", p.ma_dang_ky AS "MaDangKy",
                   p.ngay_gio_muon AS "NgayGioMuon", p.ngay_gio_hen_tra AS "NgayGioHenTra",
                   p.so_lan_gia_han AS "SoLanGiaHan", p.trang_thai AS "TrangThai",
                   (SELECT count(*)::integer FROM muon_tra.chi_tiet_phieu_muon c WHERE c.ma_phieu = p.ma_phieu) AS "SoThietBi"
            FROM muon_tra.phieu_muon p
            WHERE EXISTS (
                SELECT 1
                FROM muon_tra.chi_tiet_phieu_muon c
                JOIN danh_muc.thiet_bi t ON t.ma_thiet_bi = c.ma_thiet_bi
                WHERE c.ma_phieu = p.ma_phieu AND t.ma_phong = ANY({dsMaPhong}))
            """);
    }

    public static async Task<IReadOnlyDictionary<string, SinhVienTomTatDto>> LaySinhVienAsync(
        DbContext db,
        IReadOnlyCollection<string> dsMaSv,
        CancellationToken ct)
    {
        if (dsMaSv.Count == 0)
        {
            return new Dictionary<string, SinhVienTomTatDto>();
        }

        var dsMa = dsMaSv.Distinct().ToList();
        var cacSv = await db.Database.SqlQuery<DongSinhVienTomTat>($"""
                SELECT ma_sv AS "MaSv", ho_ten AS "HoTen", lop AS "Lop"
                FROM sinh_vien.sinh_vien
                """)
            .Where(s => dsMa.Contains(s.MaSv))
            .ToListAsync(ct);

        return cacSv.ToDictionary(s => s.MaSv, s => new SinhVienTomTatDto(s.MaSv, s.HoTen, s.Lop));
    }

    public static SinhVienTomTatDto SinhVienHoacRong(IReadOnlyDictionary<string, SinhVienTomTatDto> theoMa, string maSv)
    {
        return theoMa.TryGetValue(maSv, out var sv) ? sv : new SinhVienTomTatDto(maSv, string.Empty, null);
    }

    public static IQueryable<DongPhieuMuon> DongQuaHan30Ngay(
        DbContext db,
        DateOnly homNay,
        bool khongLocPhong,
        Guid[] dsMaPhong)
    {
        return db.Database.SqlQuery<DongPhieuMuon>($"""
            SELECT p.ma_phieu AS "MaPhieu", p.ma_sv AS "MaSv", p.ma_nv_lap AS "MaNvLap",
                   p.ma_yeu_cau AS "MaYeuCau", p.ma_dang_ky AS "MaDangKy",
                   p.ngay_gio_muon AS "NgayGioMuon", p.ngay_gio_hen_tra AS "NgayGioHenTra",
                   p.so_lan_gia_han AS "SoLanGiaHan", p.trang_thai AS "TrangThai",
                   0 AS "SoThietBi"
            FROM muon_tra.phieu_muon p
            WHERE ({homNay}::date - (p.ngay_gio_hen_tra AT TIME ZONE 'Asia/Ho_Chi_Minh')::date) > 30
              AND EXISTS (
                  SELECT 1
                  FROM muon_tra.chi_tiet_phieu_muon c
                  JOIN danh_muc.thiet_bi t ON t.ma_thiet_bi = c.ma_thiet_bi
                  WHERE c.ma_phieu = p.ma_phieu
                    AND c.ket_qua_kiem_tra IS NULL
                    AND ({khongLocPhong} OR t.ma_phong = ANY({dsMaPhong})))
            """);
    }
}
