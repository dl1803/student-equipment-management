using FluentValidation;

namespace QuanLyMuonTra.Api.ViPham;

public sealed record KhoanLapBienBanRequest(string MaThietBi, string LoaiViPham, Guid? MaKhungPhat);

public sealed record LapBienBanRequest(string MaPhieuMuon, IReadOnlyList<KhoanLapBienBanRequest> DsKhoan);

public sealed class LapBienBanValidator : AbstractValidator<LapBienBanRequest>
{
    private static readonly string[] LoaiHopLe = ["TRA_TRE", "HU_HONG", "MAT"];

    public LapBienBanValidator()
    {
        RuleFor(x => x.MaPhieuMuon).NotEmpty();
        RuleFor(x => x.DsKhoan).NotEmpty();
        RuleForEach(x => x.DsKhoan).NotNull();
        RuleForEach(x => x.DsKhoan).Where(k => k is not null).ChildRules(khoan =>
        {
            khoan.RuleFor(k => k.MaThietBi).NotEmpty();
            khoan.RuleFor(k => k.LoaiViPham)
                .Must(l => l is not null && LoaiHopLe.Contains(l))
                .WithMessage("Loại vi phạm phải là TRA_TRE, HU_HONG hoặc MAT.");
            khoan.RuleFor(k => k.MaKhungPhat)
                .Null()
                .When(k => k.LoaiViPham != "HU_HONG")
                .WithMessage("Chỉ khoản hư hỏng mới có khung phạt.");
        });
    }
}

