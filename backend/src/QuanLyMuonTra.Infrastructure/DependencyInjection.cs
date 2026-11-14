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
        IReadOnlyCollection<Assembly> assemblyCauHinhEf)
    {
        services.AddSingleton(new EfConfigurationAssemblies(assemblyCauHinhEf));
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(chuoiKetNoi));
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}

