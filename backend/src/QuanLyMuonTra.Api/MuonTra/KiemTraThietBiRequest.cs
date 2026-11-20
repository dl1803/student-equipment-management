using FluentValidation;

namespace QuanLyMuonTra.Api.MuonTra;

public sealed record KiemTraThietBiRequest(string MaThietBi, Guid MaPhong, Guid MaLoai);

public sealed class KiemTraThietBiValidator : AbstractValidator<KiemTraThietBiRequest>
{
    public KiemTraThietBiValidator()
    {
        RuleFor(x => x.MaThietBi).NotEmpty();
        RuleFor(x => x.MaPhong).NotEmpty();
        RuleFor(x => x.MaLoai).NotEmpty();
    }
}
