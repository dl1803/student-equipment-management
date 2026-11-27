using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.Infrastructure.Scheduler;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

[Collection(ThanhLyDuLieu.TenBoSuuTap)]
public sealed class NhacBaoTriJobThatTests
{
    private static void ChiChayJ6(IServiceCollection dichVu)
    {
        dichVu.RemoveAll<IJob>();
        dichVu.AddScoped<IJob, NhacBaoTriJob>();
    }

    private static BoChayLich TaoBoChay(ServiceProvider dichVu) =>
        new(
            dichVu.GetRequiredService<IServiceScopeFactory>(),
            dichVu.GetRequiredService<IClock>(),
            NullLogger<BoChayLich>.Instance);

    private static Task<int> DemThongBaoAsync(ServiceProvider dichVu) =>
        ThanhLyDuLieu.DemAsync(dichVu, $"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao
            WHERE ma_nv_nhan IN ({ThanhLyDuLieu.MaNv1}::uuid, {ThanhLyDuLieu.MaNv2}::uuid, {ThanhLyDuLieu.MaNv3}::uuid)
            """);

    [CanKetNoiTestFact]
    public async Task UC17_J6_KhoaJobDangDuocGiu_BoQuaTickRoiChayKhiNhaKhoa()
    {
        await ThanhLyDuLieu.ChayAsync(
            async dichVu =>
            {
                await using var phamViGiu = dichVu.CreateAsyncScope();
                var dbGiu = phamViGiu.ServiceProvider.GetRequiredService<AppDbContext>();
                await using var giaoDichGiu = await dbGiu.Database.BeginTransactionAsync();
                var khoa = BoChayLich.SuyKhoaJob("NhacBaoTri");
                await dbGiu.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({khoa})");

                await TaoBoChay(dichVu).ChayCacJobDenLuotAsync(CancellationToken.None);
                Assert.Equal(0, await DemThongBaoAsync(dichVu));

                await giaoDichGiu.RollbackAsync();

                await TaoBoChay(dichVu).ChayCacJobDenLuotAsync(CancellationToken.None);
                Assert.Equal(5, await DemThongBaoAsync(dichVu));
            },
            ChiChayJ6);
    }

    [CanKetNoiTestFact]
    public async Task UC17_J6_HaiInstanceChayDongThoi_KhongSinhThongBaoTrung()
    {
        await ThanhLyDuLieu.ChayAsync(
            async dichVu =>
            {
                await Task.WhenAll(
                    TaoBoChay(dichVu).ChayCacJobDenLuotAsync(CancellationToken.None),
                    TaoBoChay(dichVu).ChayCacJobDenLuotAsync(CancellationToken.None));

                Assert.Equal(5, await DemThongBaoAsync(dichVu));
                Assert.Equal(3, await ThanhLyDuLieu.DemAsync(dichVu, $"""
                    SELECT count(DISTINCT khoa_trung_lap)::int AS "Value" FROM thong_bao.thong_bao
                    WHERE khoa_trung_lap LIKE 'BAO_TRI:D16-TB-%'
                    """));
            },
            ChiChayJ6);
    }
}
