namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

internal static class TinhTrangThietBiMa
{
    public const string SanSang = "SAN_SANG";
    public const string DangDuocMuon = "DANG_DUOC_MUON";
    public const string DangBaoTriSuaChua = "DANG_BAO_TRI_SUA_CHUA";
    public const string DaThanhLy = "DA_THANH_LY";
    public const string DaBaoMat = "DA_BAO_MAT";

    public static string ToMa(TinhTrangThietBi tinhTrang) => tinhTrang switch
    {
        TinhTrangThietBi.SanSang => SanSang,
        TinhTrangThietBi.DangDuocMuon => DangDuocMuon,
        TinhTrangThietBi.DangBaoTriSuaChua => DangBaoTriSuaChua,
        TinhTrangThietBi.DaThanhLy => DaThanhLy,
        TinhTrangThietBi.DaBaoMat => DaBaoMat,
        _ => throw new ArgumentOutOfRangeException(nameof(tinhTrang), tinhTrang, null),
    };

    public static TinhTrangThietBi ToTinhTrang(string ma) => ma switch
    {
        SanSang => TinhTrangThietBi.SanSang,
        DangDuocMuon => TinhTrangThietBi.DangDuocMuon,
        DangBaoTriSuaChua => TinhTrangThietBi.DangBaoTriSuaChua,
        DaThanhLy => TinhTrangThietBi.DaThanhLy,
        DaBaoMat => TinhTrangThietBi.DaBaoMat,
        _ => throw new ArgumentOutOfRangeException(nameof(ma), ma, null),
    };
}
