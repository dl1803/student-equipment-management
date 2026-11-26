using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.BaoTri.Domain;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class LapBaoTriHandler(
    DbContext db,
    IClock dongHo,
    IDanhMucReader danhMuc,
    IThietBiService thietBiService,
    IAdvisoryLockService khoa,
    IAvailabilityService khaDung,
    IQuyenPhongService quyenPhong,
    IPhieuMuonReader phieuMuon)
{
    public async Task<BaoTriSuaChuaDto> XuLyAsync(Guid maNv, LapBaoTriRequest yeuCau, CancellationToken ct)
    {
        var maThietBi = yeuCau.MaThietBi?.Trim() ?? string.Empty;
        var maPhieu = yeuCau.MaPhieuMuon?.Trim();
        if (!LoaiThucHienBaoTriMa.TryParse(yeuCau.LoaiThucHien, out var loai))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Loại thực hiện không hợp lệ.");
        }

        BaoTriSuaChua.KiemTraYeuCau(maThietBi, loai, maPhieu);

        var thietBiTruoc = await danhMuc.LayThietBiAsync(maThietBi, ct) ?? throw KhongTonTai();
        await quyenPhong.EnsureAsync(maNv, thietBiTruoc.MaPhong, ct);

        var giaoDichNgoai = db.Database.CurrentTransaction;
        await using var giaoDichRieng = giaoDichNgoai is null ? await db.Database.BeginTransactionAsync(ct) : null;

        await khoa.LockPhongLoaiAsync(thietBiTruoc.MaPhong, thietBiTruoc.MaLoai, ct);

        var thietBi = await danhMuc.LayThietBiAsync(maThietBi, ct) ?? throw KhongTonTai();
        if (thietBi.MaPhong != thietBiTruoc.MaPhong || thietBi.MaLoai != thietBiTruoc.MaLoai)
        {
            throw new LoiNghiepVuException("XUNG_DOT_CAP_NHAT", "Thiết bị vừa được thay đổi bởi giao dịch khác. Vui lòng tải lại.");
        }

        var daCoBanGhiMo = await db.Set<BaoTriSuaChua>()
            .AsNoTracking()
            .AnyAsync(b => b.MaThietBi == maThietBi && b.KetQua == null, ct);
        if (daCoBanGhiMo)
        {
            throw TrangThaiKhongHopLe("Thiết bị đã có hồ sơ bảo trì/sửa chữa đang mở.");
        }

        if (loai == LoaiThucHienBaoTri.BaoTriDinhKy)
        {
            await ChuyenSangBaoTriAsync(thietBi, ct);
        }
        else
        {
            await KiemTraTiepNhanSuaChuaAsync(thietBi, maPhieu, ct);
        }

        var banGhi = BaoTriSuaChua.Lap(maThietBi, maNv, loai, maPhieu, dongHo.UtcNow);
        db.Set<BaoTriSuaChua>().Add(banGhi);
        await db.SaveChangesAsync(ct);

        if (giaoDichRieng is not null)
        {
            await giaoDichRieng.CommitAsync(ct);
        }

        return new BaoTriSuaChuaDto(
            banGhi.MaBaoTri,
            banGhi.MaThietBi,
            thietBi.TenThietBi,
            banGhi.MaNvThucHien,
            banGhi.MaPhieuMuon,
            LoaiThucHienBaoTriMa.ToMa(banGhi.LoaiThucHien),
            banGhi.NgayLap,
            banGhi.NoiDungThucHien,
            banGhi.KetQua,
            banGhi.NgayHoanThanh);
    }

    private async Task ChuyenSangBaoTriAsync(ThietBiDto thietBi, CancellationToken ct)
    {
        if (thietBi.TinhTrang != MaTinhTrangThietBi.SanSang)
        {
            throw TrangThaiKhongHopLe("Chỉ lập bảo trì định kỳ cho thiết bị đang sẵn sàng; thiết bị đang được mượn thì lập lịch sau khi được trả.");
        }

        if (!thietBi.NgungSuDung)
        {
            var soLuongKhaDung = await khaDung.TinhKhaDungAsync(thietBi.MaPhong, thietBi.MaLoai, ct);
            if (soLuongKhaDung <= 0)
            {
                throw new LoiNghiepVuException(
                    "SO_LUONG_KHA_DUNG_THAY_DOI",
                    "Thiết bị đang thuộc hạn ngạch giữ chỗ của phiếu mượn hoặc đăng ký chờ, chưa thể lập bảo trì.");
            }
        }

        if (!await thietBiService.ChuyenBaoTriAsync(thietBi.MaThietBi, ct))
        {
            throw TrangThaiKhongHopLe("Thiết bị không còn ở trạng thái sẵn sàng.");
        }
    }

    private async Task KiemTraTiepNhanSuaChuaAsync(ThietBiDto thietBi, string? maPhieu, CancellationToken ct)
    {
        if (thietBi.TinhTrang != MaTinhTrangThietBi.DangBaoTriSuaChua)
        {
            throw TrangThaiKhongHopLe("Chỉ tiếp nhận sửa chữa thiết bị đang ở trạng thái bảo trì/sửa chữa.");
        }

        if (maPhieu is null)
        {
            return;
        }

        var dsChiTiet = await phieuMuon.LayChiTietTheoPhieuAsync(maPhieu, ct);
        var phieuDung = dsChiTiet.Any(c =>
            c.MaThietBi == thietBi.MaThietBi
            && c.KetQuaKiemTra is "HU_HONG" or "THIEU_PHU_KIEN");
        if (!phieuDung)
        {
            throw new LoiNghiepVuException(
                "DU_LIEU_KHONG_HOP_LE",
                "Phiếu mượn không có thiết bị này với kết quả kiểm tra hư hỏng hoặc thiếu phụ kiện.");
        }
    }

    private static LoiNghiepVuException KhongTonTai() =>
        new("DU_LIEU_KHONG_HOP_LE", "Không tìm thấy thiết bị.");

    private static LoiNghiepVuException TrangThaiKhongHopLe(string thongDiep) =>
        new("TRANG_THAI_KHONG_HOP_LE", thongDiep);
}
