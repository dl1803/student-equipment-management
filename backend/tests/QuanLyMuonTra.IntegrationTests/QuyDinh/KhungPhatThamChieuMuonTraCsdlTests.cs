using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.DanhMuc;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.MuonTra;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Application;
using QuanLyMuonTra.Modules.ViPham;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.QuyDinh;

public sealed class KhungPhatThamChieuMuonTraCsdlTests
{
    private const string MaKhungDaDung = "019c0000-0000-7000-8000-0000000d0d01";
    private const string MaKhungChuaDung = "019c0000-0000-7000-8000-0000000d0d02";

    [CanKetNoiTestFact]
    public async Task UC13_XoaKhungPhat_ChiCoProviderMuonTra_KhungDaGanChiTietPhieuMuon_Nem409()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVu(s => s.AddQuyDinhModule().AddMuonTraModule());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaKhungPhatHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => xoa.XuLyAsync(Guid.Parse(MaKhungDaDung), CancellationToken.None));

        Assert.Equal("KHUNG_PHAT_DA_DUNG_KHONG_XOA", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC13_XoaKhungPhat_CaMuonTraVaViPham_KhungChuaDung_XoaDuoc()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVu(s => s.AddQuyDinhModule().AddMuonTraModule().AddViPhamModule());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaKhungPhatHandler>();

        await xoa.XuLyAsync(Guid.Parse(MaKhungChuaDung), CancellationToken.None);

        var conDong = await XoaNgungTestSupport.DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM quy_dinh.khung_phat WHERE ma_khung = {Guid.Parse(MaKhungChuaDung)}
            """);
        Assert.Equal(0, conDong);
    }
}
