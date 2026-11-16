using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Domain;

namespace QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

public sealed class QuyDinhReader : IQuyDinhReader
{
    private readonly DbContext _db;

    public QuyDinhReader(DbContext db)
    {
        _db = db;
    }

    public async Task<QuyDinhNhomDto?> LayQuyDinhNhomAsync(string maNhom, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maNhom))
        {
            return null;
        }

        var cacDong = await _db.Set<QuyDinhMuon>()
            .AsNoTracking()
            .Where(q => q.MaNhom == maNhom)
            .Select(q => new { q.MaNhom, q.MaLoai, q.SoThietBiToiDa, q.ThoiGianMuonToiDaGio })
            .ToListAsync(ct);

        var chung = cacDong.FirstOrDefault(q => q.MaLoai is null);
        if (chung is null)
        {
            return null;
        }

        if (chung.SoThietBiToiDa is null)
        {
            throw new InvalidOperationException($"Quy định chung của nhóm {chung.MaNhom} thiếu số thiết bị tối đa.");
        }

        var dsNgoaiLe = cacDong
            .Where(q => q.MaLoai.HasValue)
            .OrderBy(q => q.MaLoai)
            .Select(q => new NgoaiLeTheoLoaiDto(q.MaLoai.GetValueOrDefault(), q.ThoiGianMuonToiDaGio))
            .ToList();

        return new QuyDinhNhomDto(chung.MaNhom, chung.SoThietBiToiDa.Value, chung.ThoiGianMuonToiDaGio, dsNgoaiLe);
    }

    public async Task<ThamSoHeThongDto> LayThamSoAsync(CancellationToken ct)
    {
        var thamSo = await _db.Set<ThamSoHeThong>()
            .AsNoTracking()
            .Where(t => t.Id == 1)
            .Select(t => new ThamSoHeThongDto(
                t.SoNgayKhoa,
                t.ThoiGianGiuChoPhut,
                t.AnHanPhut,
                t.SoGioGiaHanToiDa,
                t.NguongViPham,
                t.SoLanDangNhapSaiToiDa,
                t.GioMoCua,
                t.GioDongCua))
            .SingleOrDefaultAsync(ct);

        return thamSo ?? throw new InvalidOperationException(
            "Thiếu dòng quy_dinh.tham_so_he_thong (id = 1). Chạy db/seed-required/quy-dinh-bat-buoc.sql.");
    }

    public Task<KhungPhatDto?> LayKhungPhatAsync(Guid maKhung, CancellationToken ct)
    {
        return _db.Set<KhungPhat>()
            .AsNoTracking()
            .Where(k => k.MaKhung == maKhung)
            .Select(k => new KhungPhatDto(k.MaKhung, k.TenKhung, k.TyLePhanTram, k.DangSuDung))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<KhungPhatDto>> DsKhungPhatDangSuDungAsync(CancellationToken ct)
    {
        return await _db.Set<KhungPhat>()
            .AsNoTracking()
            .Where(k => k.DangSuDung)
            .OrderBy(k => k.TyLePhanTram)
            .ThenBy(k => k.MaKhung)
            .Select(k => new KhungPhatDto(k.MaKhung, k.TenKhung, k.TyLePhanTram, k.DangSuDung))
            .ToListAsync(ct);
    }
}
