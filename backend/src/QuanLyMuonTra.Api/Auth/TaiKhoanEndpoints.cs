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

        MapNhanVienQuanTri(duongDan);

        return duongDan;
    }

    private static void MapNhanVienQuanTri(IEndpointRouteBuilder duongDan)
    {
        var nhom = duongDan.MapGroup("/api/v1/quan-tri")
            .RequireAuthorization(ChinhSach.QuanTri);

        nhom.MapGet("/nhan-vien", async (
                [FromServices] DsNhanVienHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(trang, kichThuoc, ct)));

        nhom.MapPost("/nhan-vien", async (
                NhanVienTaoMoiRequest yeuCau,
                [FromServices] TaoNhanVienHandler boXuLy,
                CancellationToken ct) =>
                Results.Json(
                    await boXuLy.XuLyAsync(yeuCau.TenDangNhap, yeuCau.MatKhau, yeuCau.HoTen, yeuCau.MaNv, yeuCau.Email, ct),
                    statusCode: StatusCodes.Status201Created))
            .AddEndpointFilter<KiemTraDuLieuFilter<NhanVienTaoMoiRequest>>();

        nhom.MapPut("/nhan-vien/{ma}", async (
                Guid ma,
                NhanVienCapNhatRequest yeuCau,
                [FromServices] SuaNhanVienHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(ma, yeuCau.HoTen, yeuCau.Email, ct)))
            .AddEndpointFilter<KiemTraDuLieuFilter<NhanVienCapNhatRequest>>();

        nhom.MapPost("/nhan-vien/{ma}/khoa", async (
                Guid ma,
                [FromServices] KhoaNhanVienHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(ma, ct)));

        nhom.MapPost("/nhan-vien/{ma}/mo-khoa", async (
                Guid ma,
                [FromServices] MoKhoaNhanVienHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(ma, ct)));

        nhom.MapPost("/nhan-vien/{ma}/dat-lai-mat-khau", async (
                Guid ma,
                DatLaiMatKhauRequest yeuCau,
                [FromServices] DatLaiMatKhauNhanVienHandler boXuLy,
                CancellationToken ct) =>
            {
                await boXuLy.XuLyAsync(ma, yeuCau.MatKhauMoi, ct);
                return Results.NoContent();
            })
            .AddEndpointFilter<KiemTraDuLieuFilter<DatLaiMatKhauRequest>>();

        nhom.MapPut("/nhan-vien/{ma}/quyen-phong", async (
                Guid ma,
                QuyenPhongRequest yeuCau,
                [FromServices] CapNhatQuyenPhongHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(ma, yeuCau.DsMaPhong!, ct)))
            .AddEndpointFilter<KiemTraDuLieuFilter<QuyenPhongRequest>>();
    }
}
