using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class KiemTraDieuKienDuyet
{
    public const string TrangThaiChoDuyet = "CHO_DUYET";

    public static async Task<IReadOnlyList<KhaDungTheoLoaiDto>> TinhKhaDungAsync(
        IAvailabilityService khaDung,
        PhieuYeuCauMuonDto phieu,
        CancellationToken ct)
    {
        var ketQua = new List<KhaDungTheoLoaiDto>();
        foreach (var dong in phieu.DsLoaiVaSoLuong)
        {
            var soLuongKhaDung = await khaDung.TinhKhaDungAsync(phieu.MaPhong, dong.MaLoai, ct);
            ketQua.Add(new KhaDungTheoLoaiDto(dong.MaLoai, dong.TenLoai, dong.SoLuong, soLuongKhaDung));
        }

        return ketQua;
    }

    public static IReadOnlyList<LyDoDto> TongHopLyDo(
        DongHoSoSinhVien hoSo,
        DateOnly ngayHienTai,
        QuyDinhNhomDto quyDinh,
        IReadOnlyList<LoaiVaSoLuongDto> dsLoai,
        int thoiGianMuonGio,
        int soDangCam,
        IReadOnlyList<KhaDungTheoLoaiDto> dsKhaDung)
    {
        var dsLyDo = new List<LyDoDto>();

        GhiNhanLoi(dsLyDo, () => KiemTraDieuKienGuiYeuCau.KiemTraSinhVien(hoSo, ngayHienTai));
        GhiNhanLoi(dsLyDo, () => KiemTraDieuKienGuiYeuCau.KiemTraThoiGianMuon(quyDinh, dsLoai.Select(d => d.MaLoai), thoiGianMuonGio));
        GhiNhanLoi(dsLyDo, () => KiemTraDieuKienGuiYeuCau.KiemTraGioiHanThietBi(quyDinh, soDangCam, dsLoai.Sum(d => (long)d.SoLuong)));

        foreach (var dong in dsKhaDung.Where(d => d.SoLuongKhaDung < d.SoLuongYeuCau))
        {
            dsLyDo.Add(new LyDoDto(
                MaLoi.SoLuongKhaDungThayDoi,
                $"Loại {dong.TenLoai} chỉ còn {dong.SoLuongKhaDung}, cần {dong.SoLuongYeuCau}."));
        }

        return dsLyDo;
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
