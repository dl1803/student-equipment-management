using Microsoft.Extensions.Configuration;
using QuanLyMuonTra.Infrastructure.LuuTru;

namespace QuanLyMuonTra.IntegrationTests.Anh;

public static class CauHinhStorageThat
{
    public static SupabaseStorageOptions? Doc()
    {
        var cauHinh = new ConfigurationBuilder()
            .AddUserSecrets(typeof(CauHinhStorageThat).Assembly)
            .AddEnvironmentVariables()
            .Build();

        var url = cauHinh["Supabase:Url"];
        var khoa = cauHinh["Supabase:ServiceRoleKey"];

        return string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(khoa)
            ? null
            : new SupabaseStorageOptions(url, khoa);
    }
}

public sealed class CanStorageThatFactAttribute : FactAttribute
{
    public CanStorageThatFactAttribute()
    {
        if (CauHinhStorageThat.Doc() is null)
        {
            Skip = "Chưa cấu hình Supabase:Url và Supabase:ServiceRoleKey (user-secrets của IntegrationTests hoặc biến môi trường Supabase__Url, Supabase__ServiceRoleKey).";
        }
    }
}

