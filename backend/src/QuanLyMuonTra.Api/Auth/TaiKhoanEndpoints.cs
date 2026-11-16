using Microsoft.AspNetCore.Mvc;
using QuanLyMuonTra.Api.Loi;
using QuanLyMuonTra.Modules.TaiKhoan.Application;

namespace QuanLyMuonTra.Api.Auth;

public static class TaiKhoanEndpoints
{
    public static IEndpointRouteBuilder MapTaiKhoanEndpoints(this IEndpointRouteBuilder duongDan)
    {
        var nhom = duongDan.MapGroup("/api/v1/auth");

        nhom.MapPost("/nhan-vien/dang-nhap", async (
                DangNhapNhanVienRequest yeuCau,
                [FromServices] DangNhapNhanVienHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(yeuCau.TenDangNhap, yeuCau.MatKhau, ct)))
            .AddEndpointFilter<KiemTraDuLieuFilter<DangNhapNhanVienRequest>>();

        nhom.MapGet("/toi", async ([FromServices] LayThongTinToiHandler boXuLy, CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(ct)))
            .RequireAuthorization(ChinhSach.NhanVienQuanTri);

        return duongDan;
    }
}
