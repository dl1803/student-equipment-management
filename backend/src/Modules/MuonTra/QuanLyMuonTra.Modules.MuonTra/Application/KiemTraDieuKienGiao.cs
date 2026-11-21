using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
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
        DongHoSoSinhVien hoSo,
        DateOnly ngayHienTai,
        QuyDinhNhomDto quyDinh,
        int soDangCam,
        long soGiaoMoi)
    {
        var dsLyDo = new List<LyDoDto>();

        GhiNhanLoi(dsLyDo, () => KiemTraDieuKienGuiYeuCau.KiemTraSinhVien(hoSo, ngayHienTai));
        GhiNhanLoi(dsLyDo, () => KiemTraDieuKienGuiYeuCau.KiemTraGioiHanThietBi(quyDinh, soDangCam, soGiaoMoi));

        return dsLyDo;
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

    private static void GhiNhanLoi(List<LyDoDto> dsLyDo, Action kiemTra)
    {
        try
        {
            kiemTra();
        }
        catch (LoiNghiepVuException loi)
        {
            dsLyDo.Add(new LyDoDto(loi.MaLoi, loi.Message));
        }
    }
}
