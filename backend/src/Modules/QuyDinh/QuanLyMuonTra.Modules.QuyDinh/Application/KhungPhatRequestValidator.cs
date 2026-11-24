using FluentValidation;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

internal sealed class KhungPhatRequestValidator : AbstractValidator<KhungPhatRequest>
{
    public KhungPhatRequestValidator()
    {
        RuleFor(x => x.TenKhung).NotNull().MinimumLength(1).WithMessage("Tên khung phạt không được để trống.");
    }
}
