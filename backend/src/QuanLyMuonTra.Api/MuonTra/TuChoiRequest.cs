using FluentValidation;

namespace QuanLyMuonTra.Api.MuonTra;

public sealed record TuChoiRequest(string LyDo);

public sealed class TuChoiValidator : AbstractValidator<TuChoiRequest>
{
    public TuChoiValidator()
    {
        RuleFor(x => x.LyDo).NotEmpty().WithMessage("Lý do từ chối không được để trống.");
    }
}
