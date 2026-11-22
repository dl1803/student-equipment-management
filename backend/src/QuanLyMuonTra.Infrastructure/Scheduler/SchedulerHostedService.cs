using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuanLyMuonTra.Infrastructure.Scheduler;

public sealed class SchedulerHostedService(BoChayLich boChay, ILogger<SchedulerHostedService> nhatKy) : BackgroundService
{
    public static readonly TimeSpan NhipQuet = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        nhatKy.LogInformation("Bộ chạy lịch khởi động, nhịp quét {NhipPhut} phút.", NhipQuet.TotalMinutes);

        using var boHen = new PeriodicTimer(NhipQuet);

        try
        {
            do
            {
                await ChayMotNhipAsync(stoppingToken);
            }
            while (await boHen.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task ChayMotNhipAsync(CancellationToken ct)
    {
        try
        {
            await boChay.ChayCacJobDenLuotAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception loi)
        {
            nhatKy.LogError(loi, "Nhịp quét của bộ chạy lịch gặp lỗi; nhịp sau vẫn chạy.");
        }
    }
}

