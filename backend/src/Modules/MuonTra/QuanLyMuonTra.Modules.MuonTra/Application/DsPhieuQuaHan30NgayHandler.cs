using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DsPhieuQuaHan30NgayHandler(
    DbContext db,
    IDanhMucReader danhMuc,
    IQuyenPhongService quyenPhong,
    IClock dongHo)
{
    public async Task<KetQuaPhanTrang<PhieuQuaHan30NgayDto>> XuLyAsync(
        Guid maNv,
        Guid? maPhong,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var dsMaPhong = (await quyenPhong.LayDsMaPhongAsync(maNv, ct)).ToList();
        if (maPhong is not null)
        {
            dsMaPhong = dsMaPhong.Where(p => p == maPhong.Value).ToList();
        }

        if (dsMaPhong.Count == 0)
        {
            return PhanTrang.Rong<PhieuQuaHan30NgayDto>(0, trang, kichThuoc);
        }

        var homNay = VnTime.NgayLich(dongHo.UtcNow);
        var goc = PhieuMuonTruyVan.DongQuaHan30Ngay(db, homNay, false, dsMaPhong.ToArray());
        var (tongSo, dong) = await PhieuMuonTruyVan.LayTrangAsync(
            goc,
            q => q.OrderBy(d => d.NgayGioHenTra).ThenBy(d => d.MaPhieu),
            trang,
            kichThuoc,
            ct);
        if (dong.Count == 0)
        {
            return PhanTrang.Rong<PhieuQuaHan30NgayDto>(tongSo, trang, kichThuoc);
        }

        var dsMaPhieu = dong.Select(d => d.MaPhieu).ToList();
        var dsChuaTra = await PhieuMuonTruyVan.DongChiTiet(db)
            .Where(c => dsMaPhieu.Contains(c.MaPhieu) && c.KetQuaKiemTra == null)
            .OrderBy(c => c.MaThietBi)
            .ToListAsync(ct);

        var thietBiTheoMa = new Dictionary<string, ThietBiDto>();
        foreach (var maThietBi in dsChuaTra.Select(c => c.MaThietBi).Distinct())
        {
            var thietBi = await danhMuc.LayThietBiAsync(maThietBi, ct)
                ?? throw new InvalidOperationException($"Thiếu thiết bị {maThietBi} của phiếu mượn quá hạn.");
            thietBiTheoMa[maThietBi] = thietBi;
        }

        var dsChuaTraHopLe = dsChuaTra
            .Where(c => thietBiTheoMa.TryGetValue(c.MaThietBi, out var tb) && dsMaPhong.Contains(tb.MaPhong))
            .ToList();

        var theoMaSv = await PhieuMuonTruyVan.LaySinhVienAsync(db, dong.Select(d => d.MaSv).ToList(), ct);

        var danhSach = dong
            .Select(d => new PhieuQuaHan30NgayDto(
                d.MaPhieu,
                PhieuMuonTruyVan.SinhVienHoacRong(theoMaSv, d.MaSv),
                d.NgayGioHenTra,
                homNay.DayNumber - VnTime.NgayLich(d.NgayGioHenTra).DayNumber,
                dsChuaTraHopLe
                    .Where(c => c.MaPhieu == d.MaPhieu)
                    .Select(c => new ThietBiTomTatDto(c.MaThietBi, thietBiTheoMa[c.MaThietBi].TenThietBi))
                    .ToList()))
            .ToList();

        return new KetQuaPhanTrang<PhieuQuaHan30NgayDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
