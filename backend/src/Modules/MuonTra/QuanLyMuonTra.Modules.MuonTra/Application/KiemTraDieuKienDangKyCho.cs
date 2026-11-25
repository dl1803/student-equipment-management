using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class KiemTraDieuKienDangKyCho
{
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
