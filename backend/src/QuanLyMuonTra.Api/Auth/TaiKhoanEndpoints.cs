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

        nhom.MapPost("/sinh-vien/dang-nhap", async (
                DangNhapSinhVienRequest yeuCau,
                [FromServices] DangNhapSinhVienHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(yeuCau.Mssv, yeuCau.MatKhau, ct)))
            .AddEndpointFilter<KiemTraDuLieuFilter<DangNhapSinhVienRequest>>();

        nhom.MapPost("/lam-moi", async (
                LamMoiRequest yeuCau,
                [FromServices] LamMoiTokenHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(yeuCau.RefreshToken, ct)))
            .AddEndpointFilter<KiemTraDuLieuFilter<LamMoiRequest>>();

        nhom.MapPost("/dang-xuat", async (
                DangXuatRequest yeuCau,
                [FromServices] DangXuatHandler boXuLy,
                CancellationToken ct) =>
            {
                await boXuLy.XuLyAsync(yeuCau.RefreshToken, ct);
                return Results.NoContent();
            })
            .AddEndpointFilter<KiemTraDuLieuFilter<DangXuatRequest>>();

        nhom.MapGet("/toi", async ([FromServices] LayThongTinToiHandler boXuLy, CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(ct)))
            .RequireAuthorization(ChinhSach.NhanVienQuanTri);

        return duongDan;
    }
}
