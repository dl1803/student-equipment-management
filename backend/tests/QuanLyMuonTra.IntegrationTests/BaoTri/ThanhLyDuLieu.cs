using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

internal static class ThanhLyDuLieu
{
    public const string TenBoSuuTap = "D16-N-thanh-ly-nhac-bao-tri";
    public const string TenTep = "D16-N-du-lieu-thu.sql";
    public const string MaQt = "019c0000-0000-7000-8000-0000000c1631";
    public const string MaNv1 = "019c0000-0000-7000-8000-0000000c1621";
    public const string MaNv2 = "019c0000-0000-7000-8000-0000000c1622";
    public const string MaNv3 = "019c0000-0000-7000-8000-0000000c1623";
    public const string MaQuyetDinhCu = "QD-261101-916001";

    public static readonly Guid MaQtGuid = Guid.Parse(MaQt);
    public static readonly Guid MaNv1Guid = Guid.Parse(MaNv1);
    public static readonly Guid MaNv3Guid = Guid.Parse(MaNv3);
    public static readonly Guid BaoTriTb01 = Guid.Parse("019c0000-0000-7000-8000-0000000c16a1");
    public static readonly Guid BaoTriTb02 = Guid.Parse("019c0000-0000-7000-8000-0000000c16a3");

    public const string XoaTatCa = """
        DELETE FROM thong_bao.thong_bao WHERE ma_nv_nhan IN ('019c0000-0000-7000-8000-0000000c1621', '019c0000-0000-7000-8000-0000000c1622', '019c0000-0000-7000-8000-0000000c1623');
        DELETE FROM bao_tri.chi_tiet_thanh_ly WHERE ma_thiet_bi LIKE 'D16-TB-%';
        DELETE FROM bao_tri.quyet_dinh_thanh_ly WHERE ma_qt_lap = '019c0000-0000-7000-8000-0000000c1631';
        DELETE FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi LIKE 'D16-TB-%';
        DELETE FROM muon_tra.dang_ky_cho WHERE ma_dang_ky = 'DK-261123-916001';
        DELETE FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi LIKE 'D16-TB-%';
        DELETE FROM danh_muc.thiet_bi WHERE ma_thiet_bi LIKE 'D16-TB-%';
        DELETE FROM danh_muc.loai_thiet_bi WHERE ma_loai IN ('019c0000-0000-7000-8000-0000000c1611', '019c0000-0000-7000-8000-0000000c1612', '019c0000-0000-7000-8000-0000000c1613');
        DELETE FROM tai_khoan.nhan_vien_phong WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000c1621', '019c0000-0000-7000-8000-0000000c1622', '019c0000-0000-7000-8000-0000000c1623');
        DELETE FROM danh_muc.phong_thiet_bi WHERE ma_phong IN ('019c0000-0000-7000-8000-0000000c1601', '019c0000-0000-7000-8000-0000000c1602');
        DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000c1621', '019c0000-0000-7000-8000-0000000c1622', '019c0000-0000-7000-8000-0000000c1623');
        DELETE FROM tai_khoan.quan_tri_vien WHERE ma_nguoi_dung = '019c0000-0000-7000-8000-0000000c1631';
        DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000c1631', '019c0000-0000-7000-8000-0000000c1621', '019c0000-0000-7000-8000-0000000c1622', '019c0000-0000-7000-8000-0000000c1623');
        DELETE FROM sinh_vien.sinh_vien WHERE ma_sv = 'D16-SV-A';
        DELETE FROM sinh_vien.nhom_sinh_vien WHERE ma_nhom = 'D16-NHOM';
        """;

    public static Task<string> DocTepAsync() =>
        File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "BaoTri", TenTep));

    public static async Task<string> TinhTrangAsync(BaoTriMoiTruong mt, string maThietBi) =>
        (await mt.LayChuoiAsync($"""
            SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {maThietBi}
            """)).Single();

    public static Task<int> DemNhatKyAsync(BaoTriMoiTruong mt, string doiTuong, string doiTuongId) =>
        mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = {doiTuong} AND doi_tuong_id = {doiTuongId}
            """);

    public static async Task ChayAsync(Func<ServiceProvider, Task> kiemTra, Action<IServiceCollection>? tuyChinh = null)
    {
        await using var dichVu = BaoTriTestSupport.TaoDichVu(tuyChinh);
        var sqlNap = await DocTepAsync();
        await ThucThiAsync(dichVu, XoaTatCa);
        await ThucThiAsync(dichVu, sqlNap);
        try
        {
            await kiemTra(dichVu);
        }
        finally
        {
            await ThucThiAsync(dichVu, XoaTatCa);
        }
    }

    public static async Task ThucThiAsync(ServiceProvider dichVu, string sql)
    {
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlRawAsync(sql);
    }

    public static async Task<int> DemAsync(ServiceProvider dichVu, FormattableString sql)
    {
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Database.SqlQuery<int>(sql).SingleAsync();
    }

    public static async Task<string> ChuoiAsync(ServiceProvider dichVu, FormattableString sql)
    {
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Database.SqlQuery<string>(sql).SingleAsync();
    }

    public static Task<int> DemQuyetDinhMoiAsync(ServiceProvider dichVu) =>
        DemAsync(dichVu, $"""
            SELECT count(*)::int AS "Value" FROM bao_tri.quyet_dinh_thanh_ly
            WHERE ma_qt_lap = {MaQtGuid} AND ma_quyet_dinh <> {MaQuyetDinhCu}
            """);
}
