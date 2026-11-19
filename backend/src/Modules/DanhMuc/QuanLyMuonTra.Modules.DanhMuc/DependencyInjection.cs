using FluentValidation;
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
        services.AddScoped<TaoPhongThietBiHandler>();
        services.AddScoped<SuaPhongThietBiHandler>();
        services.AddScoped<IValidator<PhongThietBiRequest>, PhongThietBiRequestValidator>();
        services.AddScoped<TaoLoaiThietBiHandler>();
        services.AddScoped<SuaLoaiThietBiHandler>();
        services.AddScoped<IValidator<LoaiThietBiTaoMoiRequest>, LoaiThietBiTaoMoiRequestValidator>();
        services.AddScoped<IValidator<LoaiThietBiCapNhatRequest>, LoaiThietBiCapNhatRequestValidator>();
        services.AddScoped<CapNhatPhiTreHandler>();
        services.AddScoped<TaoThietBiHandler>();
        services.AddScoped<SuaThietBiHandler>();
        services.AddScoped<CapNhatGiaTriThietBiHandler>();
        services.AddScoped<IValidator<ThietBiTaoMoiRequest>, ThietBiTaoMoiRequestValidator>();
        services.AddScoped<IValidator<ThietBiCapNhatRequest>, ThietBiCapNhatRequestValidator>();
        services.AddScoped<IValidator<GiaTriThietBiRequest>, GiaTriThietBiRequestValidator>();
        return services;
    }
}
