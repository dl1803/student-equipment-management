using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure.Scheduler;

public sealed class BoChayLich(IServiceScopeFactory nhaMayPhamVi, IClock dongHo, ILogger<BoChayLich> nhatKy)
{
    public static readonly TimeSpan DungSaiNhip = TimeSpan.FromSeconds(5);

    public static readonly TimeSpan NguongTre = TimeSpan.FromMinutes(5);

    private readonly Dictionary<string, DateTimeOffset> _lanChayCuoi = new();

    public async Task ChayCacJobDenLuotAsync(CancellationToken ct)
    {
        var bayGio = dongHo.UtcNow;

        List<(string TenJob, TimeSpan ChuKy)> dsJob;
        using (var phamVi = nhaMayPhamVi.CreateScope())
        {
            dsJob = phamVi.ServiceProvider.GetServices<IJob>()
                .Select(job => (job.TenJob, job.ChuKy))
                .ToList();
        }

        foreach (var (tenJob, chuKy) in dsJob)
        {
            if (!DenLuot(tenJob, chuKy, bayGio))
            {
                continue;
            }

            CanhBaoNeuTre(tenJob, chuKy, bayGio);
            await ChayMotJobAsync(tenJob, bayGio, ct);
        }
    }

    public static long SuyKhoaJob(string tenJob)
    {
        Span<byte> bam = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes($"QuanLyMuonTra.Job:{tenJob}"), bam);

        return BinaryPrimitives.ReadInt64LittleEndian(bam);
    }

    private bool DenLuot(string tenJob, TimeSpan chuKy, DateTimeOffset bayGio) =>
        !_lanChayCuoi.TryGetValue(tenJob, out var lanCuoi) || bayGio - lanCuoi >= chuKy - DungSaiNhip;

    private void CanhBaoNeuTre(string tenJob, TimeSpan chuKy, DateTimeOffset bayGio)
    {
        if (_lanChayCuoi.TryGetValue(tenJob, out var lanCuoi) && bayGio - lanCuoi > chuKy + NguongTre)
        {
            nhatKy.LogWarning(
                "Job {TenJob} trễ {SoPhut:F1} phút so với chu kỳ {ChuKyPhut:F1} phút.",
                tenJob,
                (bayGio - lanCuoi).TotalMinutes,
                chuKy.TotalMinutes);
        }
    }

    private async Task ChayMotJobAsync(string tenJob, DateTimeOffset bayGio, CancellationToken ct)
    {
        var daoDong = Stopwatch.StartNew();

        try
        {
            await using var phamVi = nhaMayPhamVi.CreateAsyncScope();
            var job = phamVi.ServiceProvider.GetServices<IJob>().First(j => j.TenJob == tenJob);
            var db = phamVi.ServiceProvider.GetRequiredService<DbContext>();

            await using var giaoDich = await db.Database.BeginTransactionAsync(ct);

            var khoa = SuyKhoaJob(tenJob);
            var layDuocKhoa = await db.Database
                .SqlQuery<bool>($"SELECT pg_try_advisory_xact_lock({khoa}) AS \"Value\"")
                .SingleAsync(ct);
            if (!layDuocKhoa)
            {
                nhatKy.LogInformation(
                    "Job {MaJob} {TenJob} bỏ qua tick vì tiến trình khác đang giữ khóa.",
                    job.MaJob,
                    tenJob);

                _lanChayCuoi[tenJob] = bayGio;

                return;
            }

            var soPhanTu = await job.ChayAsync(ct);
            await giaoDich.CommitAsync(ct);

            _lanChayCuoi[tenJob] = bayGio;

            nhatKy.LogInformation(
                "Job {MaJob} {TenJob} xử lý {SoPhanTu} phần tử trong {ThoiLuongMs} ms.",
                job.MaJob,
                tenJob,
                soPhanTu,
                daoDong.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception loi)
        {
            nhatKy.LogWarning(
                loi,
                "Job {TenJob} lỗi sau {ThoiLuongMs} ms, giao dịch đã hoàn tác; các job khác vẫn chạy.",
                tenJob,
                daoDong.ElapsedMilliseconds);
        }
    }
}

