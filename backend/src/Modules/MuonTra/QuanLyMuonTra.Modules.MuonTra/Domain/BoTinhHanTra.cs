using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Domain;

public sealed class BoTinhHanTra : IBoTinhHanTra
{
    public DateTimeOffset TinhHanTra(DateTimeOffset gioGiao, int thoiGianMuonGio, TimeOnly? gioDongCua)
    {
        if (gioDongCua is null)
        {
            throw new LoiNghiepVuException(MaLoi.ChuaCauHinhGioQuay, "Chưa cấu hình giờ đóng cửa quầy.");
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
