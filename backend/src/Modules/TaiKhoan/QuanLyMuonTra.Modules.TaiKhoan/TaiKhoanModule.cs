using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.TaiKhoan.Application;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

namespace QuanLyMuonTra.Modules.TaiKhoan;

public static class TaiKhoanModule
{
    public static IServiceCollection AddTaiKhoanModule(this IServiceCollection dichVu, JwtOptions jwt)
    {
        dichVu.AddSingleton(jwt);
        dichVu.AddSingleton<IPasswordHasher<NguoiDungHeThong>, PasswordHasher<NguoiDungHeThong>>();
        dichVu.AddSingleton<IPhatHanhAccessToken, PhatHanhAccessToken>();
        dichVu.AddScoped<QuanTriVienSeeder>();
        dichVu.AddMemoryCache();
        dichVu.AddScoped<QuyenPhongService>();
        dichVu.AddScoped<IQuyenPhongService>(sp => sp.GetRequiredService<QuyenPhongService>());
        dichVu.AddScoped<IBoNhoDemQuyenPhong>(sp => sp.GetRequiredService<QuyenPhongService>());
        dichVu.AddScoped<ITrangThaiTaiKhoanReader, TrangThaiTaiKhoanReader>();
        dichVu.AddScoped<DangNhapNhanVienHandler>();
        dichVu.AddScoped<LayThongTinToiHandler>();
        dichVu.AddScoped<DangNhapSinhVienHandler>();
        dichVu.AddScoped<LamMoiTokenHandler>();
        dichVu.AddScoped<DangXuatHandler>();
        dichVu.AddScoped<NhanVienDtoBuilder>();
        dichVu.AddScoped<DsNhanVienHandler>();
        dichVu.AddScoped<TaoNhanVienHandler>();
        dichVu.AddScoped<SuaNhanVienHandler>();
        dichVu.AddScoped<KhoaNhanVienHandler>();
        dichVu.AddScoped<MoKhoaNhanVienHandler>();
        dichVu.AddScoped<DatLaiMatKhauNhanVienHandler>();
        dichVu.AddScoped<CapNhatQuyenPhongHandler>();

        return dichVu;
    }
}
