using FluentValidation;

namespace QuanLyMuonTra.Api.MuonTra;

public sealed record DangKyChoRequest(Guid MaPhong, Guid MaLoai);

public sealed class DangKyChoValidator : AbstractValidator<DangKyChoRequest>
{
    public DangKyChoValidator()
    {
        RuleFor(x => x.MaPhong).NotEmpty();
        RuleFor(x => x.MaLoai).NotEmpty();
    }
}
