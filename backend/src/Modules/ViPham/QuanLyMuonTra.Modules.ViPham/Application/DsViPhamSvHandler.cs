using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed class DsViPhamSvHandler(DbContext db)
{
    public async Task<KetQuaPhanTrang<ViPhamDto>> XuLyAsync(
        string maSv,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var goc = db.Database.SqlQuery<DongViPham>($"""
            SELECT ma_vi_pham AS "MaViPham",
                   loai_vi_pham AS "LoaiViPham",
                   thoi_diem_ghi_nhan AS "ThoiDiemGhiNhan",
                   hoc_ky AS "HocKy",
                   ma_bien_ban AS "MaBienBan",
                   ma_yeu_cau AS "MaYeuCau",
                   ma_dang_ky AS "MaDangKy"
            FROM vi_pham.vi_pham
            WHERE ma_sv = {maSv}
            """);

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<ViPhamDto>(tongSo, trang, kichThuoc);
        }

        var dong = await goc
            .OrderByDescending(d => d.ThoiDiemGhiNhan)
            .ThenBy(d => d.MaViPham)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        var danhSach = dong
            .Select(d => new ViPhamDto(d.MaViPham, d.LoaiViPham, d.ThoiDiemGhiNhan, d.HocKy, d.MaBienBan, d.MaYeuCau, d.MaDangKy))
            .ToList();

        return new KetQuaPhanTrang<ViPhamDto>(tongSo, trang, kichThuoc, danhSach);
    }

    public sealed class DongViPham
    {
        public Guid MaViPham { get; set; }

        public string LoaiViPham { get; set; } = string.Empty;

        public DateTimeOffset ThoiDiemGhiNhan { get; set; }

        public string HocKy { get; set; } = string.Empty;

        public string? MaBienBan { get; set; }

        public string? MaYeuCau { get; set; }

        public string? MaDangKy { get; set; }
    }
}

