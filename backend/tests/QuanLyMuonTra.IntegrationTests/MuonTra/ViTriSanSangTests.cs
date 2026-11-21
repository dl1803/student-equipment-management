using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class ViTriSanSangTests
{
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d8b01");
    private static readonly Guid X = Guid.Parse("019c0000-0000-7000-8000-0000000d8c01");
    private static readonly Guid Y = Guid.Parse("019c0000-0000-7000-8000-0000000d8c02");
    private static readonly DateTimeOffset BayGio = new(2026, 11, 20, 3, 0, 0, TimeSpan.Zero);
    private static readonly CancellationToken Ct = CancellationToken.None;

    private sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }

    private static async Task<YeuCauMuonSvTests.YeuCauMuonTestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        await gd.NapDuLieuThuAsync("D08-V-du-lieu-thu.sql");
        await gd.NapDuLieuThuAsync("D08-V-nguon-du-lieu-thu.sql");
        var options = new DbContextOptionsBuilder<YeuCauMuonSvTests.YeuCauMuonTestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new YeuCauMuonSvTests.YeuCauMuonTestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich, Ct);
        return db;
    }

    [CanKetNoiTestFact]
    public async Task UC07_DsViTriSanSang_NhomTheoKeTuVaBoQuaThietBiKhongSanSang()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);

        var ds = await new AvailabilityService(db, new DongHoCoDinh(BayGio)).DsViTriSanSangAsync(P1, X, Ct);

        Assert.Equal(3, ds.Count);
        Assert.Equal(new ViTriSanSangDto("K1", "T1", 2), ds[0]);
        Assert.Equal(new ViTriSanSangDto("K1", "T2", 1), ds[1]);
        Assert.Equal(new ViTriSanSangDto("K2", null, 1), ds[2]);
    }

    [CanKetNoiTestFact]
    public async Task UC07_DsViTriSanSang_TheoLoaiKhac()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);

        var ds = await new AvailabilityService(db, new DongHoCoDinh(BayGio)).DsViTriSanSangAsync(P1, Y, Ct);

        Assert.Equal(2, ds.Count);
        Assert.Equal(new ViTriSanSangDto("K1", "T1", 1), ds[0]);
        Assert.Equal(new ViTriSanSangDto("K3", "T3", 2), ds[1]);
    }

    [CanKetNoiTestFact]
    public async Task UC07_TinhKhaDung_TruGiuChoConHanVaKhongTruGiuChoDungHan()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);

        var khaDung = await new AvailabilityService(db, new DongHoCoDinh(BayGio)).TinhKhaDungAsync(P1, X, Ct);

        Assert.Equal(1, khaDung);
    }
}
