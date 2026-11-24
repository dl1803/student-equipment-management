using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace QuanLyMuonTra.IntegrationTests.Database;

public static class BundleMigration
{
    public sealed record KetQuaChay(int MaThoat, string DauRa);

    public static string? LayDuongDan()
    {
        return DocCauHinh("MigrationBundle:Path");
    }

    public static string? LayPhienBanTruoc()
    {
        return DocCauHinh("MigrationBundle:PhienBanTruoc");
    }

    public static string BatBuocDuongDan()
    {
        return LayDuongDan() ?? throw new InvalidOperationException("Thiếu MigrationBundle:Path.");
    }

    public static string BatBuocPhienBanTruoc()
    {
        return LayPhienBanTruoc() ?? throw new InvalidOperationException("Thiếu MigrationBundle:PhienBanTruoc.");
    }

    public static async Task<KetQuaChay> ChayAsync(string duongDan, string chuoiKetNoi, string? phienBan, CancellationToken ct = default)
    {
        var thongTin = new ProcessStartInfo(duongDan)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        if (!string.IsNullOrWhiteSpace(phienBan))
        {
            thongTin.ArgumentList.Add(phienBan);
        }

        thongTin.ArgumentList.Add("--connection");
        thongTin.ArgumentList.Add(chuoiKetNoi);

        using var tienTrinh = new Process { StartInfo = thongTin };
        using var huy = CancellationTokenSource.CreateLinkedTokenSource(ct);
        huy.CancelAfter(TimeSpan.FromMinutes(5));

        tienTrinh.Start();
        var dauRaChuan = tienTrinh.StandardOutput.ReadToEndAsync(huy.Token);
        var dauRaLoi = tienTrinh.StandardError.ReadToEndAsync(huy.Token);
        try
        {
            await tienTrinh.WaitForExitAsync(huy.Token);
        }
        catch (OperationCanceledException)
        {
            tienTrinh.Kill(entireProcessTree: true);
            throw;
        }

        return new KetQuaChay(tienTrinh.ExitCode, await dauRaChuan + await dauRaLoi);
    }

    private static string? DocCauHinh(string khoa)
    {
        var cauHinh = new ConfigurationBuilder()
            .AddUserSecrets(typeof(BundleMigration).Assembly)
            .AddEnvironmentVariables()
            .Build();

        var giaTri = cauHinh[khoa];
        return string.IsNullOrWhiteSpace(giaTri) ? null : giaTri;
    }
}
