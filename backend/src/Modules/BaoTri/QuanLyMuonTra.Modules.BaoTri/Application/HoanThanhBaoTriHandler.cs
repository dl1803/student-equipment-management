using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.BaoTri.Domain;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class HoanThanhBaoTriHandler(
    DbContext db,
    IDanhMucReader danhMuc,
    IThietBiService thietBiService,
    IAdvisoryLockService khoa,
    IQuyenPhongService quyenPhong,
    IFifoService capLuot)
{
    public async Task<BaoTriSuaChuaDto> XuLyAsync(
        Guid maNv,
        Guid maBaoTri,
        HoanThanhBaoTriRequest yeuCau,
        CancellationToken ct)
    {
        BaoTriSuaChua.KiemTraKetQua(yeuCau.NoiDungThucHien, yeuCau.KetQua);

        var banGhiTruoc = await db.Set<BaoTriSuaChua>()
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.MaBaoTri == maBaoTri, ct) ?? throw KhongTimThay();
        var thietBiTruoc = await danhMuc.LayThietBiAsync(banGhiTruoc.MaThietBi, ct) ?? throw KhongTimThay();
        await quyenPhong.EnsureAsync(maNv, thietBiTruoc.MaPhong, ct);

        var giaoDichNgoai = db.Database.CurrentTransaction;
        await using var giaoDichRieng = giaoDichNgoai is null ? await db.Database.BeginTransactionAsync(ct) : null;

        await khoa.LockPhongLoaiAsync(thietBiTruoc.MaPhong, thietBiTruoc.MaLoai, ct);

        var banGhi = await db.Set<BaoTriSuaChua>()
            .SingleOrDefaultAsync(b => b.MaBaoTri == maBaoTri, ct) ?? throw KhongTimThay();
        var thietBi = await danhMuc.LayThietBiAsync(banGhi.MaThietBi, ct) ?? throw KhongTimThay();
        if (thietBi.MaPhong != thietBiTruoc.MaPhong || thietBi.MaLoai != thietBiTruoc.MaLoai)
        {
            throw new LoiNghiepVuException("XUNG_DOT_CAP_NHAT", "Thiết bị vừa được thay đổi bởi giao dịch khác. Vui lòng tải lại.");
        }

        banGhi.HoanThanh(yeuCau.NoiDungThucHien, yeuCau.KetQua, yeuCau.NgayHoanThanh);

        if (thietBi.TinhTrang != MaTinhTrangThietBi.DangBaoTriSuaChua)
        {
            throw TrangThaiKhongHopLe("Thiết bị không còn ở trạng thái bảo trì/sửa chữa.");
        }

        await db.SaveChangesAsync(ct);

        if (banGhi.KetQua == KetQuaBaoTriMa.DuaVeSanSang)
        {
            if (!await thietBiService.HoanThanhBaoTriAsync(thietBi.MaThietBi, ct))
            {
                throw TrangThaiKhongHopLe("Thiết bị không còn ở trạng thái bảo trì/sửa chữa.");
            }

            await capLuot.CapLuotChoAsync(thietBi.MaPhong, thietBi.MaLoai, ct);
        }

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

    private static LoiNghiepVuException KhongTimThay() =>
        new("KHONG_TIM_THAY", "Không tìm thấy hồ sơ bảo trì/sửa chữa.");

    private static LoiNghiepVuException TrangThaiKhongHopLe(string thongDiep) =>
        new("TRANG_THAI_KHONG_HOP_LE", thongDiep);
}
