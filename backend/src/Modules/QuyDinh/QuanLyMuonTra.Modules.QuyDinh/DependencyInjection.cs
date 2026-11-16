using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

namespace QuanLyMuonTra.Modules.QuyDinh;

public static class DependencyInjection
{
    public static IServiceCollection AddQuyDinhModule(this IServiceCollection services)
    {
        services.AddScoped<IThamChieuDanhMucProvider, ThamChieuQuyDinhProvider>();
        services.AddScoped<IQuyDinhReader, QuyDinhReader>();
        return services;
    }
}
