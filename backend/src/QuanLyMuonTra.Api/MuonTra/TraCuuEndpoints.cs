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

        nhomSv.MapGet("/phieu-muon", async (
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] DsPhieuMuonSvHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaSv(nguoiDung), trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.SinhVien);

        nhomSv.MapGet("/phieu-muon/{ma}", async (
                string ma,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] ChiTietPhieuMuonSvHandler boXuLy,
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

        nhomQuay.MapGet("/phieu-yeu-cau-muon", async (
                [FromQuery] string? trangThai,
                [FromQuery] Guid? maPhong,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] DsPhieuYeuCauMuonQuayHandler boXuLy,
                CancellationToken ct,
                [FromQuery] int trang = 1,
                [FromQuery] int kichThuoc = 20) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaNv(nguoiDung), trangThai, maPhong, trang, kichThuoc, ct)))
            .RequireAuthorization(ChinhSach.NhanVien);

        nhomQuay.MapGet("/phieu-yeu-cau-muon/{ma}/kiem-tra", async (
                string ma,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] KiemTraPhieuYeuCauMuonHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaNv(nguoiDung), ma, ct)))
            .RequireAuthorization(ChinhSach.NhanVien);

        nhomQuay.MapPost("/phieu-yeu-cau-muon/{ma}/duyet", async (
                string ma,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] DuyetPhieuYeuCauMuonHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaNv(nguoiDung), ma, ct)))
            .RequireAuthorization(ChinhSach.NhanVien);

        nhomQuay.MapPost("/phieu-yeu-cau-muon/{ma}/tu-choi", async (
                string ma,
                TuChoiRequest yeuCau,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] TuChoiPhieuYeuCauMuonHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(LayMaNv(nguoiDung), ma, yeuCau.LyDo, ct)))
            .AddEndpointFilter<KiemTraDuLieuFilter<TuChoiRequest>>()
            .RequireAuthorization(ChinhSach.NhanVien);

        nhomQuay.MapPost("/phieu-muon/kiem-tra-thiet-bi", async (
                KiemTraThietBiRequest yeuCau,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] KiemTraThietBiGiaoHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await boXuLy.XuLyAsync(
                    LayMaNv(nguoiDung),
                    yeuCau.MaThietBi,
                    yeuCau.MaPhong,
                    yeuCau.MaLoai,
                    ct)))
            .AddEndpointFilter<KiemTraDuLieuFilter<KiemTraThietBiRequest>>()
            .RequireAuthorization(ChinhSach.NhanVien);

        nhomQuay.MapPost("/phieu-muon/kiem-tra-nguon", async (
                KiemTraNguonRequest yeuCau,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] KiemTraNguonGiaoHandler boXuLy,
                CancellationToken ct) =>
                Results.Ok(await KiemTraNguonAsync(yeuCau, LayMaNv(nguoiDung), boXuLy, ct)))
            .AddEndpointFilter<KiemTraDuLieuFilter<KiemTraNguonRequest>>()
            .RequireAuthorization(ChinhSach.NhanVien);

        nhomQuay.MapPost("/phieu-muon", async (
                LapPhieuMuonRequest yeuCau,
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] GiaoThietBiHandler boXuLy,
                CancellationToken ct) =>
            {
                var phieu = await LapPhieuAsync(yeuCau, LayMaNv(nguoiDung), boXuLy, ct);

                return Results.Created($"/api/v1/quay/phieu-muon/{phieu.MaPhieu}", phieu);
            })
            .AddEndpointFilter<KiemTraDuLieuFilter<LapPhieuMuonRequest>>()
            .RequireAuthorization(ChinhSach.NhanVien);

        return duongDan;
    }

    private static Task<PhieuMuonDayDuDto> LapPhieuAsync(
        LapPhieuMuonRequest yeuCau,
        Guid maNv,
        GiaoThietBiHandler boXuLy,
        CancellationToken ct)
    {
        var dsThietBi = yeuCau.DsThietBi
            .Where(t => t is not null)
            .Select(t => new ThietBiGiaoYeuCau(t.MaThietBi!, t.MoTa!, t.DsMaAnh))
            .ToList();

        return yeuCau.Nguon switch
        {
            LapPhieuMuonRequest.PhieuYeuCau => boXuLy.GiaoTheoPhieuYeuCauAsync(maNv, yeuCau.MaYeuCau!, dsThietBi, ct),
            LapPhieuMuonRequest.DangKyCho => boXuLy.GiaoTheoDangKyChoAsync(maNv, yeuCau.MaDangKy!, dsThietBi, ct),
            LapPhieuMuonRequest.TrucTiep => boXuLy.GiaoTrucTiepAsync(
                maNv,
                yeuCau.MaSV!,
                yeuCau.MaPhong!.Value,
                yeuCau.DsLoaiVaSoLuong.Select(d => new LoaiVaSoLuongYeuCau(d.MaLoai, d.SoLuong)).ToList(),
                dsThietBi,
                ct),
            _ => throw new InvalidOperationException($"Nguồn giao không hợp lệ: {yeuCau.Nguon}.")
        };
    }

    private static Task<KetQuaKiemTraNguonDto> KiemTraNguonAsync(
        KiemTraNguonRequest yeuCau,
        Guid maNv,
        KiemTraNguonGiaoHandler boXuLy,
        CancellationToken ct)
    {
        return yeuCau.Nguon switch
        {
            KiemTraNguonRequest.PhieuYeuCau => boXuLy.KiemTraPhieuYeuCauAsync(maNv, yeuCau.MaYeuCau!, ct),
            KiemTraNguonRequest.DangKyCho => boXuLy.KiemTraDangKyChoAsync(maNv, yeuCau.MaDangKy!, ct),
            KiemTraNguonRequest.TrucTiep => boXuLy.KiemTraTrucTiepAsync(
                maNv,
                yeuCau.MaSV!,
                yeuCau.MaPhong!.Value,
                yeuCau.DsLoaiVaSoLuong.Select(d => new LoaiVaSoLuongYeuCau(d.MaLoai, d.SoLuong)).ToList(),
                ct),
            _ => throw new InvalidOperationException($"Nguồn giao không hợp lệ: {yeuCau.Nguon}.")
        };
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
