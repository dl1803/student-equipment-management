using FluentValidation;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

namespace QuanLyMuonTra.Api.Auth;

public sealed record DatLaiMatKhauRequest(string MatKhauMoi);

public sealed class DatLaiMatKhauValidator : AbstractValidator<DatLaiMatKhauRequest>
{
    public DatLaiMatKhauValidator()
    {
        RuleFor(x => x.MatKhauMoi)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
            .MinimumLength(QuanTriVienSeeder.DoDaiMatKhauToiThieu)
            .WithMessage($"Mật khẩu mới phải có tối thiểu {QuanTriVienSeeder.DoDaiMatKhauToiThieu} ký tự.");
    }
}

