using FluentValidation;

namespace QuanLyMuonTra.Api.Auth;

public sealed record DangNhapNhanVienRequest(string TenDangNhap, string MatKhau);

public sealed class DangNhapNhanVienValidator : AbstractValidator<DangNhapNhanVienRequest>
{
    public DangNhapNhanVienValidator()
    {
        RuleFor(x => x.TenDangNhap).NotEmpty();
        RuleFor(x => x.MatKhau).NotNull().MinimumLength(1);
    }
}

