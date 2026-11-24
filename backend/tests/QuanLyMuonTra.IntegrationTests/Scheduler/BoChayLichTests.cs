using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Scheduler;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.Scheduler;

public sealed class BoChayLichTests
{
    private static readonly DateTimeOffset MocDau = new(2026, 11, 22, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task UC17_TickDau_ChayMoiJobNgayVaTrongGiaoDich()
    {
        var jobPhut = new GhiNhanJob();
        var jobGio = new GhiNhanJob();
        await using var moiTruong = MoiTruong.Tao(
            MocDau,
            ("JPhut", TimeSpan.FromMinutes(1), jobPhut),
            ("JGio", TimeSpan.FromHours(1), jobGio));

        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);

        Assert.Equal(1, jobPhut.SoLanChay);
        Assert.Equal(1, jobGio.SoLanChay);
        Assert.True(jobPhut.CoGiaoDichKhiChay);
        Assert.Contains(moiTruong.NhatKy.Dong, d => d.Muc == LogLevel.Information && d.NoiDung.Contains("xử lý 1 phần tử"));
    }

    [Fact]
    public async Task UC17_Tick_ChiChayJobDenLuotTheoChuKyRieng()
    {
        var jobPhut = new GhiNhanJob();
        var jobGio = new GhiNhanJob();
        await using var moiTruong = MoiTruong.Tao(
            MocDau,
            ("JPhut", TimeSpan.FromMinutes(1), jobPhut),
            ("JGio", TimeSpan.FromHours(1), jobGio));

        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);

        moiTruong.DongHo.UtcNow = MocDau.AddSeconds(30);
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(1, jobPhut.SoLanChay);

        moiTruong.DongHo.UtcNow = MocDau.AddMinutes(1).AddSeconds(-2);
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(2, jobPhut.SoLanChay);
        Assert.Equal(1, jobGio.SoLanChay);

