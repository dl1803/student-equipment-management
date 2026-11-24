using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DsLoaiThietBiSvHandler(DbContext db)
{
    public async Task<KetQuaPhanTrang<LoaiThietBiTomTatDto>> XuLyAsync(int trang, int kichThuoc, CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var goc = db.Database.SqlQuery<DongLoaiThietBi>($"""
            SELECT ma_loai AS "MaLoai", ten_loai AS "TenLoai", mo_ta AS "MoTa", hang_san_xuat AS "HangSanXuat"
            FROM danh_muc.loai_thiet_bi
            WHERE ngung_su_dung = false
            """);

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<LoaiThietBiTomTatDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await goc
            .OrderBy(d => d.TenLoai)
            .ThenBy(d => d.MaLoai)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        var danhSach = cacDong
            .Select(d => new LoaiThietBiTomTatDto(d.MaLoai, d.TenLoai, d.MoTa, d.HangSanXuat))
            .ToList();

        return new KetQuaPhanTrang<LoaiThietBiTomTatDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
