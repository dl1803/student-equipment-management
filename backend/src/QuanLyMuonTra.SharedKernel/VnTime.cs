namespace QuanLyMuonTra.SharedKernel;

public static class VnTime
{
    private static readonly TimeZoneInfo MuiGio = LayMuiGio();

    private static TimeZoneInfo LayMuiGio()
    {
        if (TimeZoneInfo.TryFindSystemTimeZoneById("Asia/Ho_Chi_Minh", out var tz)) return tz;
        if (TimeZoneInfo.TryFindSystemTimeZoneById("Asia/Saigon", out tz)) return tz;
        if (TimeZoneInfo.TryFindSystemTimeZoneById("SE Asia Standard Time", out tz)) return tz;
        return TimeZoneInfo.CreateCustomTimeZone("Asia/Ho_Chi_Minh", TimeSpan.FromHours(7), "ICT", "ICT");
    }

    public static DateOnly NgayLich(DateTimeOffset thoiDiem)
    {
        var gioVietNam = TimeZoneInfo.ConvertTime(thoiDiem, MuiGio);
        return DateOnly.FromDateTime(gioVietNam.DateTime);
    }
}
