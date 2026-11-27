namespace QuanLyMuonTra.IntegrationTests.BaoTri;

internal static class HoanThanhBaoTriDuLieu
{
    public const string TenBoSuuTap = "D15-N-hoan-thanh-bao-tri";
    public const string TenTep = "D15-N-du-lieu-thu-hoan-thanh.sql";
    public const string MaNv = "019c0000-0000-7000-8000-0000000c1521";
    public const string Dk1 = "DK-261123-915001";
    public const string Dk2 = "DK-261123-915002";
    public const string Dk3 = "DK-261123-915003";

    public static readonly Guid BaoTri1 = Guid.Parse("019c0000-0000-7000-8000-0000000c1531");
    public static readonly Guid BaoTri2 = Guid.Parse("019c0000-0000-7000-8000-0000000c1532");
    public static readonly Guid BaoTri3 = Guid.Parse("019c0000-0000-7000-8000-0000000c1533");
    public static readonly Guid BaoTri4 = Guid.Parse("019c0000-0000-7000-8000-0000000c1534");
    public static readonly Guid BaoTri6 = Guid.Parse("019c0000-0000-7000-8000-0000000c1536");

    public const string XoaTatCa = """
        DELETE FROM thong_bao.thong_bao WHERE ma_sv_nhan LIKE 'D15-SV-%';
        DELETE FROM muon_tra.dang_ky_cho WHERE ma_dang_ky LIKE 'DK-261123-915%';
        DELETE FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi LIKE 'D15-TB-%';
        DELETE FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi LIKE 'D15-TB-%';
        DELETE FROM danh_muc.thiet_bi WHERE ma_thiet_bi LIKE 'D15-TB-%';
        DELETE FROM danh_muc.loai_thiet_bi WHERE ma_loai IN ('019c0000-0000-7000-8000-0000000c1511', '019c0000-0000-7000-8000-0000000c1512', '019c0000-0000-7000-8000-0000000c1513');
        DELETE FROM tai_khoan.nhan_vien_phong WHERE ma_nguoi_dung = '019c0000-0000-7000-8000-0000000c1521';
        DELETE FROM danh_muc.phong_thiet_bi WHERE ma_phong IN ('019c0000-0000-7000-8000-0000000c1501', '019c0000-0000-7000-8000-0000000c1502');
        DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung = '019c0000-0000-7000-8000-0000000c1521';
        DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = '019c0000-0000-7000-8000-0000000c1521';
        DELETE FROM sinh_vien.sinh_vien WHERE ma_sv LIKE 'D15-SV-%';
        DELETE FROM sinh_vien.nhom_sinh_vien WHERE ma_nhom = 'D15-NHOM';
        """;

    public static Task<string> DocTepAsync() =>
        File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "BaoTri", TenTep));
}
