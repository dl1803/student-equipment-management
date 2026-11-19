using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

internal static class DocThietBiQuanTri
{
    internal static async Task<ThietBiQuanTriDto> LayAsync(DbContext db, string maThietBi, CancellationToken ct)
    {
        var d = await (
            from t in db.Set<ThietBi>().AsNoTracking()
            join l in db.Set<LoaiThietBi>() on t.MaLoai equals l.MaLoai
            join p in db.Set<PhongThietBi>() on t.MaPhong equals p.MaPhong
            where t.MaThietBi == maThietBi
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
                GiaTri = db.Set<LichSuGiaTri>()
                    .Where(g => g.MaThietBi == t.MaThietBi)
                    .OrderByDescending(g => g.NgayCapNhat)
                    .ThenByDescending(g => g.MaLichSu)
                    .Select(g => (decimal?)g.GiaTri)
                    .FirstOrDefault(),
            })
            .SingleOrDefaultAsync(ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy thiết bị.");

        var dsNcc = await (
            from g in db.Set<ThietBiNhaCungCap>().AsNoTracking()
            join n in db.Set<NhaCungCap>() on g.MaNCC equals n.MaNCC
            where g.MaThietBi == maThietBi
            orderby n.TenNCC, n.MaNCC
            select new NhaCungCapQuanTriDto(n.MaNCC, n.TenNCC, n.ThongTinLienHe))
            .ToListAsync(ct);

        return new ThietBiQuanTriDto(
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
            dsNcc);
    }
}
