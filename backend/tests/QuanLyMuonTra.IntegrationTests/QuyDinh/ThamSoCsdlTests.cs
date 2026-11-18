using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Application;
using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.QuyDinh;

public sealed class ThamSoCsdlTests
{
    [CanKetNoiTestFact]
    public async Task UC13_CapNhatThamSo_GhiDongVaNhatKyGiaTriTruocSau()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await DatThamSoMacDinhAsync(db);
        var handler = phamVi.ServiceProvider.GetRequiredService<CapNhatThamSoHandler>();

        var ketQua = await handler.XuLyAsync(
            new CapNhatThamSoRequest(4, 30, 15, 2, 3, 5, new TimeOnly(7, 30), new TimeOnly(17, 0)),
            CancellationToken.None);

        Assert.Equal(4, ketQua.SoNgayKhoa);
        Assert.Equal(new TimeOnly(7, 30), ketQua.GioMoCua);
        var soNgayKhoa = await LayChuoiAsync(db, $"""SELECT so_ngay_khoa::text AS "Value" FROM quy_dinh.tham_so_he_thong WHERE id = 1""");
        Assert.Equal("4", Assert.Single(soNgayKhoa));
        var truoc = await LayChuoiAsync(db, $"""
            SELECT gia_tri_truoc ->> 'SoNgayKhoa' AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'quy_dinh.tham_so_he_thong' AND doi_tuong_id = '1'
            """);
        var sau = await LayChuoiAsync(db, $"""
            SELECT gia_tri_sau ->> 'SoNgayKhoa' AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'quy_dinh.tham_so_he_thong' AND doi_tuong_id = '1'
            """);
        Assert.Equal("3", Assert.Single(truoc));
        Assert.Equal("4", Assert.Single(sau));
    }

    [CanKetNoiTestFact]
    public async Task UC13_CapNhatThamSo_GioMoSauGioDong_Nem422VaKhongDoiDong()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await DatThamSoMacDinhAsync(db);
        var handler = phamVi.ServiceProvider.GetRequiredService<CapNhatThamSoHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => handler.XuLyAsync(
            new CapNhatThamSoRequest(4, 30, 15, 2, 3, 5, new TimeOnly(17, 0), new TimeOnly(7, 30)),
            CancellationToken.None));

        Assert.Equal(422, loi.MaHttp);
        var soNgayKhoa = await LayChuoiAsync(db, $"""SELECT so_ngay_khoa::text AS "Value" FROM quy_dinh.tham_so_he_thong WHERE id = 1""");
        Assert.Equal("3", Assert.Single(soNgayKhoa));
    }

    private static async Task DatThamSoMacDinhAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("INSERT INTO quy_dinh.tham_so_he_thong (id) VALUES (1) ON CONFLICT (id) DO NOTHING");
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE quy_dinh.tham_so_he_thong SET so_ngay_khoa = 3, thoi_gian_giu_cho_phut = 30, an_han_phut = 15, so_gio_gia_han_toi_da = 2, nguong_vi_pham = 3, so_lan_dang_nhap_sai_toi_da = 5, gio_mo_cua = NULL, gio_dong_cua = NULL WHERE id = 1");
    }

    private static Task<List<string>> LayChuoiAsync(AppDbContext db, FormattableString sql) =>
        db.Database.SqlQuery<string>(sql).ToListAsync();

    private static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(KhungPhat).Assembly])
            .AddQuyDinhModule()
            .BuildServiceProvider();
}
