using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

public sealed class NgungSuDungCsdlTests
{
    private const string MaThietBi = "D02-TB-01";
    private const string MaPhongP1 = "019c0000-0000-7000-8000-0000000d0b01";
    private const string MaLoaiL1 = "019c0000-0000-7000-8000-0000000d0c01";

    [CanKetNoiTestFact]
    public async Task UC12_NgungSuDungThietBi_DatCoGiamKhaDungVaGhiNhatKySua()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var khaDung = phamVi.ServiceProvider.GetRequiredService<IAvailabilityService>();
        var handler = phamVi.ServiceProvider.GetRequiredService<NgungSuDungThietBiHandler>();
        var maPhong = Guid.Parse(MaPhongP1);
        var maLoai = Guid.Parse(MaLoaiL1);
        var truoc = await khaDung.TinhKhaDungAsync(maPhong, maLoai, CancellationToken.None);
        Assert.True(truoc >= 1);

        var ketQua = await handler.XuLyAsync(MaThietBi, CancellationToken.None);

        Assert.True(ketQua.NgungSuDung);
        Assert.Equal(truoc - 1, await khaDung.TinhKhaDungAsync(maPhong, maLoai, CancellationToken.None));
        var giaTriSau = await XoaNgungTestSupport.LayChuoiAsync(db, $"""
            SELECT gia_tri_sau ->> 'NgungSuDung' AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.thiet_bi' AND doi_tuong_id = {MaThietBi}
            """);
        Assert.Equal("true", Assert.Single(giaTriSau));
    }

    [CanKetNoiTestFact]
    public async Task UC12_NgungSuDungThietBi_GoiLaiKhongGhiThemNhatKy()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var handler = phamVi.ServiceProvider.GetRequiredService<NgungSuDungThietBiHandler>();

        await handler.XuLyAsync(MaThietBi, CancellationToken.None);
        var lanHai = await handler.XuLyAsync(MaThietBi, CancellationToken.None);

        Assert.True(lanHai.NgungSuDung);
        var soDong = await XoaNgungTestSupport.DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.thiet_bi' AND doi_tuong_id = {MaThietBi}
            """);
        Assert.Equal(1, soDong);
    }

    [CanKetNoiTestFact]
    public async Task UC12_NgungSuDungThietBi_GiuKhoaTuVanCapPhongLoaiTrongGiaoDich()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var handler = phamVi.ServiceProvider.GetRequiredService<NgungSuDungThietBiHandler>();
        var truoc = await DemKhoaTuVanAsync(db);

        await handler.XuLyAsync(MaThietBi, CancellationToken.None);

        Assert.Equal(truoc + 1, await DemKhoaTuVanAsync(db));
    }

    [CanKetNoiTestFact]
    public async Task UC12_NgungSuDungThietBi_KhongTonTai_Nem404()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var handler = phamVi.ServiceProvider.GetRequiredService<NgungSuDungThietBiHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync("KHONG-CO-D08", CancellationToken.None));

        Assert.Equal(404, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC12_NgungSuDungLoai_DatCoVaBienMatKhoiDanhSachDangSuDung()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var handler = phamVi.ServiceProvider.GetRequiredService<NgungSuDungLoaiThietBiHandler>();
        var reader = phamVi.ServiceProvider.GetRequiredService<IDanhMucReader>();
        var maLoai = Guid.Parse(MaLoaiL1);
        Assert.Contains(await reader.DsLoaiDangSuDungAsync(CancellationToken.None), l => l.MaLoai == maLoai);

        var ketQua = await handler.XuLyAsync(maLoai, CancellationToken.None);

        Assert.True(ketQua.NgungSuDung);
        Assert.DoesNotContain(await reader.DsLoaiDangSuDungAsync(CancellationToken.None), l => l.MaLoai == maLoai);
        var giaTriSau = await XoaNgungTestSupport.LayChuoiAsync(db, $"""
            SELECT gia_tri_sau ->> 'NgungSuDung' AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.loai_thiet_bi' AND doi_tuong_id = {maLoai.ToString()}
            """);
        Assert.Equal("true", Assert.Single(giaTriSau));
    }

    [CanKetNoiTestFact]
    public async Task UC12_NgungSuDungLoai_KhongTonTai_Nem404()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var handler = phamVi.ServiceProvider.GetRequiredService<NgungSuDungLoaiThietBiHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(404, loi.MaHttp);
    }

    private static Task<int> DemKhoaTuVanAsync(AppDbContext db) =>
        XoaNgungTestSupport.DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM pg_locks
            WHERE locktype = 'advisory' AND pid = pg_backend_pid() AND granted
            """);
}
