using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.SharedKernel.Contracts;

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
        services.AddScoped<IAdvisoryLockService, AdvisoryLockService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IBoTinhHanTra, BoTinhHanTra>();
        services.AddScoped<TraCuuThietBiHandler>();
        services.AddScoped<TraCuuThietBiChiTietHandler>();
        services.AddScoped<DsPhongThietBiSvHandler>();
        services.AddScoped<DsLoaiThietBiSvHandler>();
        services.AddScoped<QuayDsLoaiThietBiHandler>();
        services.AddScoped<GuiYeuCauMuonHandler>();
        services.AddScoped<DsPhieuYeuCauMuonSvHandler>();
        services.AddScoped<ChiTietPhieuYeuCauMuonSvHandler>();
        services.AddScoped<DsPhieuYeuCauMuonQuayHandler>();
        services.AddScoped<KiemTraPhieuYeuCauMuonHandler>();
        services.AddScoped<IThongBaoDuyetOutbox, ThongBaoDuyetOutboxChuaNoi>();
        services.AddScoped<TuChoiPhieuYeuCauMuonHandler>();
        services.AddScoped<DuyetPhieuYeuCauMuonHandler>();
        services.AddScoped<KiemTraThietBiGiaoHandler>();
        return services;
    }
}
