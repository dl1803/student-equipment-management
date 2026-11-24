using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Application;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

namespace QuanLyMuonTra.Modules.QuyDinh;

public static class DependencyInjection
{
    public static IServiceCollection AddQuyDinhModule(this IServiceCollection services)
    {
        services.AddScoped<IThamChieuDanhMucProvider, ThamChieuQuyDinhProvider>();
        services.AddScoped<IQuyDinhReader, QuyDinhReader>();
        services.AddScoped<LayThamSoHandler>();
        services.AddScoped<CapNhatThamSoHandler>();
        services.AddScoped<DsKhungPhatHandler>();
        services.AddScoped<TaoKhungPhatHandler>();
        services.AddScoped<SuaKhungPhatHandler>();
        services.AddScoped<TatKhungPhatHandler>();
        services.AddScoped<XoaKhungPhatHandler>();
        services.AddScoped<IValidator<KhungPhatRequest>, KhungPhatRequestValidator>();
        return services;
    }
}
