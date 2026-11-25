namespace QuanLyMuonTra.Modules.BaoTri.Domain;

public enum LoaiThucHienBaoTri
{
    BaoTriDinhKy,
    SuaChua,
}

internal static class LoaiThucHienBaoTriMa
{
    public const string BaoTriDinhKy = "BAO_TRI_DINH_KY";
    public const string SuaChua = "SUA_CHUA";

    public static string ToMa(LoaiThucHienBaoTri loai) => loai switch
    {
        LoaiThucHienBaoTri.BaoTriDinhKy => BaoTriDinhKy,
        LoaiThucHienBaoTri.SuaChua => SuaChua,
        _ => throw new ArgumentOutOfRangeException(nameof(loai), loai, null),
    };

    public static LoaiThucHienBaoTri ToLoai(string ma) => ma switch
    {
        BaoTriDinhKy => LoaiThucHienBaoTri.BaoTriDinhKy,
        SuaChua => LoaiThucHienBaoTri.SuaChua,
        _ => throw new ArgumentOutOfRangeException(nameof(ma), ma, null),
    };

    public static bool TryParse(string? ma, out LoaiThucHienBaoTri loai)
    {
        switch (ma)
        {
            case BaoTriDinhKy:
                loai = LoaiThucHienBaoTri.BaoTriDinhKy;
                return true;
            case SuaChua:
                loai = LoaiThucHienBaoTri.SuaChua;
                return true;
            default:
                loai = default;
                return false;
        }
    }
}

internal static class MaTinhTrangThietBi
{
    public const string SanSang = "SAN_SANG";
    public const string DangBaoTriSuaChua = "DANG_BAO_TRI_SUA_CHUA";
}
