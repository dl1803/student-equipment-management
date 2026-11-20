using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure.LuuTru;

public static class LuuTruDependencyInjection
{
    public static IServiceCollection AddAnhLuuTru(this IServiceCollection services, SupabaseStorageOptions tuyChon)
    {
        services.AddSingleton<IAnhStorage>(_ => new SupabaseAnhStorage(TaoHttpClient(), tuyChon));
        services.AddScoped<IAnhService, AnhService>();

        return services;
    }

    private static HttpClient TaoHttpClient() =>
        new(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
}

