using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Domain;

public static class TinhPhi
{
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

    public static decimal PhiTre(int soNgayTre, decimal phiMoiNgay, decimal giaTriHienHanh)
    {
        return Math.Min(soNgayTre * phiMoiNgay, giaTriHienHanh);
    }

    public static decimal BoiThuongHuHong(decimal giaTriHienHanh, decimal tyLePhanTram)
    {
        return decimal.Round(giaTriHienHanh * tyLePhanTram / 100m, 0, MidpointRounding.AwayFromZero);
    }

    public static decimal BoiThuongMat(decimal giaTriHienHanh)
    {
        return giaTriHienHanh;
    }
}
