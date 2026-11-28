using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Application;

internal static class BienBanTruyVan
{
    public static IQueryable<DongBienBan> TruyVanBienBan(DbContext db)
    {
        return db.Database.SqlQuery<DongBienBan>($"""
            SELECT bb.ma_bien_ban AS "MaBienBan",
                   bb.ma_phieu_muon AS "MaPhieuMuon",
                   p.ma_sv AS "MaSv",
                   bb.ngay_lap AS "NgayLap",
                   bb.tong_tien::bigint AS "TongTien",
                   bb.trang_thai AS "TrangThai",
                   bb.thoi_diem_nop AS "ThoiDiemNop"
            FROM vi_pham.bien_ban_vi_pham bb
            JOIN muon_tra.phieu_muon p ON p.ma_phieu = bb.ma_phieu_muon
            """);
    }

    public static IQueryable<DongBienBan> TruyVanBienBanTheoPhong(DbContext db, Guid[] dsMaPhong)
    {
        return db.Database.SqlQuery<DongBienBan>($"""
            SELECT bb.ma_bien_ban AS "MaBienBan",
                   bb.ma_phieu_muon AS "MaPhieuMuon",
                   p.ma_sv AS "MaSv",
                   bb.ngay_lap AS "NgayLap",
                   bb.tong_tien::bigint AS "TongTien",
                   bb.trang_thai AS "TrangThai",
                   bb.thoi_diem_nop AS "ThoiDiemNop"
            FROM vi_pham.bien_ban_vi_pham bb
            JOIN muon_tra.phieu_muon p ON p.ma_phieu = bb.ma_phieu_muon
            WHERE EXISTS (
                SELECT 1
                FROM muon_tra.chi_tiet_phieu_muon c
                JOIN danh_muc.thiet_bi t ON t.ma_thiet_bi = c.ma_thiet_bi
                WHERE c.ma_phieu = bb.ma_phieu_muon
                  AND t.ma_phong = ANY({dsMaPhong}))
            """);
    }

    public static async Task<KetQuaPhanTrang<BienBanViPhamDto>> LayTrangAsync(
        DbContext db,
        IQueryable<DongBienBan> goc,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<BienBanViPhamDto>(tongSo, trang, kichThuoc);
        }

        var dong = await goc
            .OrderByDescending(d => d.NgayLap)
            .ThenBy(d => d.MaBienBan)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        var dsMaBienBan = dong.Select(d => d.MaBienBan).ToList();
        var dsChiTiet = await db.Database.SqlQuery<DongChiTiet>($"""
                SELECT ma_bien_ban AS "MaBienBan",
                       ma_thiet_bi AS "MaThietBi",
                       loai_vi_pham AS "LoaiViPham",
                       so_ngay_tre AS "SoNgayTre",
                       so_tien::bigint AS "SoTien",
                       ma_khung AS "MaKhung"
                FROM vi_pham.chi_tiet_bien_ban
                """)
            .Where(c => dsMaBienBan.Contains(c.MaBienBan))
            .OrderBy(c => c.MaThietBi)
            .ThenBy(c => c.LoaiViPham)
            .ToListAsync(ct);

        var chiTietTheoBienBan = dsChiTiet.ToLookup(c => c.MaBienBan);
        var danhSach = dong
            .Select(d => new BienBanViPhamDto(
                d.MaBienBan,
                d.MaPhieuMuon,
                d.MaSv,
                d.NgayLap,
                d.TongTien,
                d.TrangThai,
                d.ThoiDiemNop,
                chiTietTheoBienBan[d.MaBienBan]
                    .Select(c => new ChiTietBienBanDto(c.MaThietBi, c.LoaiViPham, c.SoNgayTre, c.SoTien, c.MaKhung))
                    .ToList()))
            .ToList();

        return new KetQuaPhanTrang<BienBanViPhamDto>(tongSo, trang, kichThuoc, danhSach);
    }

    public static async Task<DongBienBan> LayDongAsync(DbContext db, string maBienBan, CancellationToken ct)
    {
        return await TruyVanBienBan(db)
                .Where(d => d.MaBienBan == maBienBan)
                .FirstOrDefaultAsync(ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy biên bản vi phạm.");
    }

    public static async Task<BienBanViPhamDto> LayMotAsync(DbContext db, string maBienBan, CancellationToken ct)
    {
        var trang = await LayTrangAsync(db, TruyVanBienBan(db).Where(d => d.MaBienBan == maBienBan), 1, 1, ct);

        return trang.DanhSach.SingleOrDefault()
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy biên bản vi phạm.");
    }

    public sealed class DongBienBan
    {
        public string MaBienBan { get; set; } = string.Empty;

        public string MaPhieuMuon { get; set; } = string.Empty;

        public string MaSv { get; set; } = string.Empty;

        public DateTimeOffset NgayLap { get; set; }

        public long TongTien { get; set; }

        public string TrangThai { get; set; } = string.Empty;

        public DateTimeOffset? ThoiDiemNop { get; set; }
    }

    public sealed class DongChiTiet
    {
        public string MaBienBan { get; set; } = string.Empty;

        public string MaThietBi { get; set; } = string.Empty;

        public string LoaiViPham { get; set; } = string.Empty;

        public int? SoNgayTre { get; set; }

        public long SoTien { get; set; }

        public Guid? MaKhung { get; set; }
    }
}

