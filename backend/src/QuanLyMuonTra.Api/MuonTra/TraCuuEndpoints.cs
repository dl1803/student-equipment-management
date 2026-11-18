using Microsoft.AspNetCore.Mvc;
using QuanLyMuonTra.Api.Auth;
using QuanLyMuonTra.Api.Loi;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

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

        nhomSv.MapPost("/phieu-yeu-cau-muon", async (
                GuiYeuCauMuonRequest yeuCau,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] GuiYeuCauMuonHandler boXuLy,
                CancellationToken ct) =>
            {
                var dsLoai = yeuCau.DsLoaiVaSoLuong
                    .Where(d => d is not null)
                    .Select(d => new LoaiVaSoLuongYeuCau(d.MaLoai, d.SoLuong))
                    .ToList();
                var phieu = await boXuLy.XuLyAsync(
                    LayMaSv(nguoiDung),
                    yeuCau.MaPhong,
                    yeuCau.ThoiGianMuonDuKienGio,
                    dsLoai,
                    ct);

                return Results.Created($"/api/v1/sv/phieu-yeu-cau-muon/{phieu.MaYeuCau}", phieu);
            })
            .AddEndpointFilter<KiemTraDuLieuFilter<GuiYeuCauMuonRequest>>()
            .RequireAuthorization(ChinhSach.SinhVien);

        nhomSv.MapGet("/phieu-yeu-cau-muon", async (
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] DsPhieuYeuCauMuonSvHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaSv(nguoiDung), trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.SinhVien);

        nhomSv.MapGet("/phieu-yeu-cau-muon/{ma}", async (
                string ma,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] ChiTietPhieuYeuCauMuonSvHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaSv(nguoiDung), ma, ct)))
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

    private static string LayMaSv(ICurrentUser nguoiDung)
    {
        return nguoiDung.MaSv
            ?? throw new LoiNghiepVuException(MaLoi.ChuaXacThuc, "Chưa xác thực.");
    }
}
