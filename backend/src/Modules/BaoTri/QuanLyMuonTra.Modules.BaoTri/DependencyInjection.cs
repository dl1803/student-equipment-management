using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.Modules.BaoTri.Infrastructure;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.BaoTri;

public static class DependencyInjection
{
    public static IServiceCollection AddBaoTriModule(this IServiceCollection services)
    {
        services.AddScoped<IThamChieuDanhMucProvider, ThamChieuBaoTriProvider>();
        services.AddScoped<DsThietBiDenHanBaoTriHandler>();
        services.AddScoped<DsChoTiepNhanSuaChuaHandler>();
        services.AddScoped<LapBaoTriHandler>();
        services.AddScoped<HoanThanhBaoTriHandler>();
        return services;
    }
}
