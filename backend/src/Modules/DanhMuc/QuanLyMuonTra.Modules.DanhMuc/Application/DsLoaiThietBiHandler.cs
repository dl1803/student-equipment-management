using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class DsLoaiThietBiHandler
{
    private readonly DbContext _db;

    public DsLoaiThietBiHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<KetQuaPhanTrang<LoaiThietBiQuanTriDto>> XuLyAsync(
        bool? ngungSuDung,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        IQueryable<LoaiThietBi> goc = _db.Set<LoaiThietBi>().AsNoTracking();
        if (ngungSuDung.HasValue)
        {
            var giaTri = ngungSuDung.Value;
            goc = goc.Where(l => l.NgungSuDung == giaTri);
        }

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<LoaiThietBiQuanTriDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await goc
            .OrderBy(l => l.TenLoai)
            .ThenBy(l => l.MaLoai)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .Select(l => new
            {
                l.MaLoai,
                l.TenLoai,
                l.MoTa,
                l.HangSanXuat,
                l.ChuKyBaoTri,
                l.PhiTreMoiNgay,
                l.NgungSuDung,
            })
            .ToListAsync(ct);

        var danhSach = cacDong.Select(l => new LoaiThietBiQuanTriDto(
            l.MaLoai,
            l.TenLoai,
            l.MoTa,
            l.HangSanXuat,
            l.ChuKyBaoTri,
            Convert.ToInt64(Math.Round(l.PhiTreMoiNgay)),
            l.NgungSuDung)).ToList();

        return new KetQuaPhanTrang<LoaiThietBiQuanTriDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
