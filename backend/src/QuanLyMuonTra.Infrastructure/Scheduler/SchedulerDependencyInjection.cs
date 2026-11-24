using Microsoft.Extensions.DependencyInjection;

namespace QuanLyMuonTra.Infrastructure.Scheduler;

public static class SchedulerDependencyInjection
{
    public static IServiceCollection AddScheduler(this IServiceCollection services)
    {
        services.AddSingleton<BoChayLich>();
        services.AddHostedService<SchedulerHostedService>();

        return services;
    }
}

