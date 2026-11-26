using Microsoft.AspNetCore.Mvc;
using QuanLyMuonTra.Api.Auth;
using QuanLyMuonTra.Modules.ViPham.Application;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.ViPham;

public static class ViPhamEndpoints
{
    public static IEndpointRouteBuilder MapViPhamEndpoints(this IEndpointRouteBuilder duongDan)
    {
        duongDan.MapGet("/api/v1/sv/bien-ban-vi-pham", async (
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] DsBienBanViPhamSvHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaSv(nguoiDung), trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.SinhVien);

        duongDan.MapGet("/api/v1/sv/vi-pham", async (
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] DsViPhamSvHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaSv(nguoiDung), trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.SinhVien);

        duongDan.MapGet("/api/v1/quay/bien-ban-vi-pham", async (
                [FromQuery(Name = "maSV")] string? maSv,
                [FromQuery] string? trangThai,
                [FromQuery] Guid? maPhong,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] DsBienBanViPhamQuayHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaNv(nguoiDung), maSv, trangThai, maPhong, trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.NhanVien);

        duongDan.MapGet("/api/v1/quay/phieu-muon/{ma}/de-xuat-bien-ban", async (
                string ma,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] DeXuatBienBanHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaNv(nguoiDung), ma, ct)))
            .RequireAuthorization(ChinhSach.NhanVien);

        duongDan.MapGet("/api/v1/quan-tri/bien-ban-vi-pham", async (
                [FromQuery(Name = "maSV")] string? maSv,
                [FromQuery] string? trangThai,
                [FromServices] DsBienBanViPhamQuanTriHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(maSv, trangThai, trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.QuanTri);

        return duongDan;
    }

    private static string LayMaSv(ICurrentUser nguoiDung)
    {
        return nguoiDung.MaSv
            ?? throw new LoiNghiepVuException(MaLoi.ChuaXacThuc, "Chưa xác thực.");
    }

    private static Guid LayMaNv(ICurrentUser nguoiDung)
    {
        return nguoiDung.MaNguoiDung
            ?? throw new LoiNghiepVuException(MaLoi.ChuaXacThuc, "Chưa xác thực.");
    }
}

