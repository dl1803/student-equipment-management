using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuanLyMuonTra.Infrastructure.Auditing;
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

        services.AddSingleton<IClock, SystemClock>();
        services.TryAddSingleton<ICurrentUser, TacNhanHeThong>();
        services.AddSingleton<NhatKyInterceptor>();
        services.AddDbContext<AppDbContext>((dichVu, options) => options
            .UseNpgsql(chuoiKetNoi)
            .ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, AppDbContextModelCacheKeyFactory>()
            .AddInterceptors(dichVu.GetRequiredService<NhatKyInterceptor>()));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAuditLogger, AuditLogger>();

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
