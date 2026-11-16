using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

namespace QuanLyMuonTra.Modules.DanhMuc;

public static class DependencyInjection
{
    public static IServiceCollection AddDanhMucModule(this IServiceCollection services)
    {
        services.AddScoped<IDanhMucReader, DanhMucReader>();
        services.AddScoped<DsPhongThietBiHandler>();
        services.AddScoped<DsLoaiThietBiHandler>();
        services.AddScoped<DsThietBiHandler>();
        services.AddScoped<DsNhaCungCapHandler>();
        return services;
    }
}
