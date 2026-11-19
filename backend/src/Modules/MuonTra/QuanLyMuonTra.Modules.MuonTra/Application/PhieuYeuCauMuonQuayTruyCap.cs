using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class PhieuYeuCauMuonQuayTruyCap
{
    private static readonly Regex MauMaYeuCau = new(@"\AYC-[0-9]{6}-[0-9]{6,}\z", RegexOptions.CultureInvariant);

    public static async Task<PhieuYeuCauMuonDto> LayVaKiemQuyenAsync(
        DbContext db,
        IDanhMucReader danhMuc,
        IQuyenPhongService quyenPhong,
        Guid maNv,
        string maYeuCau,
        CancellationToken ct)
    {
        if (!MauMaYeuCau.IsMatch(maYeuCau))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Mã phiếu yêu cầu không đúng định dạng.");
        }

        var phieu = await PhieuYeuCauMuonTruyVan.LayTheoMaAsync(db, danhMuc, maYeuCau, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy phiếu yêu cầu.");

        await quyenPhong.EnsureAsync(maNv, phieu.MaPhong, ct);

        return phieu;
    }
}
