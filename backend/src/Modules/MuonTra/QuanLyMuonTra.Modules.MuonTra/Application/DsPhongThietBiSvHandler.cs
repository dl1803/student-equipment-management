using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DsPhongThietBiSvHandler(DbContext db)
{
    public async Task<KetQuaPhanTrang<PhongThietBiDto>> XuLyAsync(int trang, int kichThuoc, CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var goc = db.Database.SqlQuery<DongPhongThietBi>($"""
            SELECT ma_phong AS "MaPhong", ten_phong AS "TenPhong", co_so AS "CoSo"
            FROM danh_muc.phong_thiet_bi
            """);

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<PhongThietBiDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await goc
            .OrderBy(d => d.TenPhong)
            .ThenBy(d => d.MaPhong)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        var danhSach = cacDong
            .Select(d => new PhongThietBiDto(d.MaPhong, d.TenPhong, d.CoSo))
            .ToList();

        return new KetQuaPhanTrang<PhongThietBiDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
