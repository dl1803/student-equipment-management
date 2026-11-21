using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class KiemTraDieuKienGiao
{
    public static LyDoDto? KiemTraGiuCho(
        string maNguon,
        bool dungTrangThai,
        DateTimeOffset? hanGiuCho,
        DateTimeOffset thoiDiem)
    {
        if (!dungTrangThai)
        {
            return new LyDoDto(MaLoi.TrangThaiKhongHopLe, $"Nguồn {maNguon} không ở trạng thái cho phép giao.");
        }

        if (hanGiuCho is null || hanGiuCho.Value <= thoiDiem)
        {
            return new LyDoDto(MaLoi.GiuChoHetHan, $"Giữ chỗ của {maNguon} đã quá hạn, không giao theo nguồn cũ.");
        }

        return null;
    }

    public static IReadOnlyList<LyDoDto> TongHopLyDoSinhVien(
        HoSoSinhVienDto hoSo,
        DateOnly ngayHienTai,
        QuyDinhNhomDto quyDinh,
        int soDangCam,
        long soGiaoMoi)
    {
        return KiemTraTatCa(hoSo.HanThe, hoSo.TrangThaiHoc, hoSo.TrangThaiQuyenMuon, ngayHienTai, quyDinh, soDangCam, soGiaoMoi);
    }

    public static IReadOnlyList<LyDoDto> TongHopLyDoSinhVien(
        DongHoSoSinhVien hoSo,
        DateOnly ngayHienTai,
        QuyDinhNhomDto quyDinh,
        int soDangCam,
        long soGiaoMoi)
    {
        return KiemTraTatCa(hoSo.HanThe, hoSo.TrangThaiHoc, hoSo.TrangThaiQuyenMuon, ngayHienTai, quyDinh, soDangCam, soGiaoMoi);
    }

    public static LyDoDto? KiemTraKhaDung(string tenLoai, int soLuongKhaDung, int soLuongCan)
    {
        if (soLuongKhaDung >= soLuongCan)
        {
            return null;
        }

        return new LyDoDto(
            MaLoi.SoLuongKhaDungThayDoi,
            $"Loại {tenLoai} chỉ còn {soLuongKhaDung}, cần {soLuongCan}.");
    }

    private static IReadOnlyList<LyDoDto> KiemTraTatCa(
        DateOnly hanThe,
        string trangThaiHoc,
        string trangThaiQuyenMuon,
        DateOnly ngayHienTai,
        QuyDinhNhomDto quyDinh,
        int soDangCam,
        long soGiaoMoi)
    {
        var dsLyDo = new List<LyDoDto>();

        if (hanThe < ngayHienTai)
        {
            dsLyDo.Add(new LyDoDto(MaLoi.SvTheHetHan, "Thẻ sinh viên đã hết hạn."));
        }

        if (trangThaiHoc != "DANG_HOC")
        {
            dsLyDo.Add(new LyDoDto(MaLoi.SvKhongConTheoHoc, "Sinh viên không còn theo học."));
        }

        if (trangThaiQuyenMuon != "BINH_THUONG")
        {
            dsLyDo.Add(new LyDoDto(MaLoi.SvBiKhoaQuyenMuon, "Quyền mượn của sinh viên đang bị khóa."));
        }

        if (quyDinh.VuotGioiHanSoThietBi(soDangCam, soGiaoMoi))
        {
            dsLyDo.Add(new LyDoDto(
                MaLoi.VuotGioiHanThietBi,
                $"Vượt giới hạn {quyDinh.SoThietBiToiDa} thiết bị: đang cầm {soDangCam}, yêu cầu thêm {soGiaoMoi}."));
        }

        return dsLyDo;
    }
}
