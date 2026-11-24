using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Domain;

/// <summary>
/// BR-42: Bộ tính hạn trả theo giờ giao, thời gian mượn và giờ đóng cửa quầy trong ngày.
/// </summary>
public sealed class BoTinhHanTra : IBoTinhHanTra
{
    /// <summary>
    /// BR-42: Tính hạn trả = min(giờ giao + thời gian mượn, giờ đóng cửa cùng ngày lịch Việt Nam).
    /// </summary>
    public DateTimeOffset TinhHanTra(DateTimeOffset gioGiao, int thoiGianMuonGio, TimeOnly? gioDongCua)
    {
        if (gioDongCua is null)
        {
            throw new LoiNghiepVuException(MaLoi.ChuaCauHinhGioQuay, "Chưa cấu hình giờ đóng cửa quầy.");
        }

        if (thoiGianMuonGio <= 0)
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Thời gian mượn phải lớn hơn 0.");
        }

        if (VnTime.GioLich(gioGiao) >= gioDongCua.Value)
        {
            throw new LoiNghiepVuException(MaLoi.GiaoSauGioDongCua, "Không thể giao thiết bị từ giờ đóng cửa quầy trở đi.");
        }

        var hanTheoThoiGianMuon = gioGiao.AddHours(thoiGianMuonGio);
        var gioDongCuaCungNgay = VnTime.ThoiDiem(VnTime.NgayLich(gioGiao), gioDongCua.Value);
        var hanTra = hanTheoThoiGianMuon < gioDongCuaCungNgay ? hanTheoThoiGianMuon : gioDongCuaCungNgay;
        return hanTra.ToUniversalTime();
    }
}
