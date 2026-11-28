using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.ViPham.Application;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;

namespace QuanLyMuonTra.Modules.ViPham;

public static class ViPhamModule
{
    public static IServiceCollection AddViPhamModule(this IServiceCollection services)
    {
        services.AddScoped<IBienBanChuaNopReader, BienBanChuaNopReader>();
        services.AddScoped<IThamChieuDanhMucProvider, ThamChieuDanhMucViPhamProvider>();
        services.AddScoped<IThamChieuKhungPhatProvider, ThamChieuKhungPhatViPhamProvider>();
        services.AddScoped<DsBienBanViPhamSvHandler>();
        services.AddScoped<DsBienBanViPhamQuayHandler>();
        services.AddScoped<DsBienBanViPhamQuanTriHandler>();
        services.AddScoped<DsViPhamSvHandler>();
        services.AddScoped<DeXuatBienBanHandler>();
        services.AddScoped<LapBienBanHandler>();
        services.AddScoped<XetMoKhoaQuyenMuon>();
        services.AddScoped<XacNhanNhanDuThietBiHandler>();
        services.AddScoped<GhiNhanDaNopHandler>();
        services.AddScoped<HuyBienBanHandler>();
        services.AddScoped<DieuChinhBienBanHandler>();

        return services;
    }
}

