using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

public sealed class ThietBiServiceCsdlTests
{
    [CanKetNoiTestFact]
    public async Task UC12_DatDangDuocMuon_DungDieuKienChuyenMotLanVaGhiNhatKy()
    {
        await using var dichVu = ThietBiGhiCsdlTests.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhong, maLoai) = await ThietBiGhiCsdlTests.TaoPhongLoaiAsync(phamVi.ServiceProvider, "dat");
        var (maPhongKhac, maLoaiKhac) = await ThietBiGhiCsdlTests.TaoPhongLoaiAsync(phamVi.ServiceProvider, "khac");
        var ma = await TaoThietBiAsync(phamVi.ServiceProvider, "TB-D07-DAT", maLoai, maPhong);
        var dichVuThietBi = phamVi.ServiceProvider.GetRequiredService<IThietBiService>();
        var ct = CancellationToken.None;

        Assert.False(await dichVuThietBi.DatDangDuocMuonAsync(ma, maPhongKhac, maLoai, ct));
        Assert.False(await dichVuThietBi.DatDangDuocMuonAsync(ma, maPhong, maLoaiKhac, ct));
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE danh_muc.thiet_bi SET ngung_su_dung = true WHERE ma_thiet_bi = {ma}");
        Assert.False(await dichVuThietBi.DatDangDuocMuonAsync(ma, maPhong, maLoai, ct));
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE danh_muc.thiet_bi SET ngung_su_dung = false WHERE ma_thiet_bi = {ma}");

        Assert.True(await dichVuThietBi.DatDangDuocMuonAsync(ma, maPhong, maLoai, ct));
        Assert.False(await dichVuThietBi.DatDangDuocMuonAsync(ma, maPhong, maLoai, ct));

        Assert.Equal("DANG_DUOC_MUON", await LayTinhTrangAsync(db, ma));
        var truoc = await LayChuoiAsync(db, $"""
            SELECT gia_tri_truoc::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.thiet_bi' AND doi_tuong_id = {ma}
            """);
        var sau = await LayChuoiAsync(db, $"""
            SELECT gia_tri_sau::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.thiet_bi' AND doi_tuong_id = {ma}
            """);
        Assert.Contains("SanSang", Assert.Single(truoc));
        Assert.Contains("DangDuocMuon", Assert.Single(sau));
    }

    [CanKetNoiTestFact]
    public async Task UC12_ThietBiService_ChuyenDungNguonDichVaTuChoiTrangThaiSai()
    {
        await using var dichVu = ThietBiGhiCsdlTests.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhong, maLoai) = await ThietBiGhiCsdlTests.TaoPhongLoaiAsync(phamVi.ServiceProvider, "chuyen");
        var ma = await TaoThietBiAsync(phamVi.ServiceProvider, "TB-D07-CHUYEN", maLoai, maPhong);
        var dichVuThietBi = phamVi.ServiceProvider.GetRequiredService<IThietBiService>();
        var ct = CancellationToken.None;
        var cacChuyen = new (string Nguon, string Dich, Func<IThietBiService, Task<bool>> HanhDong)[]
        {
            ("DANG_DUOC_MUON", "SAN_SANG", s => s.NhanTraNguyenVenAsync(ma, ct)),
            ("DANG_DUOC_MUON", "DANG_BAO_TRI_SUA_CHUA", s => s.NhanTraHuHongAsync(ma, ct)),
            ("DANG_DUOC_MUON", "DA_BAO_MAT", s => s.DanhDauMatAsync(ma, ct)),
            ("SAN_SANG", "DANG_BAO_TRI_SUA_CHUA", s => s.ChuyenBaoTriAsync(ma, ct)),
            ("DANG_BAO_TRI_SUA_CHUA", "SAN_SANG", s => s.HoanThanhBaoTriAsync(ma, ct)),
            ("SAN_SANG", "DA_THANH_LY", s => s.ThanhLyAsync(ma, ct)),
            ("DANG_BAO_TRI_SUA_CHUA", "DA_THANH_LY", s => s.ThanhLyAsync(ma, ct)),
        };

        foreach (var (nguon, dich, hanhDong) in cacChuyen)
        {
            await DatTinhTrangAsync(db, ma, nguon);
            Assert.True(await hanhDong(dichVuThietBi));
            Assert.Equal(dich, await LayTinhTrangAsync(db, ma));

            await DatTinhTrangAsync(db, ma, "DA_THANH_LY");
            Assert.False(await hanhDong(dichVuThietBi));
            Assert.Equal("DA_THANH_LY", await LayTinhTrangAsync(db, ma));
        }

        var soNhatKy = await LayChuoiAsync(db, $"""
            SELECT count(*)::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.thiet_bi' AND doi_tuong_id = {ma}
            """);
        Assert.Equal(cacChuyen.Length.ToString(), Assert.Single(soNhatKy));
    }

    [CanKetNoiTestFact]
    public async Task UC12_ThietBiService_ThietBiKhongTonTai_TraFalse()
    {
        await using var dichVu = ThietBiGhiCsdlTests.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var dichVuThietBi = phamVi.ServiceProvider.GetRequiredService<IThietBiService>();

        Assert.False(await dichVuThietBi.NhanTraNguyenVenAsync("TB-KHONG-CO", CancellationToken.None));
        Assert.False(await dichVuThietBi.DatDangDuocMuonAsync("TB-KHONG-CO", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }

    [CanKetNoiTestFact]
    public async Task UC12_ThietBiService_KhongCoGiaoDich_NemInvalidOperationVaKhongGhi()
    {
        await using var dichVu = ThietBiGhiCsdlTests.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var dichVuThietBi = phamVi.ServiceProvider.GetRequiredService<IThietBiService>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dichVuThietBi.NhanTraNguyenVenAsync("TB-KHONG-CO", CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dichVuThietBi.DatDangDuocMuonAsync("TB-KHONG-CO", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }

    private static async Task<string> TaoThietBiAsync(IServiceProvider dv, string ma, Guid maLoai, Guid maPhong)
    {
        await dv.GetRequiredService<TaoThietBiHandler>().XuLyAsync(
            new ThietBiTaoMoiRequest(ma, maLoai, maPhong, $"Thiet bi {ma}", null, null, new DateOnly(2026, 1, 5), null, null, null, null, null, 1_000_000),
            CancellationToken.None);
        return ma;
    }

    private static Task DatTinhTrangAsync(AppDbContext db, string ma, string tinhTrang) =>
        db.Database.ExecuteSqlInterpolatedAsync($"UPDATE danh_muc.thiet_bi SET tinh_trang = {tinhTrang} WHERE ma_thiet_bi = {ma}");

    private static async Task<string> LayTinhTrangAsync(AppDbContext db, string ma) =>
        (await LayChuoiAsync(db, $"""
            SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {ma}
            """)).Single();

    private static Task<List<string>> LayChuoiAsync(AppDbContext db, FormattableString sql) =>
        db.Database.SqlQuery<string>(sql).ToListAsync();
}
