using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

public sealed class DanhMucReader : IDanhMucReader
{
    private readonly DbContext _db;

    public DanhMucReader(DbContext db)
    {
        _db = db;
    }

    public async Task<ThietBiDto?> LayThietBiAsync(string maThietBi, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maThietBi))
        {
            return null;
        }

        var dong = await _db.Set<ThietBi>()
            .AsNoTracking()
            .Where(t => t.MaThietBi == maThietBi)
            .Select(t => new
            {
                t.MaThietBi,
                t.MaLoai,
                t.MaPhong,
                t.TenThietBi,
                t.NgayNhapKho,
                t.TinhTrang,
                t.NgungSuDung,
                t.ViTriKe,
                t.ViTriTu,
                GiaTri = _db.Set<LichSuGiaTri>()
                    .Where(g => g.MaThietBi == t.MaThietBi)
                    .OrderByDescending(g => g.NgayCapNhat)
                    .ThenByDescending(g => g.MaLichSu)
                    .Select(g => (decimal?)g.GiaTri)
                    .FirstOrDefault(),
            })
            .SingleOrDefaultAsync(ct);

        if (dong is null)
        {
            return null;
        }

        if (dong.GiaTri is null)
        {
            throw new InvalidOperationException($"Thiết bị {dong.MaThietBi} chưa có bản ghi lịch sử giá trị.");
        }

        return new ThietBiDto(
            dong.MaThietBi,
            dong.MaLoai,
            dong.MaPhong,
            dong.TenThietBi,
            dong.NgayNhapKho,
            TinhTrangThietBiMa.ToMa(dong.TinhTrang),
            dong.NgungSuDung,
            dong.ViTriKe,
            dong.ViTriTu,
            dong.GiaTri.Value);
    }

    public Task<LoaiThietBiDto?> LayLoaiAsync(Guid maLoai, CancellationToken ct)
    {
        return _db.Set<LoaiThietBi>()
            .AsNoTracking()
            .Where(l => l.MaLoai == maLoai)
            .Select(l => new LoaiThietBiDto(l.MaLoai, l.TenLoai, l.ChuKyBaoTri, l.PhiTreMoiNgay, l.NgungSuDung))
            .SingleOrDefaultAsync(ct);
    }

    public Task<PhongThietBiDto?> LayPhongAsync(Guid maPhong, CancellationToken ct)
    {
        return _db.Set<PhongThietBi>()
            .AsNoTracking()
            .Where(p => p.MaPhong == maPhong)
            .Select(p => new PhongThietBiDto(p.MaPhong, p.TenPhong, p.CoSo))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<PhongThietBiDto>> DsPhongAsync(CancellationToken ct)
    {
        return await _db.Set<PhongThietBi>()
            .AsNoTracking()
            .OrderBy(p => p.TenPhong)
            .ThenBy(p => p.MaPhong)
            .Select(p => new PhongThietBiDto(p.MaPhong, p.TenPhong, p.CoSo))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LoaiThietBiDto>> DsLoaiDangSuDungAsync(CancellationToken ct)
    {
        return await _db.Set<LoaiThietBi>()
            .AsNoTracking()
            .Where(l => !l.NgungSuDung)
            .OrderBy(l => l.TenLoai)
            .ThenBy(l => l.MaLoai)
            .Select(l => new LoaiThietBiDto(l.MaLoai, l.TenLoai, l.ChuKyBaoTri, l.PhiTreMoiNgay, l.NgungSuDung))
            .ToListAsync(ct);
    }
}
