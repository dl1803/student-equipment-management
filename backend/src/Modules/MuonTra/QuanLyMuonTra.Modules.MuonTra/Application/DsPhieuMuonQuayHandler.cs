using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DsPhieuMuonQuayHandler(DbContext db, IQuyenPhongService quyenPhong)
{
    public async Task<KetQuaPhanTrang<PhieuMuonQuayTomTatDto>> XuLyAsync(
        Guid maNv,
        string? maSv,
        string? trangThai,
        Guid? maPhong,
        int trang,
        int kichThuoc,
        CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);
        KiemTraDocPhieuMuon.KiemTraTrangThai(trangThai);

        var dsMaPhong = (await quyenPhong.LayDsMaPhongAsync(maNv, ct)).ToList();
        if (maPhong is not null)
        {
            dsMaPhong = dsMaPhong.Where(p => p == maPhong.Value).ToList();
        }

        if (dsMaPhong.Count == 0)
        {
            return PhanTrang.Rong<PhieuMuonQuayTomTatDto>(0, trang, kichThuoc);
        }

        var goc = PhieuMuonTruyVan.DongPhieuTheoPhong(db, dsMaPhong.ToArray());
        var maSvChuan = maSv?.Trim();
        if (!string.IsNullOrEmpty(maSvChuan))
        {
            goc = goc.Where(d => d.MaSv == maSvChuan);
        }

        if (trangThai is not null)
        {
            goc = goc.Where(d => d.TrangThai == trangThai);
        }

        var (tongSo, dong) = await PhieuMuonTruyVan.LayTrangAsync(
            goc,
            q => q.OrderByDescending(d => d.NgayGioMuon).ThenBy(d => d.MaPhieu),
            trang,
            kichThuoc,
            ct);
        var theoMaSv = await PhieuMuonTruyVan.LaySinhVienAsync(db, dong.Select(d => d.MaSv).ToList(), ct);

        var danhSach = dong
            .Select(d => new PhieuMuonQuayTomTatDto(
                d.MaPhieu,
                d.MaSv,
                d.MaYeuCau,
                d.MaDangKy,
                d.NgayGioMuon,
                d.NgayGioHenTra,
                d.SoLanGiaHan,
                d.TrangThai,
                d.SoThietBi,
                PhieuMuonTruyVan.SinhVienHoacRong(theoMaSv, d.MaSv)))
            .ToList();

        return new KetQuaPhanTrang<PhieuMuonQuayTomTatDto>(tongSo, trang, kichThuoc, danhSach);
    }
}
