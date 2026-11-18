using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Domain;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

public sealed class CapNhatThamSoHandler
{
    private readonly DbContext _db;

    public CapNhatThamSoHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<ThamSoHeThongDto> XuLyAsync(CapNhatThamSoRequest yeuCau, CancellationToken ct)
    {
        var thamSo = await _db.Set<ThamSoHeThong>().SingleOrDefaultAsync(t => t.Id == 1, ct)
            ?? throw new InvalidOperationException(
                "Thiếu dòng quy_dinh.tham_so_he_thong (id = 1). Chạy db/seed-required/quy-dinh-bat-buoc.sql.");

        thamSo.CapNhat(
            yeuCau.SoNgayKhoa,
            yeuCau.ThoiGianGiuChoPhut,
            yeuCau.AnHanPhut,
            yeuCau.SoGioGiaHanToiDa,
            yeuCau.NguongViPham,
            yeuCau.SoLanDangNhapSaiToiDa,
            yeuCau.GioMoCua,
            yeuCau.GioDongCua);
        await _db.SaveChangesAsync(ct);

        return new ThamSoHeThongDto(
            thamSo.SoNgayKhoa,
            thamSo.ThoiGianGiuChoPhut,
            thamSo.AnHanPhut,
            thamSo.SoGioGiaHanToiDa,
            thamSo.NguongViPham,
            thamSo.SoLanDangNhapSaiToiDa,
            thamSo.GioMoCua,
            thamSo.GioDongCua);
    }
}
