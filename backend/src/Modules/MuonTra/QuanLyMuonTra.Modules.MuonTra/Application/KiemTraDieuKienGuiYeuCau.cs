using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class KiemTraDieuKienGuiYeuCau
{
    public static void KiemTraSinhVien(DongHoSoSinhVien hoSo, DateOnly ngayHienTai)
    {
        if (hoSo.HanThe < ngayHienTai)
        {
            throw new LoiNghiepVuException(MaLoi.SvTheHetHan, "Thẻ sinh viên đã hết hạn.");
        }

        if (hoSo.TrangThaiHoc != "DANG_HOC")
        {
            throw new LoiNghiepVuException(MaLoi.SvKhongConTheoHoc, "Sinh viên không còn theo học.");
        }

        if (hoSo.TrangThaiQuyenMuon != "BINH_THUONG")
        {
            throw new LoiNghiepVuException(MaLoi.SvBiKhoaQuyenMuon, "Quyền mượn của sinh viên đang bị khóa.");
        }
    }

    public static void KiemTraThoiGianMuon(QuyDinhNhomDto quyDinh, IEnumerable<Guid> dsMaLoai, int thoiGianGio)
    {
        var toiDa = quyDinh.ThoiGianToiDaGio(dsMaLoai);
        if (thoiGianGio > toiDa)
        {
            throw new LoiNghiepVuException(
                MaLoi.VuotThoiGianMuonToiDa,
                $"Thời gian mượn tối đa cho phép là {toiDa} giờ.");
        }
    }

    public static void KiemTraGioiHanThietBi(QuyDinhNhomDto quyDinh, int soDangCam, long soMuonThem)
    {
        if (quyDinh.VuotGioiHanSoThietBi(soDangCam, soMuonThem))
        {
            throw new LoiNghiepVuException(
                MaLoi.VuotGioiHanThietBi,
                $"Vượt giới hạn {quyDinh.SoThietBiToiDa} thiết bị: đang cầm {soDangCam}, yêu cầu thêm {soMuonThem}.");
        }
    }
}
