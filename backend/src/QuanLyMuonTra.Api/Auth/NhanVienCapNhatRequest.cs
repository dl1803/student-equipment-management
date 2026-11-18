using FluentValidation;

namespace QuanLyMuonTra.Api.Auth;

public sealed record NhanVienCapNhatRequest(string HoTen, string? Email);

public sealed class NhanVienCapNhatValidator : AbstractValidator<NhanVienCapNhatRequest>
{
    public NhanVienCapNhatValidator()
    {
        RuleFor(x => x.HoTen).NotEmpty().WithMessage("Họ tên không được để trống.");
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email is not null).WithMessage("Email không đúng định dạng.");
    }
}

