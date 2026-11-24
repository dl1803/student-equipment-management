using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests;

public class MaLoiNghiepVuTests
{
    [Fact]
    public void CV67_MaLoi_KhopChinhXacEnumMaLoiTrongOpenApi()
    {
        var trongYaml = DocMaLoiTuOpenApi();

        Assert.Equal(32, trongYaml.Count);
        Assert.Equal(
            trongYaml.Order(StringComparer.Ordinal).ToList(),
            MaLoiNghiepVu.TatCa.Keys.Order(StringComparer.Ordinal).ToList());
    }

    [Theory]
    [InlineData(400, new[] { "DU_LIEU_KHONG_HOP_LE" })]
    [InlineData(401, new[] { "DANG_NHAP_SAI", "CHUA_XAC_THUC" })]
    [InlineData(403, new[] { "TAI_KHOAN_BI_KHOA", "KHONG_DU_QUYEN", "PHONG_CHUA_CAP_QUYEN" })]
    [InlineData(404, new[] { "KHONG_TIM_THAY" })]
    [InlineData(409, new[]
    {
        "SO_LUONG_KHA_DUNG_THAY_DOI", "GIU_CHO_HET_HAN", "THIET_BI_DA_DUOC_GIAO", "TRANG_THAI_KHONG_HOP_LE",
        "MA_THIET_BI_TRUNG", "MA_NV_TRUNG", "TEN_DANG_NHAP_TRUNG", "KHUNG_PHAT_DA_DUNG_KHONG_XOA",
        "CHUA_CAU_HINH_GIO_QUAY", "XUNG_DOT_CAP_NHAT", "DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG"
    })]
    [InlineData(422, new[]
    {
        "SV_THE_HET_HAN", "SV_KHONG_CON_THEO_HOC", "SV_BI_KHOA_QUYEN_MUON", "VUOT_GIOI_HAN_THIET_BI",
        "VUOT_THOI_GIAN_MUON_TOI_DA", "CON_HANG_KHONG_DANG_KY_CHO", "THIET_BI_KHONG_SAN_SANG",
        "THIET_BI_SAI_PHONG", "THIET_BI_SAI_LOAI", "THIET_BI_KHONG_THUOC_PHIEU",
        "THIEU_MO_TA_HOAC_KHUNG_PHAT", "GIA_TRI_CAU_HINH_KHONG_HOP_LE", "GIAO_SAU_GIO_DONG_CUA"
    })]
    [InlineData(429, new[] { "QUA_NHIEU_YEU_CAU" })]
    public void CV67_MaLoi_MaHttpDungBang(int maHttp, string[] cacMaLoi)
    {
        foreach (var maLoi in cacMaLoi)
        {
            Assert.Equal(maHttp, MaLoiNghiepVu.LayMaHttp(maLoi));
        }
    }

    [Fact]
    public void CV67_LoiNghiepVu_MaLoiNgoaiBang_NemArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new LoiNghiepVuException("MA_LOI_TU_DAT", "Thông điệp"));
    }

    [Fact]
    public void CV67_LoiNghiepVu_MaLoiHopLe_GiuMaLoiMaHttpVaThongDiep()
    {
        var loi = new LoiNghiepVuException("TRANG_THAI_KHONG_HOP_LE", "Phiếu không ở trạng thái cho phép.");

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("Phiếu không ở trạng thái cho phép.", loi.Message);
    }

    private static List<string> DocMaLoiTuOpenApi()
    {
        var cacDong = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "openapi.yaml"));
        var batDau = Array.FindIndex(cacDong, dong => dong.TrimEnd() == "    MaLoi:");
        var ketThuc = Array.FindIndex(cacDong, batDau + 1, dong => dong.TrimEnd() == "    ProblemDetails:");

        return cacDong[batDau..ketThuc]
            .Where(dong => dong.StartsWith("      - "))
            .Select(dong => dong[8..].Trim())
            .ToList();
    }
}

