namespace QuanLyMuonTra.Modules.BaoTri.Domain;

internal static class KetQuaBaoTriMa
{
    public const string DuaVeSanSang = "DUA_VE_SAN_SANG";
    public const string DeXuatThanhLy = "DE_XUAT_THANH_LY";

    public static bool HopLe(string? ma) => ma is DuaVeSanSang or DeXuatThanhLy;
}
