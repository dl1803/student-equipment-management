using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;

namespace QuanLyMuonTra.Modules.ViPham;

public static class ViPhamModule
{
    public static IServiceCollection AddViPhamModule(this IServiceCollection services)
    {
        services.AddScoped<IBienBanChuaNopReader, BienBanChuaNopReader>();
        services.AddScoped<IThamChieuDanhMucProvider, ThamChieuDanhMucViPhamProvider>();
        services.AddScoped<IThamChieuKhungPhatProvider, ThamChieuKhungPhatViPhamProvider>();

        return services;
    }
}

