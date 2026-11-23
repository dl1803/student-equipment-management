using System.Text.RegularExpressions;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class KiemTraDocPhieuMuon
{
    private static readonly Regex MauMaPhieu = new(@"\APM-[0-9]{6}-[0-9]{6,}\z", RegexOptions.CultureInvariant);

    private static readonly HashSet<string> TrangThaiHopLe =
    [
        "DANG_MUON",
        "QUA_HAN",
        "DA_TRA"
    ];

    public static void KiemTraMaPhieu(string maPhieu)
    {
        if (!MauMaPhieu.IsMatch(maPhieu))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Mã phiếu mượn không đúng định dạng.");
        }
    }

    public static void KiemTraTrangThai(string? trangThai)
    {
        if (trangThai is not null && !TrangThaiHopLe.Contains(trangThai))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Trạng thái lọc không hợp lệ.");
        }
    }
}
