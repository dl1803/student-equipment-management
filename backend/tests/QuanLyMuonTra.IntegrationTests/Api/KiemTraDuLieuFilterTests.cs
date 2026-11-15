using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Api.Loi;

namespace QuanLyMuonTra.IntegrationTests.Api;

public class KiemTraDuLieuFilterTests
{
    [Fact]
    public async Task KT14_DuLieuSai_NemValidationException()
    {
        var (boLoc, boiCanh) = Tao(new YeuCauThu(0));
        EndpointFilterDelegate tiepTheo = _ => ValueTask.FromResult<object?>("da-qua");

        await Assert.ThrowsAsync<ValidationException>(async () => await boLoc.InvokeAsync(boiCanh, tiepTheo));
    }

    [Fact]
    public async Task KT14_DuLieuDung_ChoQuaBuocTiepTheo()
    {
        var (boLoc, boiCanh) = Tao(new YeuCauThu(2));
        EndpointFilterDelegate tiepTheo = _ => ValueTask.FromResult<object?>("da-qua");

        var ketQua = await boLoc.InvokeAsync(boiCanh, tiepTheo);

        Assert.Equal("da-qua", ketQua);
    }

    private static (KiemTraDuLieuFilter<YeuCauThu> BoLoc, EndpointFilterInvocationContext BoiCanh) Tao(YeuCauThu yeuCau)
    {
        var dichVu = new ServiceCollection()
            .AddSingleton<IValidator<YeuCauThu>, YeuCauThuValidator>()
            .BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = dichVu };

        return (new KiemTraDuLieuFilter<YeuCauThu>(), EndpointFilterInvocationContext.Create(httpContext, yeuCau));
    }

    public record YeuCauThu(int SoLuong);

    public class YeuCauThuValidator : AbstractValidator<YeuCauThu>
    {
        public YeuCauThuValidator()
        {
            RuleFor(y => y.SoLuong).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
        }
    }
}

