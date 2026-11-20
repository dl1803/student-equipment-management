using Microsoft.AspNetCore.Mvc;
using QuanLyMuonTra.Api.Auth;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.Anh;

public sealed record TaiAnhResponse(Guid MaAnh);

public static class AnhEndpoints
{
    public static IEndpointRouteBuilder MapAnhEndpoints(this IEndpointRouteBuilder duongDan)
    {
        duongDan.MapPost("/api/v1/anh", async (
                HttpRequest yeuCau,
                [FromServices] IAnhService anhService,
                CancellationToken ct) =>
            {
                if (!yeuCau.HasFormContentType)
                {
                    throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Yêu cầu phải là multipart/form-data.");
                }

                IFormCollection form;
                try
                {
                    form = await yeuCau.ReadFormAsync(ct);
                }
                catch (InvalidDataException)
                {
                    throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Nội dung multipart không hợp lệ.");
                }

                var tep = form.Files.GetFile("file")
                    ?? throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Thiếu tệp ảnh (trường file).");
                if (tep.Length > GioiHanAnh.KichThuocToiDaBytes)
                {
                    throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Ảnh vượt quá 2 MB.");
                }
                var giaiDoan = form["giaiDoan"].ToString();

                await using var luong = tep.OpenReadStream();
                var maAnh = await anhService.TaiLenAsync(giaiDoan, luong, ct);

                return Results.Json(new TaiAnhResponse(maAnh), statusCode: StatusCodes.Status201Created);
            })
            .RequireAuthorization(ChinhSach.NhanVienQuanTri);

        return duongDan;
    }
}

