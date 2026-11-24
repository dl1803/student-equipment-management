using Microsoft.Extensions.Configuration;
using Npgsql;

namespace QuanLyMuonTra.IntegrationTests.Database;

public static class KetNoiKiemThu
{
    public static string LayChuoiKetNoi()
    {
        var cauHinh = new ConfigurationBuilder()
            .AddUserSecrets(typeof(KetNoiKiemThu).Assembly)
            .AddEnvironmentVariables()
            .Build();

        var chuoi = cauHinh["ConnectionStrings:Test"];
        if (string.IsNullOrWhiteSpace(chuoi))
        {
            throw new InvalidOperationException(
                "Thiếu ConnectionStrings:Test. Đặt bằng dotnet user-secrets (máy dev) hoặc biến môi trường ConnectionStrings__Test (CI).");
        }

        return chuoi;
    }

    public static async Task<NpgsqlConnection> MoAsync()
    {
        var ketNoi = new NpgsqlConnection(LayChuoiKetNoi());
        await ketNoi.OpenAsync();
        return ketNoi;
    }
}

