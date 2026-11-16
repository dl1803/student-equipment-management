using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class DsThietBiHandler
{
    private readonly DbContext _db;

    public DsThietBiHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<KetQuaPhanTrang<ThietBiQuanTriDto>> XuLyAsync(
        string? ten,
        Guid? maLoai,
        Guid? maPhong,
        string? tinhTrang,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);
        var tinhTrangLoc = ChuyenTinhTrang(tinhTrang);

        IQueryable<ThietBi> goc = _db.Set<ThietBi>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(ten))
        {
            var mau = ten.Trim().ToLower();
            goc = goc.Where(t => t.TenThietBi.ToLower().Contains(mau));
        }

        if (maLoai.HasValue)
        {
            var loai = maLoai.Value;
            goc = goc.Where(t => t.MaLoai == loai);
        }

        if (maPhong.HasValue)
        {
            var phong = maPhong.Value;
            goc = goc.Where(t => t.MaPhong == phong);
        }

        if (tinhTrangLoc.HasValue)
        {
            var tt = tinhTrangLoc.Value;
            goc = goc.Where(t => t.TinhTrang == tt);
        }

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<ThietBiQuanTriDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await (
            from t in goc
            join l in _db.Set<LoaiThietBi>() on t.MaLoai equals l.MaLoai
            join p in _db.Set<PhongThietBi>() on t.MaPhong equals p.MaPhong
            orderby t.MaThietBi
            select new
            {
                t.MaThietBi,
                t.MaLoai,
                l.TenLoai,
                t.MaPhong,
                p.TenPhong,
                t.TenThietBi,
                t.MoTa,
                t.ThongSoKyThuat,
                t.NgayNhapKho,
                t.ThoiHanBaoHanh,
                t.TinhTrang,
                t.ViTriKe,
                t.ViTriTu,
                t.NgungSuDung,
                GiaTri = _db.Set<LichSuGiaTri>()
                    .Where(g => g.MaThietBi == t.MaThietBi)
                    .OrderByDescending(g => g.NgayCapNhat)
                    .ThenByDescending(g => g.MaLichSu)
                    .Select(g => (decimal?)g.GiaTri)
                    .FirstOrDefault(),
            })
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        var dsMa = cacDong.Select(d => d.MaThietBi).ToList();
        var cacNcc = await (
            from g in _db.Set<ThietBiNhaCungCap>().AsNoTracking()
            join n in _db.Set<NhaCungCap>() on g.MaNCC equals n.MaNCC
            where dsMa.Contains(g.MaThietBi)
            orderby n.TenNCC, n.MaNCC
            select new { g.MaThietBi, n.MaNCC, n.TenNCC, n.ThongTinLienHe })
            .ToListAsync(ct);
        var nccTheoThietBi = cacNcc.ToLookup(x => x.MaThietBi);

        var danhSach = cacDong.Select(d => new ThietBiQuanTriDto(
            d.MaThietBi,
            d.MaLoai,
            d.TenLoai,
            d.MaPhong,
            d.TenPhong,
            d.TenThietBi,
            d.MoTa,
            d.ThongSoKyThuat,
            d.NgayNhapKho,
            d.ThoiHanBaoHanh,
            TinhTrangThietBiMa.ToMa(d.TinhTrang),
            d.ViTriKe,
            d.ViTriTu,
            d.NgungSuDung,
            (long)(d.GiaTri ?? 0m),
            nccTheoThietBi[d.MaThietBi]
                .Select(n => new NhaCungCapQuanTriDto(n.MaNCC, n.TenNCC, n.ThongTinLienHe))
                .ToList())).ToList();

        return new KetQuaPhanTrang<ThietBiQuanTriDto>(tongSo, trang, kichThuoc, danhSach);
    }

    private static TinhTrangThietBi? ChuyenTinhTrang(string? tinhTrang)
    {
        if (tinhTrang is null)
        {
            return null;
        }

        try
        {
            return TinhTrangThietBiMa.ToTinhTrang(tinhTrang);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Tình trạng thiết bị không hợp lệ.");
        }
    }
}
