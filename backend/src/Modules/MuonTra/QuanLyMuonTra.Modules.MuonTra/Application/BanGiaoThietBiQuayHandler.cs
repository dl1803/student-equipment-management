using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class BanGiaoThietBiQuayHandler(
    DbContext db,
    IDanhMucReader danhMuc,
    IAnhService anhService,
    IQuyenPhongService quyenPhong)
{
    public async Task<BanGiaoThietBiDto> XuLyAsync(Guid maNv, string maThietBi, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maThietBi))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Mã thiết bị không được để trống.");
        }

        var thietBi = await danhMuc.LayThietBiAsync(maThietBi, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, $"Không tìm thấy thiết bị {maThietBi}.");

        var dsMaPhongDuocCap = (await quyenPhong.LayDsMaPhongAsync(maNv, ct)).ToHashSet();
        if (!dsMaPhongDuocCap.Contains(thietBi.MaPhong))
        {
            throw new LoiNghiepVuException(
                MaLoi.PhongChuaCapQuyen,
                "Nhân viên chưa được cấp quyền trên phòng của thiết bị này.");
        }

        var banGiao = await BanGiaoTruyVan.TimChiTietDangHieuLucAsync(db, maThietBi, ct)
            ?? throw new LoiNghiepVuException(
                MaLoi.ThietBiKhongThuocPhieu,
                $"Thiết bị {maThietBi} không thuộc phiếu mượn đang hiệu lực.");

        var sinhVien = await PhieuMuonTruyVan.LaySinhVienAsync(db, [banGiao.MaSv], ct);
        var dsAnhBanDau = await anhService.LayDsAnhKyAsync(
            MaDoiTuongAnh.ChiTietPhieuMuon,
            banGiao.MaChiTiet.ToString(),
            MaGiaiDoanAnh.BanDau,
            ct);

        return new BanGiaoThietBiDto(
            thietBi.MaThietBi,
            thietBi.TenThietBi,
            banGiao.MaPhieu,
            PhieuMuonTruyVan.SinhVienHoacRong(sinhVien, banGiao.MaSv),
            banGiao.NgayGioMuon,
            banGiao.NgayGioHenTra,
            banGiao.TinhTrangBanDau,
            dsAnhBanDau,
            null);
    }
}
