namespace QuanLyMuonTra.Modules.ThongBao.Domain;

public static class LichGuiLaiEmail
{
    public const int SoLanThuToiDa = 5;

    private static readonly TimeSpan[] GianCach =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(60)
    ];

    public static bool LoiVinhVien(int soLanThu) => soLanThu >= SoLanThuToiDa;

    public static TimeSpan GianCachSauLanThu(int soLanThu)
    {
        if (soLanThu < 1 || soLanThu >= SoLanThuToiDa)
        {
            throw new ArgumentOutOfRangeException(
                nameof(soLanThu),
                soLanThu,
                $"Chỉ có giãn cách sau lần thử từ 1 đến {SoLanThuToiDa - 1}.");
        }

        return GianCach[soLanThu - 1];
    }
}

