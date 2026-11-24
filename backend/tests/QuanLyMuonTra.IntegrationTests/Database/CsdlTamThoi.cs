using Microsoft.Extensions.Configuration;
using Npgsql;

namespace QuanLyMuonTra.IntegrationTests.Database;

public sealed class CsdlTamThoi : IAsyncDisposable
{
    private readonly string _chuoiKetNoiQuanTri;

    private CsdlTamThoi(string chuoiKetNoiQuanTri, string tenCsdl, string chuoiKetNoi)
    {
        _chuoiKetNoiQuanTri = chuoiKetNoiQuanTri;
        TenCsdl = tenCsdl;
        ChuoiKetNoi = chuoiKetNoi;
    }

    public string TenCsdl { get; }

    public string ChuoiKetNoi { get; }

    public static string LayChuoiKetNoiQuanTri()
    {
        var cauHinh = new ConfigurationBuilder()
            .AddUserSecrets(typeof(CsdlTamThoi).Assembly)
            .AddEnvironmentVariables()
            .Build();

        var rieng = cauHinh["ConnectionStrings:TestTam"];
        return string.IsNullOrWhiteSpace(rieng) ? KetNoiKiemThu.LayChuoiKetNoi() : rieng;
    }

    public static async Task<CsdlTamThoi> TaoAsync(CancellationToken ct = default)
    {
        var chuoiQuanTri = LayChuoiKetNoiQuanTri();
        var ten = "kt_tam_" + Guid.NewGuid().ToString("N");

        await using (var ketNoi = new NpgsqlConnection(chuoiQuanTri))
        {
            await ketNoi.OpenAsync(ct);
            await using var lenh = new NpgsqlCommand($"CREATE DATABASE \"{ten}\"", ketNoi);
            await lenh.ExecuteNonQueryAsync(ct);
        }

        var chuoiKetNoi = new NpgsqlConnectionStringBuilder(chuoiQuanTri) { Database = ten, Pooling = false }.ConnectionString;
        return new CsdlTamThoi(chuoiQuanTri, ten, chuoiKetNoi);
    }

    public static async Task<bool> TonTaiAsync(string tenCsdl, CancellationToken ct = default)
    {
        await using var ketNoi = new NpgsqlConnection(LayChuoiKetNoiQuanTri());
        await ketNoi.OpenAsync(ct);
        await using var lenh = new NpgsqlCommand("SELECT count(*) FROM pg_database WHERE datname = @ten", ketNoi);
        lenh.Parameters.AddWithValue("ten", tenCsdl);
        return Convert.ToInt64(await lenh.ExecuteScalarAsync(ct)) > 0;
    }

    public async Task<NpgsqlConnection> MoAsync(CancellationToken ct = default)
    {
        var ketNoi = new NpgsqlConnection(ChuoiKetNoi);
        await ketNoi.OpenAsync(ct);
        return ketNoi;
    }

    public async ValueTask DisposeAsync()
    {
        await using var ketNoi = new NpgsqlConnection(_chuoiKetNoiQuanTri);
        await ketNoi.OpenAsync();
        await using var lenh = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{TenCsdl}\" WITH (FORCE)", ketNoi);
        await lenh.ExecuteNonQueryAsync();
    }
}
