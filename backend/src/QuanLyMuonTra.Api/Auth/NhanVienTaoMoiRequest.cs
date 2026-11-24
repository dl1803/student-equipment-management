using FluentValidation;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

namespace QuanLyMuonTra.Api.Auth;

public sealed record NhanVienTaoMoiRequest(string TenDangNhap, string MatKhau, string HoTen, string MaNv, string? Email);

public sealed class NhanVienTaoMoiValidator : AbstractValidator<NhanVienTaoMoiRequest>
{
    public NhanVienTaoMoiValidator()
    {
        RuleFor(x => x.TenDangNhap).NotEmpty().WithMessage("Tên đăng nhập không được để trống.");
        RuleFor(x => x.MatKhau)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(QuanTriVienSeeder.DoDaiMatKhauToiThieu)
            .WithMessage($"Mật khẩu phải có tối thiểu {QuanTriVienSeeder.DoDaiMatKhauToiThieu} ký tự.");
        RuleFor(x => x.HoTen).NotEmpty().WithMessage("Họ tên không được để trống.");
        RuleFor(x => x.MaNv).NotEmpty().OverridePropertyName("maNV").WithMessage("Mã nhân viên không được để trống.");
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email is not null).WithMessage("Email không đúng định dạng.");
    }
}

