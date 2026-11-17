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
        dichVu.AddScoped<IQuyenPhongService, QuyenPhongService>();
        dichVu.AddScoped<ITrangThaiTaiKhoanReader, TrangThaiTaiKhoanReader>();
        dichVu.AddScoped<DangNhapNhanVienHandler>();
        dichVu.AddScoped<LayThongTinToiHandler>();
        dichVu.AddScoped<DangNhapSinhVienHandler>();
        dichVu.AddScoped<LamMoiTokenHandler>();
        dichVu.AddScoped<DangXuatHandler>();

        return dichVu;
    }
}
