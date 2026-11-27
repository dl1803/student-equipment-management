using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Domain;

/// <summary>
/// Bộ tính phí vi phạm, phí trễ và bồi thường thiệt hại (UC-09, BR-57 đến BR-60).
/// </summary>
public static class TinhPhi
{
    /// <summary>
    /// BR-57: Số ngày trễ theo ngày lịch Việt Nam, ân hạn 15 phút, tối thiểu 1 ngày khi quá ân hạn.
    /// </summary>
    public static int SoNgayTre(DateTimeOffset hanTra, DateTimeOffset thoiDiem, int anHanPhut)
    {
        if (thoiDiem <= hanTra.AddMinutes(anHanPhut))
        {
            return 0;
        }

        var ngayHan = VnTime.NgayLich(hanTra);
        var ngayTra = VnTime.NgayLich(thoiDiem);
        return Math.Max(1, ngayTra.DayNumber - ngayHan.DayNumber);
    }

    /// <summary>
    /// BR-58: Phí trả trễ tính riêng từng thiết bị, tối đa bằng giá trị hiện hành, không âm.
    /// </summary>
    public static decimal PhiTre(int soNgayTre, decimal phiMoiNgay, decimal giaTriHienHanh)
    {
        if (soNgayTre <= 0 || phiMoiNgay <= 0m || giaTriHienHanh <= 0m)
        {
            return 0m;
        }

        return Math.Min(soNgayTre * phiMoiNgay, giaTriHienHanh);
    }

    /// <summary>
    /// BR-59: Bồi thường hư hỏng theo tỷ lệ phần trăm khung phạt, làm tròn AwayFromZero đến đồng, không âm.
    /// </summary>
    public static decimal BoiThuongHuHong(decimal giaTriHienHanh, decimal tyLePhanTram)
    {
        if (giaTriHienHanh <= 0m || tyLePhanTram <= 0m)
        {
            return 0m;
        }

        return decimal.Round(giaTriHienHanh * tyLePhanTram / 100m, 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// BR-60: Bồi thường mất bằng 100% giá trị hiện hành tại thời điểm mất, không âm.
    /// </summary>
    public static decimal BoiThuongMat(decimal giaTriHienHanh)
    {
        return Math.Max(0m, giaTriHienHanh);
    }

    public const int MocBaoMatSoNgay = 30;

    public static bool QuaMocBaoMat(DateTimeOffset hanTra, DateTimeOffset thoiDiem)
    {
        var soNgayLich = VnTime.NgayLich(thoiDiem).DayNumber - VnTime.NgayLich(hanTra).DayNumber;
        return soNgayLich > MocBaoMatSoNgay;
    }
}