        moiTruong.DongHo.UtcNow = MocDau.AddHours(1);
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(3, jobPhut.SoLanChay);
        Assert.Equal(2, jobGio.SoLanChay);
    }

    [Fact]
    public async Task UC17_JobLoi_KhongLamDungJobKhac_GhiCanhBao_VaNhaKhoa()
    {
        var jobLoi = new GhiNhanJob { NemLoi = true };
        var jobTot = new GhiNhanJob();
        await using var moiTruong = MoiTruong.Tao(
            MocDau,
            ("JLoi", TimeSpan.FromMinutes(1), jobLoi),
            ("JTot", TimeSpan.FromMinutes(1), jobTot));

        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);

        Assert.Equal(1, jobLoi.SoLanChay);
        Assert.Equal(1, jobTot.SoLanChay);
        Assert.Contains(moiTruong.NhatKy.Dong, d => d.Muc == LogLevel.Warning && d.NoiDung.Contains("JLoi"));
        Assert.True(await LayDuocKhoaAsync("JLoi"));
    }

    [Fact]
    public async Task UC17_KhoaDangDuocGiu_BoQuaTick_RoiChayLaiKhiKhoaDuocNha()
    {
        var job = new GhiNhanJob();
        await using var moiTruong = MoiTruong.Tao(MocDau, ("JKhoa", TimeSpan.FromMinutes(1), job));

        await using var ketNoi = await KetNoiKiemThu.MoAsync();
        await using var giaoDichGiu = await ketNoi.BeginTransactionAsync();
        await using (var lenh = new NpgsqlCommand("SELECT pg_advisory_xact_lock(@khoa)", ketNoi, giaoDichGiu))
        {
            lenh.Parameters.AddWithValue("khoa", BoChayLich.SuyKhoaJob("JKhoa"));
            await lenh.ExecuteNonQueryAsync();
        }

        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(0, job.SoLanChay);
        Assert.Contains(moiTruong.NhatKy.Dong, d => d.NoiDung.Contains("bỏ qua tick"));

        await giaoDichGiu.RollbackAsync();

        moiTruong.DongHo.UtcNow = MocDau.AddMinutes(1);
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(1, job.SoLanChay);
    }

    [Fact]
    public async Task UC17_KhoaDangDuocGiu_GhiNhanLanChay_TranhDoubleRunNhipKeTiep()
    {
        var jobGio = new GhiNhanJob();
        await using var moiTruong = MoiTruong.Tao(MocDau, ("JGioMulti", TimeSpan.FromHours(1), jobGio));

        await using var ketNoi = await KetNoiKiemThu.MoAsync();
        await using var giaoDichGiu = await ketNoi.BeginTransactionAsync();
        await using (var lenh = new NpgsqlCommand("SELECT pg_advisory_xact_lock(@khoa)", ketNoi, giaoDichGiu))
        {
            lenh.Parameters.AddWithValue("khoa", BoChayLich.SuyKhoaJob("JGioMulti"));
            await lenh.ExecuteNonQueryAsync();
        }

        // Nhịp 1 (phút 0): Khóa bị instance khác giữ -> bỏ qua tick nhưng ghi nhận lần chạy để tránh double-run
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(0, jobGio.SoLanChay);
        Assert.Contains(moiTruong.NhatKy.Dong, d => d.NoiDung.Contains("bỏ qua tick"));

        // Instance khác xử lý xong và nhả khóa
        await giaoDichGiu.RollbackAsync();

        // Nhịp 2 (phút 1): Khóa đã nhả, nhưng vì chu kỳ là 1 giờ nên instance này KHÔNG chạy lại ngay ở phút 1
        moiTruong.DongHo.UtcNow = MocDau.AddMinutes(1);
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(0, jobGio.SoLanChay);

        // Nhịp 3 (sau 1 giờ): Đúng chu kỳ 1 giờ, job mới chạy lại
        moiTruong.DongHo.UtcNow = MocDau.AddHours(1);
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(1, jobGio.SoLanChay);
    }

    [Fact]
    public async Task UC17_JobLoi_KhongGhiNhanLanChay_ChoPhepRetryNhipKeTiep()
    {
        var jobGio = new GhiNhanJob { NemLoi = true };
        await using var moiTruong = MoiTruong.Tao(MocDau, ("JGioLoi", TimeSpan.FromHours(1), jobGio));

        // Nhịp 1: Job chạy và bị lỗi ngoại lệ (transient error)
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(1, jobGio.SoLanChay);
        Assert.Contains(moiTruong.NhatKy.Dong, d => d.Muc == LogLevel.Warning && d.NoiDung.Contains("JGioLoi"));

        // Lỗi kết nối được khắc phục
        jobGio.NemLoi = false;

        // Nhịp 2 (1 phút sau): Vì lần trước lỗi không ghi nhận mốc chạy thành công, job được retry ngay ở phút 1 thay vì chờ 1 giờ
        moiTruong.DongHo.UtcNow = MocDau.AddMinutes(1);
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.Equal(2, jobGio.SoLanChay);
    }

    [Fact]
    public async Task UC17_JobTreHonNguong_GhiCanhBaoTre()
    {
        var job = new GhiNhanJob();
        await using var moiTruong = MoiTruong.Tao(MocDau, ("JTre", TimeSpan.FromMinutes(1), job));

        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        moiTruong.DongHo.UtcNow = MocDau.AddMinutes(1);
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);
        Assert.DoesNotContain(moiTruong.NhatKy.Dong, d => d.Muc == LogLevel.Warning);

        moiTruong.DongHo.UtcNow = MocDau.AddMinutes(8);
        await moiTruong.BoChay.ChayCacJobDenLuotAsync(CancellationToken.None);

        Assert.Contains(moiTruong.NhatKy.Dong, d => d.Muc == LogLevel.Warning && d.NoiDung.Contains("trễ"));
        Assert.Equal(3, job.SoLanChay);
    }

    private static async Task<bool> LayDuocKhoaAsync(string tenJob)
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();
        await using var giaoDich = await ketNoi.BeginTransactionAsync();
        await using var lenh = new NpgsqlCommand("SELECT pg_try_advisory_xact_lock(@khoa)", ketNoi, giaoDich);
        lenh.Parameters.AddWithValue("khoa", BoChayLich.SuyKhoaJob(tenJob));
        var ketQua = (bool)(await lenh.ExecuteScalarAsync())!;
        await giaoDich.RollbackAsync();

        return ketQua;
    }

    private sealed class GhiNhanJob
    {
        public int SoLanChay { get; set; }

        public bool CoGiaoDichKhiChay { get; set; }

        public bool NemLoi { get; set; }
    }

    private sealed class JobGia(string ten, TimeSpan chuKy, GhiNhanJob ghiNhan, DbContext db) : IJob
    {
        public string MaJob => ten;

        public string TenJob => ten;

        public TimeSpan ChuKy => chuKy;

        public Task<int> ChayAsync(CancellationToken ct)
        {
            ghiNhan.SoLanChay++;
            ghiNhan.CoGiaoDichKhiChay = db.Database.CurrentTransaction is not null;
            if (ghiNhan.NemLoi)
            {
                throw new InvalidOperationException("Lỗi giả lập của job.");
            }

            return Task.FromResult(1);
        }
    }

    private sealed class DongHoDieuKhien : IClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }

    private sealed class NhatKyThuThap : ILogger<BoChayLich>
    {
        public List<(LogLevel Muc, string NoiDung)> Dong { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Dong.Add((logLevel, formatter(state, exception)));
    }

    private sealed class MoiTruong : IAsyncDisposable
    {
        private readonly ServiceProvider _dichVu;

        private MoiTruong(ServiceProvider dichVu, DongHoDieuKhien dongHo, NhatKyThuThap nhatKy)
        {
            _dichVu = dichVu;
            DongHo = dongHo;
            NhatKy = nhatKy;
            BoChay = new BoChayLich(dichVu.GetRequiredService<IServiceScopeFactory>(), dongHo, nhatKy);
        }

        public DongHoDieuKhien DongHo { get; }

        public NhatKyThuThap NhatKy { get; }

        public BoChayLich BoChay { get; }

        public static MoiTruong Tao(DateTimeOffset bayGio, params (string Ten, TimeSpan ChuKy, GhiNhanJob GhiNhan)[] dsJob)
        {
            var dongHo = new DongHoDieuKhien { UtcNow = bayGio };
            var dichVu = new ServiceCollection()
                .AddLogging()
                .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi());
            dichVu.AddSingleton<IClock>(dongHo);
            foreach (var (ten, chuKy, ghiNhan) in dsJob)
            {
                dichVu.AddScoped<IJob>(sp => new JobGia(ten, chuKy, ghiNhan, sp.GetRequiredService<DbContext>()));
            }

            return new MoiTruong(dichVu.BuildServiceProvider(), dongHo, new NhatKyThuThap());
        }

        public ValueTask DisposeAsync() => _dichVu.DisposeAsync();
    }
}

