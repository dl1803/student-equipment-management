namespace QuanLyMuonTra.SharedKernel;

public static class MaLoiNghiepVu
{
    private static readonly Dictionary<string, int> MaHttpTheoMaLoi = new()
    {
        ["DU_LIEU_KHONG_HOP_LE"] = 400,
        ["DANG_NHAP_SAI"] = 401,
        ["CHUA_XAC_THUC"] = 401,
        ["TAI_KHOAN_BI_KHOA"] = 403,
        ["KHONG_DU_QUYEN"] = 403,
        ["PHONG_CHUA_CAP_QUYEN"] = 403,
        ["KHONG_TIM_THAY"] = 404,
        ["SO_LUONG_KHA_DUNG_THAY_DOI"] = 409,
        ["GIU_CHO_HET_HAN"] = 409,
        ["THIET_BI_DA_DUOC_GIAO"] = 409,
        ["TRANG_THAI_KHONG_HOP_LE"] = 409,
        ["MA_THIET_BI_TRUNG"] = 409,
        ["MA_NV_TRUNG"] = 409,
        ["TEN_DANG_NHAP_TRUNG"] = 409,
        ["KHUNG_PHAT_DA_DUNG_KHONG_XOA"] = 409,
        ["CHUA_CAU_HINH_GIO_QUAY"] = 409,
        ["XUNG_DOT_CAP_NHAT"] = 409,
        ["DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG"] = 409,
        ["SV_THE_HET_HAN"] = 422,
        ["SV_KHONG_CON_THEO_HOC"] = 422,
        ["SV_BI_KHOA_QUYEN_MUON"] = 422,
        ["VUOT_GIOI_HAN_THIET_BI"] = 422,
        ["VUOT_THOI_GIAN_MUON_TOI_DA"] = 422,
        ["CON_HANG_KHONG_DANG_KY_CHO"] = 422,
        ["THIET_BI_KHONG_SAN_SANG"] = 422,
        ["THIET_BI_SAI_PHONG"] = 422,
        ["THIET_BI_SAI_LOAI"] = 422,
        ["THIET_BI_KHONG_THUOC_PHIEU"] = 422,
        ["THIEU_MO_TA_HOAC_KHUNG_PHAT"] = 422,
        ["GIA_TRI_CAU_HINH_KHONG_HOP_LE"] = 422,
        ["GIAO_SAU_GIO_DONG_CUA"] = 422,
        ["QUA_NHIEU_YEU_CAU"] = 429
    };

    public static IReadOnlyDictionary<string, int> TatCa => MaHttpTheoMaLoi;

    public static int LayMaHttp(string maLoi)
    {
        if (!MaHttpTheoMaLoi.TryGetValue(maLoi, out var maHttp))
        {
            throw new ArgumentException($"Mã lỗi không có trong bảng mã lỗi nghiệp vụ: {maLoi}", nameof(maLoi));
        }

        return maHttp;
    }
}

