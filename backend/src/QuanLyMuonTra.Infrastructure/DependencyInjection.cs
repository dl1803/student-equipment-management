using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.Infrastructure.Time;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string chuoiKetNoi,
        IReadOnlyCollection<Assembly>? assemblyCauHinhEf = null)
    {
        if (assemblyCauHinhEf is { Count: > 0 })
        {
            services.AddSingleton(new EfConfigurationAssemblies(assemblyCauHinhEf));
        }

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(chuoiKetNoi));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }

    public static IServiceCollection AddEfConfigurationsFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddSingleton(new EfConfigurationAssemblies([assembly]));
        return services;
    }
}
