using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.SinhVien.Application;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.SinhVien;

public static class SinhVienModule
{
    public static IServiceCollection AddSinhVienModule(this IServiceCollection dichVu)
    {
        dichVu.AddScoped<ISchoolDirectory, MockSchoolDirectory>();
        dichVu.AddScoped<IXacThucSinhVien, XacThucSinhVien>();
        dichVu.AddScoped<IHoSoSinhVienReader, HoSoSinhVienReader>();
        dichVu.AddScoped<ISinhVienEmailReader, SinhVienEmailReader>();
        dichVu.AddScoped<IJob, DongBoSinhVienJob>();
        dichVu.AddScoped<IQuyenMuonService, QuyenMuonService>();

        return dichVu;
    }
}
