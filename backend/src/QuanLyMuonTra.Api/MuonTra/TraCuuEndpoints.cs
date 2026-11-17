using Microsoft.AspNetCore.Mvc;
using QuanLyMuonTra.Api.Auth;
using QuanLyMuonTra.Modules.MuonTra.Application;

namespace QuanLyMuonTra.Api.MuonTra;

public static class TraCuuEndpoints
{
    public static IEndpointRouteBuilder MapMuonTraEndpoints(this IEndpointRouteBuilder duongDan)
    {
        var nhomSv = duongDan.MapGroup("/api/v1/sv");

        nhomSv.MapGet("/tra-cuu/thiet-bi", async (
                [FromQuery] string? ten,
                [FromQuery] Guid? maLoai,
                [FromQuery] Guid? maPhong,
                [FromQuery] bool? conHang,
                [FromServices] TraCuuThietBiHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(ten, maLoai, maPhong, conHang, trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.SinhVien);

        nhomSv.MapGet("/tra-cuu/thiet-bi/{maLoai}", async (
                Guid maLoai,
                [FromQuery] Guid maPhong,
                [FromServices] TraCuuThietBiChiTietHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(maLoai, maPhong, ct)))
            .RequireAuthorization(ChinhSach.SinhVien);

        nhomSv.MapGet("/phong-thiet-bi", async (
                [FromServices] DsPhongThietBiSvHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.SinhVien);

        nhomSv.MapGet("/loai-thiet-bi", async (
                [FromServices] DsLoaiThietBiSvHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.SinhVien);

        var nhomQuay = duongDan.MapGroup("/api/v1/quay");

        nhomQuay.MapGet("/loai-thiet-bi", async (
                [FromQuery] string? ten,
                [FromQuery] Guid? maLoai,
                [FromQuery] Guid? maPhong,
                [FromQuery] bool? conHang,
                [FromServices] QuayDsLoaiThietBiHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(ten, maLoai, maPhong, conHang, trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.NhanVien);

        return duongDan;
    }
}
