using FluentValidation;

namespace QuanLyMuonTra.Api.Auth;

public sealed record DangXuatRequest(string RefreshToken);

public sealed class DangXuatValidator : AbstractValidator<DangXuatRequest>
{
    public DangXuatValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

