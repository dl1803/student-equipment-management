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

public sealed class CapNhatPhiTreCsdlTests
{
    [CanKetNoiTestFact]
    public async Task UC13_CapNhatPhiTre_GhiDongVaNhatKyPhiTruocSau()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoLoaiThietBiHandler>();
        var capNhat = phamVi.ServiceProvider.GetRequiredService<CapNhatPhiTreHandler>();
        var loai = await tao.XuLyAsync(new LoaiThietBiTaoMoiRequest("Loai D06-N", null, null, 30, 10000), CancellationToken.None);

        var ketQua = await capNhat.XuLyAsync(loai.MaLoai, new PhiTreRequest(50000), CancellationToken.None);

        Assert.Equal(new PhiTreDto(loai.MaLoai, 50000), ketQua);
        var phi = await LayChuoiAsync(db, $"""SELECT phi_tre_moi_ngay::text AS "Value" FROM danh_muc.loai_thiet_bi WHERE ma_loai = {loai.MaLoai}""");
        Assert.Equal("50000", Assert.Single(phi));
        var truoc = await LayChuoiAsync(db, $"""
            SELECT gia_tri_truoc ->> 'PhiTreMoiNgay' AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.loai_thiet_bi' AND doi_tuong_id = {loai.MaLoai.ToString()}
            """);
        var sau = await LayChuoiAsync(db, $"""
            SELECT gia_tri_sau ->> 'PhiTreMoiNgay' AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.loai_thiet_bi' AND doi_tuong_id = {loai.MaLoai.ToString()}
            """);
        Assert.Equal("10000", Assert.Single(truoc));
        Assert.Equal("50000", Assert.Single(sau));
    }

    [CanKetNoiTestFact]
    public async Task UC13_CapNhatPhiTre_PhiAm_Nem422VaKhongDoiDong()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoLoaiThietBiHandler>();
        var capNhat = phamVi.ServiceProvider.GetRequiredService<CapNhatPhiTreHandler>();
        var loai = await tao.XuLyAsync(new LoaiThietBiTaoMoiRequest("Loai D06-N", null, null, 30, 10000), CancellationToken.None);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => capNhat.XuLyAsync(loai.MaLoai, new PhiTreRequest(-1), CancellationToken.None));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
        var phi = await LayChuoiAsync(db, $"""SELECT phi_tre_moi_ngay::text AS "Value" FROM danh_muc.loai_thiet_bi WHERE ma_loai = {loai.MaLoai}""");
        Assert.Equal("10000", Assert.Single(phi));
    }

    [CanKetNoiTestFact]
    public async Task UC13_CapNhatPhiTre_LoaiKhongTonTai_Nem404()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var capNhat = phamVi.ServiceProvider.GetRequiredService<CapNhatPhiTreHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => capNhat.XuLyAsync(Guid.NewGuid(), new PhiTreRequest(1000), CancellationToken.None));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    private static Task<List<string>> LayChuoiAsync(AppDbContext db, FormattableString sql) =>
        db.Database.SqlQuery<string>(sql).ToListAsync();

    private static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(PhongThietBi).Assembly])
            .AddDanhMucModule()
            .BuildServiceProvider();
}
