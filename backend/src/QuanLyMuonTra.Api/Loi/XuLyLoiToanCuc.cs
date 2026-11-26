using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.Loi;

public sealed class XuLyLoiToanCuc(ILogger<XuLyLoiToanCuc> nhatKy) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            nhatKy.LogWarning(exception, "Phản hồi đã bắt đầu gửi, không thể ghi đè ProblemDetails.");
            return false;
        }

        var problem = TaoProblem(httpContext, exception);
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            problem.GetType(),
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }

    private static string ChuyenCamelCase(string? tenTruong)
    {
        if (string.IsNullOrEmpty(tenTruong))
        {
            return string.Empty;
        }

        return string.Join('.', tenTruong.Split('.').Select(doan =>
            doan.Length == 0 ? doan : char.ToLowerInvariant(doan[0]) + doan[1..]));
    }

    private static string TieuDe(int maHttp) => maHttp switch
    {
        StatusCodes.Status400BadRequest => "Dữ liệu không hợp lệ",
        StatusCodes.Status401Unauthorized => "Chưa xác thực",
        StatusCodes.Status403Forbidden => "Không đủ quyền",
        StatusCodes.Status404NotFound => "Không tìm thấy",
        StatusCodes.Status409Conflict => "Xung đột",
        StatusCodes.Status422UnprocessableEntity => "Vi phạm quy tắc nghiệp vụ",
        StatusCodes.Status429TooManyRequests => "Quá nhiều yêu cầu",
        _ => "Lỗi hệ thống"
    };

    private static ProblemDetails DungProblem(
        HttpContext httpContext,
        int maHttp,
        string? maLoi,
        string? chiTiet,
        IDictionary<string, string[]>? truongLoi = null)
    {
        var problem = truongLoi is null ? new ProblemDetails() : new ValidationProblemDetails(truongLoi);
        problem.Type = "about:blank";
        problem.Title = TieuDe(maHttp);
        problem.Status = maHttp;
        problem.Detail = chiTiet;
        problem.Instance = httpContext.Request.Path;
        if (maLoi is not null)
        {
            problem.Extensions["maLoi"] = maLoi;
        }

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return problem;
    }

    private ProblemDetails TaoProblem(HttpContext httpContext, Exception loi)
    {
        switch (loi)
        {
            case LoiNghiepVuException nghiepVu:
                return DungProblem(httpContext, nghiepVu.MaHttp, nghiepVu.MaLoi, nghiepVu.Message);

            case NghiepVuException quyTac when MaLoiNghiepVu.TatCa.TryGetValue(quyTac.MaLoi, out var maHttpQuyTac):
                return DungProblem(httpContext, maHttpQuyTac, quyTac.MaLoi, quyTac.Message);

            case ValidationException duLieu:
                var truongLoi = duLieu.Errors
                    .GroupBy(l => ChuyenCamelCase(l.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(l => l.ErrorMessage).ToArray());
                return DungProblem(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "DU_LIEU_KHONG_HOP_LE",
                    "Dữ liệu gửi lên không hợp lệ.",
                    truongLoi);

            case BadHttpRequestException:
                return DungProblem(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "DU_LIEU_KHONG_HOP_LE",
                    "Dữ liệu gửi lên không đúng định dạng.");

            case DbUpdateConcurrencyException:
                return DungProblem(
                    httpContext,
                    StatusCodes.Status409Conflict,
                    "XUNG_DOT_CAP_NHAT",
                    "Dữ liệu đã được thay đổi bởi người khác, vui lòng tải lại.");

            default:
                nhatKy.LogError(loi, "Lỗi không xử lý được khi xử lý {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
                return DungProblem(httpContext, StatusCodes.Status500InternalServerError, null, null);
        }
    }
}

