using FluentValidation;

namespace QuanLyMuonTra.Api.Auth;

public sealed record LamMoiRequest(string RefreshToken);

public sealed class LamMoiValidator : AbstractValidator<LamMoiRequest>
{
    public LamMoiValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

