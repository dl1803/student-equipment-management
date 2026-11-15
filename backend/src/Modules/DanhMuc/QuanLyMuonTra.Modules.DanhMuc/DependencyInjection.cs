using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

namespace QuanLyMuonTra.Modules.DanhMuc;

public static class DependencyInjection
{
    public static IServiceCollection AddDanhMucModule(this IServiceCollection services)
    {
        services.AddScoped<IDanhMucReader, DanhMucReader>();
        return services;
    }
}
