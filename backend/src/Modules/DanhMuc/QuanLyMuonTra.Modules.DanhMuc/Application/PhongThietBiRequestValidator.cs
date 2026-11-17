using FluentValidation;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

internal sealed class PhongThietBiRequestValidator : AbstractValidator<PhongThietBiRequest>
{
    public PhongThietBiRequestValidator()
    {
        RuleFor(x => x.TenPhong).NotEmpty().WithMessage("Tên phòng không được để trống.");
        RuleFor(x => x.CoSo).NotEmpty().WithMessage("Cơ sở không được để trống.");
    }
}
