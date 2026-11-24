using FluentValidation;

namespace QuanLyMuonTra.Api.Auth;

public sealed record QuyenPhongRequest(List<Guid>? DsMaPhong);

public sealed class QuyenPhongValidator : AbstractValidator<QuyenPhongRequest>
{
    public QuyenPhongValidator()
    {
        RuleFor(x => x.DsMaPhong).NotNull().WithMessage("Danh sách mã phòng là bắt buộc.");
        RuleFor(x => x.DsMaPhong)
            .Must(ds => ds!.Distinct().Count() == ds!.Count)
            .When(x => x.DsMaPhong is not null)
            .WithMessage("Danh sách mã phòng không được trùng lặp.");
    }
}

