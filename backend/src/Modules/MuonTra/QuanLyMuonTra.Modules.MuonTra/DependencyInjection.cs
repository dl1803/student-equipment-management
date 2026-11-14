using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra;

public static class DependencyInjection
{
    public static IServiceCollection AddMuonTraModule(this IServiceCollection services)
    {
        services.AddScoped<IMuonTraReader, MuonTraReader>();
        services.AddScoped<ThamChieuMuonTraProvider>();
        services.AddScoped<IThamChieuMuonTraProvider>(sp => sp.GetRequiredService<ThamChieuMuonTraProvider>());
        services.AddScoped<IThamChieuDanhMucProvider>(sp => sp.GetRequiredService<ThamChieuMuonTraProvider>());
        services.AddScoped<IThamChieuKhungPhatProvider>(sp => sp.GetRequiredService<ThamChieuMuonTraProvider>());
        return services;
    }
}
