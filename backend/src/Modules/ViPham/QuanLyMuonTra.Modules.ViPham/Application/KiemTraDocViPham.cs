using System.Text.RegularExpressions;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Application;

internal static class KiemTraDocViPham
{
    private static readonly Regex MauMaPhieu = new(@"\APM-[0-9]{6}-[0-9]{6,}\z", RegexOptions.CultureInvariant);

    private static readonly HashSet<string> TrangThaiBienBanHopLe = ["CHUA_NOP", "DA_NOP", "DA_HUY"];

    public static void KiemTraMaPhieu(string maPhieu)
    {
        if (!MauMaPhieu.IsMatch(maPhieu))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Mã phiếu mượn không đúng định dạng.");
        }
    }

    public static void KiemTraTrangThaiBienBan(string? trangThai)
    {
        if (trangThai is not null && !TrangThaiBienBanHopLe.Contains(trangThai))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Trạng thái biên bản không hợp lệ.");
        }
    }
}

