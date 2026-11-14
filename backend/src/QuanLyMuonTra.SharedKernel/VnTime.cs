namespace QuanLyMuonTra.SharedKernel;

public static class VnTime
{
    private const string MuiGioId = "Asia/Ho_Chi_Minh";

    private static readonly TimeZoneInfo MuiGio = TimeZoneInfo.FindSystemTimeZoneById(MuiGioId);

    public static DateOnly NgayLich(DateTimeOffset thoiDiem)
    {
        var gioVietNam = TimeZoneInfo.ConvertTime(thoiDiem, MuiGio);
        return DateOnly.FromDateTime(gioVietNam.DateTime);
    }
}

