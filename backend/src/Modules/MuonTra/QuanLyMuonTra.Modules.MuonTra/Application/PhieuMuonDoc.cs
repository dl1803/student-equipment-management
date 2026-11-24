using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal sealed record DuLieuPhieuMuon(
    DongPhieuMuon Phieu,
    IReadOnlyList<DongChiTietPhieuMuon> DsChiTiet,
    IReadOnlyDictionary<string, ThietBiDto> ThietBi,
    IReadOnlyDictionary<Guid, string> TenLoai);

internal static class PhieuMuonDoc
{
    public static async Task<DuLieuPhieuMuon?> TaiAsync(
        DbContext db,
        IDanhMucReader danhMuc,
        string maPhieu,
        CancellationToken ct)
    {
        var phieu = await PhieuMuonTruyVan.DongPhieu(db).Where(d => d.MaPhieu == maPhieu).SingleOrDefaultAsync(ct);
        if (phieu is null)
        {
            return null;
        }

        var dsChiTiet = await PhieuMuonTruyVan.DongChiTiet(db)
            .Where(c => c.MaPhieu == maPhieu)
            .OrderBy(c => c.MaThietBi)
            .ToListAsync(ct);

        var thietBi = new Dictionary<string, ThietBiDto>();
        foreach (var maThietBi in dsChiTiet.Select(c => c.MaThietBi).Distinct())
        {
            thietBi[maThietBi] = await danhMuc.LayThietBiAsync(maThietBi, ct)
                ?? throw new InvalidOperationException($"Thiếu thiết bị {maThietBi} của phiếu mượn {maPhieu}.");
        }

        var tenLoai = new Dictionary<Guid, string>();
        foreach (var maLoai in thietBi.Values.Select(t => t.MaLoai).Distinct())
        {
            var loai = await danhMuc.LayLoaiAsync(maLoai, ct)
                ?? throw new InvalidOperationException($"Thiếu loại thiết bị {maLoai} của phiếu mượn {maPhieu}.");
            tenLoai[maLoai] = loai.TenLoai;
        }

        return new DuLieuPhieuMuon(phieu, dsChiTiet, thietBi, tenLoai);
    }

    public static async Task<PhieuMuonDayDuDto> DungAsync(DuLieuPhieuMuon du, IAnhService anhService, CancellationToken ct)
    {
        var dsChiTiet = new List<ChiTietPhieuMuonDayDuDto>(du.DsChiTiet.Count);
        foreach (var chiTiet in du.DsChiTiet)
        {
            var thietBi = du.ThietBi[chiTiet.MaThietBi];
            var maDoiTuong = chiTiet.MaChiTiet.ToString();
            var dsAnhBanDau = await anhService.LayDsAnhKyAsync(
                MaDoiTuongAnh.ChiTietPhieuMuon,
                maDoiTuong,
                MaGiaiDoanAnh.BanDau,
                ct);
            var dsAnhKhiTra = await anhService.LayDsAnhKyAsync(
                MaDoiTuongAnh.ChiTietPhieuMuon,
                maDoiTuong,
                MaGiaiDoanAnh.KhiTra,
                ct);

            dsChiTiet.Add(new ChiTietPhieuMuonDayDuDto(
                chiTiet.MaChiTiet,
                chiTiet.MaThietBi,
                thietBi.TenThietBi,
                thietBi.MaLoai,
                du.TenLoai[thietBi.MaLoai],
                chiTiet.TinhTrangBanDau,
                chiTiet.PhiTreMoiNgay,
                chiTiet.TinhTrangKhiTra,
                chiTiet.KetQuaKiemTra,
                chiTiet.NgayGioTraThucTe,
                chiTiet.MaKhung,
                dsAnhBanDau,
                dsAnhKhiTra));
        }

        return new PhieuMuonDayDuDto(
            du.Phieu.MaPhieu,
            du.Phieu.MaSv,
            du.Phieu.MaYeuCau,
            du.Phieu.MaDangKy,
            du.Phieu.NgayGioMuon,
            du.Phieu.NgayGioHenTra,
            du.Phieu.SoLanGiaHan,
            du.Phieu.TrangThai,
            dsChiTiet.Count,
            dsChiTiet);
    }
}
