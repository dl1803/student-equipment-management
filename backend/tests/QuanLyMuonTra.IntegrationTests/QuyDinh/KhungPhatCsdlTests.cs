using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Application;
using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.Modules.ViPham;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.QuyDinh;

public sealed class KhungPhatCsdlTests
{
    private const string MaKhungDaDung = "019c0000-0000-7000-8000-0000000d0d01";
    private const string MaKhungChuaDung = "019c0000-0000-7000-8000-0000000d0d02";

    [CanKetNoiTestFact]
    public async Task UC13_TaoKhungPhat_TyLeThapPhan_GhiDongVaNhatKyThem()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var handler = phamVi.ServiceProvider.GetRequiredService<TaoKhungPhatHandler>();

        var ketQua = await handler.XuLyAsync(new KhungPhatRequest("Khung D06-N", 12.50m), CancellationToken.None);

        Assert.True(ketQua.DangSuDung);
        var tyLe = await LayChuoiAsync(db, $"""SELECT ty_le_phan_tram::text AS "Value" FROM quy_dinh.khung_phat WHERE ma_khung = {ketQua.MaKhung}""");
        Assert.Equal("12.50", Assert.Single(tyLe));
        var thaoTac = await LayChuoiAsync(db, $"""
            SELECT thao_tac AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = 'quy_dinh.khung_phat' AND doi_tuong_id = {ketQua.MaKhung.ToString()}
            """);
        Assert.Equal("THEM", Assert.Single(thaoTac));
    }

    [CanKetNoiTestFact]
    public async Task UC13_SuaVaTatKhungPhat_DoiDongVaGhiNhatKySua()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoKhungPhatHandler>();
        var sua = phamVi.ServiceProvider.GetRequiredService<SuaKhungPhatHandler>();
        var tat = phamVi.ServiceProvider.GetRequiredService<TatKhungPhatHandler>();
        var khung = await tao.XuLyAsync(new KhungPhatRequest("Khung cu", 20m), CancellationToken.None);

        var daSua = await sua.XuLyAsync(khung.MaKhung, new KhungPhatRequest("Khung moi", 25.75m), CancellationToken.None);
        var daTat = await tat.XuLyAsync(khung.MaKhung, CancellationToken.None);

        Assert.Equal("Khung moi", daSua.TenKhung);
        Assert.Equal(25.75m, daSua.TyLePhanTram);
        Assert.False(daTat.DangSuDung);
        var dangSuDung = await LayChuoiAsync(db, $"""SELECT dang_su_dung::text AS "Value" FROM quy_dinh.khung_phat WHERE ma_khung = {khung.MaKhung}""");
        Assert.Equal("false", Assert.Single(dangSuDung));
        var tenTruoc = await LayChuoiAsync(db, $"""
            SELECT gia_tri_truoc ->> 'TenKhung' AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'quy_dinh.khung_phat' AND doi_tuong_id = {khung.MaKhung.ToString()}
              AND gia_tri_truoc ->> 'TenKhung' IS NOT NULL
            """);
        Assert.Equal("Khung cu", Assert.Single(tenTruoc));
    }

    [CanKetNoiTestFact]
    public async Task UC13_XoaKhungPhat_ChuaDung_XoaDongVaGhiNhatKyXoa()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoKhungPhatHandler>();
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaKhungPhatHandler>();
        var khung = await tao.XuLyAsync(new KhungPhatRequest("Khung xoa", 10m), CancellationToken.None);

        await xoa.XuLyAsync(khung.MaKhung, CancellationToken.None);

        var conDong = await LayChuoiAsync(db, $"""SELECT ma_khung::text AS "Value" FROM quy_dinh.khung_phat WHERE ma_khung = {khung.MaKhung}""");
        Assert.Empty(conDong);
        var thaoTac = await LayChuoiAsync(db, $"""
            SELECT thao_tac AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = 'quy_dinh.khung_phat' AND doi_tuong_id = {khung.MaKhung.ToString()}
            """);
        Assert.Contains("XOA", thaoTac);
    }

    [CanKetNoiTestFact]
    public async Task UC13_XoaKhungPhat_DaDungTrongDuLieuThu_Nem409VaGiuDong()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await NapDuLieuThuMuonTraAsync(db);
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaKhungPhatHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => xoa.XuLyAsync(Guid.Parse(MaKhungDaDung), CancellationToken.None));

        Assert.Equal("KHUNG_PHAT_DA_DUNG_KHONG_XOA", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        var conDong = await LayChuoiAsync(db, $"""SELECT ma_khung::text AS "Value" FROM quy_dinh.khung_phat WHERE ma_khung = {Guid.Parse(MaKhungDaDung)}""");
        Assert.Single(conDong);
    }

    [CanKetNoiTestFact]
    public async Task UC13_XoaKhungPhat_KhungChuaDungTrongDuLieuThu_XoaDuoc()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await NapDuLieuThuMuonTraAsync(db);
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaKhungPhatHandler>();

        await xoa.XuLyAsync(Guid.Parse(MaKhungChuaDung), CancellationToken.None);

        var conDong = await LayChuoiAsync(db, $"""SELECT ma_khung::text AS "Value" FROM quy_dinh.khung_phat WHERE ma_khung = {Guid.Parse(MaKhungChuaDung)}""");
        Assert.Empty(conDong);
    }

    [CanKetNoiTestFact]
    public async Task UC13_DsKhungPhat_ChiDangSuDung_BoKhungDaTatVaSapTheoTyLe()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoKhungPhatHandler>();
        var tat = phamVi.ServiceProvider.GetRequiredService<TatKhungPhatHandler>();
        var ds = phamVi.ServiceProvider.GetRequiredService<DsKhungPhatHandler>();
        var dangDung = await tao.XuLyAsync(new KhungPhatRequest("Khung dang dung", 33.33m), CancellationToken.None);
        var daTat = await tao.XuLyAsync(new KhungPhatRequest("Khung da tat", 44.44m), CancellationToken.None);
        await tat.XuLyAsync(daTat.MaKhung, CancellationToken.None);

        var quay = await ds.XuLyAsync(true, 1, 100, CancellationToken.None);
        var quanTri = await ds.XuLyAsync(false, 1, 100, CancellationToken.None);

        Assert.Contains(quay.DanhSach, k => k.MaKhung == dangDung.MaKhung);
        Assert.DoesNotContain(quay.DanhSach, k => k.MaKhung == daTat.MaKhung);
        Assert.Contains(quanTri.DanhSach, k => k.MaKhung == daTat.MaKhung);
        var tyLe = quanTri.DanhSach.Select(k => k.TyLePhanTram).ToList();
        Assert.Equal(tyLe.Order().ToList(), tyLe);
    }

    private static async Task NapDuLieuThuMuonTraAsync(AppDbContext db)
    {
        var duongDan = Path.Combine(AppContext.BaseDirectory, "MuonTra", "D02-V-du-lieu-thu.sql");
        var sql = await File.ReadAllTextAsync(duongDan);
        await db.Database.ExecuteSqlRawAsync(sql);
    }

    private static Task<List<string>> LayChuoiAsync(AppDbContext db, FormattableString sql) =>
        db.Database.SqlQuery<string>(sql).ToListAsync();

    private static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(KhungPhat).Assembly])
            .AddQuyDinhModule()
            .AddViPhamModule()
            .BuildServiceProvider();
}
