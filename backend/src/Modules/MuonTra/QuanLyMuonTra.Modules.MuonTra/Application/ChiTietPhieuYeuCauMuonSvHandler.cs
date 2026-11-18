using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class ChiTietPhieuYeuCauMuonSvHandler(DbContext db, IDanhMucReader danhMuc)
{
    private static readonly Regex MauMaYeuCau = new(@"\AYC-[0-9]{6}-[0-9]{6,}\z", RegexOptions.CultureInvariant);

    public async Task<PhieuYeuCauMuonDto> XuLyAsync(string maSv, string maYeuCau, CancellationToken ct)
    {
        if (!MauMaYeuCau.IsMatch(maYeuCau))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Mã phiếu yêu cầu không đúng định dạng.");
        }

        var phieu = await PhieuYeuCauMuonTruyVan.LayTheoMaAsync(db, danhMuc, maYeuCau, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy phiếu yêu cầu.");

        if (phieu.MaSV != maSv)
        {
            throw new LoiNghiepVuException(MaLoi.KhongDuQuyen, "Không có quyền xem phiếu yêu cầu của người khác.");
        }

        return phieu;
    }
}
