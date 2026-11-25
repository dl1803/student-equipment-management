using System.Text.RegularExpressions;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class KiemTraDieuKienDangKyCho
{
    private static readonly Regex MauMaDangKy = new(@"\ADK-[0-9]{6}-[0-9]{6,}\z", RegexOptions.CultureInvariant);

    public static void KiemTraMaDangKy(string maDangKy)
    {
        if (!MauMaDangKy.IsMatch(maDangKy))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Mã đăng ký chờ không đúng định dạng.");
        }
    }

    public static void KiemTraQuyenMuon(string trangThaiQuyenMuon)
    {
        if (trangThaiQuyenMuon != MaTrangThaiQuyenMuon.BinhThuong)
        {
            throw new LoiNghiepVuException(MaLoi.SvBiKhoaQuyenMuon, "Quyền mượn của sinh viên đang bị khóa.");
        }
    }

    public static void KiemTraConHang(int soKhaDung)
    {
        if (soKhaDung > 0)
        {
            throw new LoiNghiepVuException(
                MaLoi.ConHangKhongDangKyCho,
                "Loại thiết bị còn số lượng khả dụng tại phòng này, vui lòng đặt trước.");
        }
    }
}
