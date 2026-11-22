using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.ThongBao.Application;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ThongBao;

public static class ThongBaoModule
{
    public static IServiceCollection AddThongBaoModule(this IServiceCollection dichVu)
    {
        dichVu.AddScoped<IThongBaoOutbox, ThongBaoOutbox>();
        dichVu.AddScoped<DsThongBaoSvHandler>();
        dichVu.AddScoped<IJob, GuiEmailJob>();

        return dichVu;
    }
}

