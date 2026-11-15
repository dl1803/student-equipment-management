using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.BaoTri.Infrastructure;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.BaoTri;

public static class DependencyInjection
{
    public static IServiceCollection AddBaoTriModule(this IServiceCollection services)
    {
        services.AddScoped<IThamChieuDanhMucProvider, ThamChieuBaoTriProvider>();
        return services;
    }
}
