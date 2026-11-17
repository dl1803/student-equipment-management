using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

public sealed class GhiPhongLoaiNhatKyCsdlTests
{
    [CanKetNoiTestFact]
    public async Task UC12_TaoPhong_GhiNhatKyThemTrongCungGiaoDich()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var handler = phamVi.ServiceProvider.GetRequiredService<TaoPhongThietBiHandler>();

        var ketQua = await handler.XuLyAsync(new PhongThietBiRequest("Phong D05-N", "Co so D05-N"), CancellationToken.None);

        var thaoTac = await db.Database.SqlQuery<string>($"""
            SELECT thao_tac AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = 'danh_muc.phong_thiet_bi' AND doi_tuong_id = {ketQua.MaPhong.ToString()}
            """).ToListAsync();
        Assert.Equal("THEM", Assert.Single(thaoTac));
    }

    [CanKetNoiTestFact]
    public async Task UC12_SuaPhong_GhiNhatKySuaChiCotBiDoi()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoPhongThietBiHandler>();
        var sua = phamVi.ServiceProvider.GetRequiredService<SuaPhongThietBiHandler>();
        var phong = await tao.XuLyAsync(new PhongThietBiRequest("Phong cu", "Co so chung"), CancellationToken.None);

        await sua.XuLyAsync(phong.MaPhong, new PhongThietBiRequest("Phong moi", "Co so chung"), CancellationToken.None);

        var truoc = await db.Database.SqlQuery<string>($"""
            SELECT gia_tri_truoc::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.phong_thiet_bi' AND doi_tuong_id = {phong.MaPhong.ToString()}
            """).ToListAsync();
        var sau = await db.Database.SqlQuery<string>($"""
            SELECT gia_tri_sau::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.phong_thiet_bi' AND doi_tuong_id = {phong.MaPhong.ToString()}
            """).ToListAsync();
        Assert.Contains("Phong cu", Assert.Single(truoc));
        Assert.Contains("Phong moi", Assert.Single(sau));
    }

    [CanKetNoiTestFact]
    public async Task UC12_TaoLoai_GhiNhatKyThemCoPhiBanDau()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var handler = phamVi.ServiceProvider.GetRequiredService<TaoLoaiThietBiHandler>();

        var ketQua = await handler.XuLyAsync(new LoaiThietBiTaoMoiRequest("Loai D05-N", null, null, 30, 12345), CancellationToken.None);

        var giaTriSau = await db.Database.SqlQuery<string>($"""
            SELECT gia_tri_sau::text AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'THEM' AND doi_tuong = 'danh_muc.loai_thiet_bi' AND doi_tuong_id = {ketQua.MaLoai.ToString()}
            """).ToListAsync();
        var json = Assert.Single(giaTriSau);
        Assert.Contains("Loai D05-N", json);
        Assert.Contains("PhiTreMoiNgay", json);
    }

    private static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(PhongThietBi).Assembly])
            .AddSingleton<ICurrentUser>(new NhatKyTests.NguoiDungGia())
            .AddDanhMucModule()
            .BuildServiceProvider();
}
