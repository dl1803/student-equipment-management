using FluentValidation;

namespace QuanLyMuonTra.Api.Auth;

public sealed record DangNhapSinhVienRequest(string Mssv, string MatKhau);

public sealed class DangNhapSinhVienValidator : AbstractValidator<DangNhapSinhVienRequest>
{
    public DangNhapSinhVienValidator()
    {
        RuleFor(x => x.Mssv).NotEmpty();
        RuleFor(x => x.MatKhau).NotNull().MinimumLength(1);
    }
}

