using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

public sealed class PhieuMuonReader(DbContext db, IClock dongHo) : IPhieuMuonReader
{
    public async Task<PhieuMuonDto?> LayPhieuMuonAsync(string maPhieu, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maPhieu))
        {
            return null;
        }

        var phieu = await db.Set<PhieuMuon>()
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.MaPhieu == maPhieu, ct);

        return phieu is null ? null : ToDto(phieu);
    }

    public async Task<ChiTietPhieuMuonDto?> LayChiTietAsync(Guid maChiTiet, CancellationToken ct)
    {
        var chiTiet = await db.Set<ChiTietPhieuMuon>()
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.MaChiTiet == maChiTiet, ct);

        return chiTiet is null ? null : ToDto(chiTiet);
    }

    public async Task<IReadOnlyList<ChiTietPhieuMuonDto>> LayChiTietTheoPhieuAsync(string maPhieu, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maPhieu))
        {
            return [];
        }

        var dsChiTiet = await db.Set<ChiTietPhieuMuon>()
            .AsNoTracking()
            .Where(c => c.MaPhieu == maPhieu)
            .OrderBy(c => c.MaThietBi)
            .ToListAsync(ct);

        return dsChiTiet.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<PhieuMuonDto>> LayDsQuaHan30NgayAsync(CancellationToken ct)
    {
        var homNay = VnTime.NgayLich(dongHo.UtcNow);
        var dong = await PhieuMuonTruyVan.DongQuaHan30Ngay(db, homNay, true, [])
            .OrderBy(d => d.NgayGioHenTra)
            .ThenBy(d => d.MaPhieu)
            .ToListAsync(ct);

        return dong
            .Select(d => new PhieuMuonDto(d.MaPhieu, d.MaSv, d.MaNvLap, d.NgayGioMuon, d.NgayGioHenTra, d.TrangThai))
            .ToList();
    }

    private static PhieuMuonDto ToDto(PhieuMuon phieu)
    {
        return new PhieuMuonDto(
            phieu.MaPhieu,
            phieu.MaSv,
            phieu.MaNvLap,
            phieu.NgayGioMuon,
            phieu.NgayGioHenTra,
            MaTrangThaiConverter<TrangThaiPhieuMuon>.ChuyenSangMa(phieu.TrangThai));
    }

    private static ChiTietPhieuMuonDto ToDto(ChiTietPhieuMuon chiTiet)
    {
        return new ChiTietPhieuMuonDto(
            chiTiet.MaChiTiet,
            chiTiet.MaPhieu,
            chiTiet.MaThietBi,
            chiTiet.PhiTreMoiNgay,
            chiTiet.TinhTrangBanDau,
            chiTiet.KetQuaKiemTra is null ? null : MaTrangThaiConverter<KetQuaKiemTra>.ChuyenSangMa(chiTiet.KetQuaKiemTra.Value),
            chiTiet.TinhTrangKhiTra,
            chiTiet.NgayGioTraThucTe,
            chiTiet.MaKhung);
    }
}
