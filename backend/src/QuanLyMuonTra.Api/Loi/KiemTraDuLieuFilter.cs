using FluentValidation;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.Loi;

public sealed class KiemTraDuLieuFilter<TRequest> : IEndpointFilter
    where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var yeuCau = context.Arguments.OfType<TRequest>().FirstOrDefault()
            ?? throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Thiếu dữ liệu gửi lên.");

        var kiemTra = context.HttpContext.RequestServices.GetRequiredService<IValidator<TRequest>>();
        await kiemTra.ValidateAndThrowAsync(yeuCau, context.HttpContext.RequestAborted);

        return await next(context);
    }
}

