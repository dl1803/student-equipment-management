using FluentValidation;

namespace QuanLyMuonTra.Api.ViPham;

public sealed record KhoanDieuChinhRequest(string MaThietBi, string LoaiViPham, long? SoTien);

public sealed record DieuChinhBienBanRequest(IReadOnlyList<KhoanDieuChinhRequest> DsKhoan);

public sealed class DieuChinhBienBanValidator : AbstractValidator<DieuChinhBienBanRequest>
{
    private static readonly string[] LoaiHopLe = ["TRA_TRE", "HU_HONG", "MAT"];

    public DieuChinhBienBanValidator()
    {
        RuleFor(x => x.DsKhoan).NotEmpty();
        RuleForEach(x => x.DsKhoan).NotNull();
        RuleForEach(x => x.DsKhoan).Where(k => k is not null).ChildRules(khoan =>
        {
            khoan.RuleFor(k => k.MaThietBi).NotEmpty();
            khoan.RuleFor(k => k.LoaiViPham)
                .Must(l => l is not null && LoaiHopLe.Contains(l))
                .WithMessage("Loại vi phạm phải là TRA_TRE, HU_HONG hoặc MAT.");
            khoan.RuleFor(k => k.SoTien).NotNull().GreaterThanOrEqualTo(0L);
        });
    }
}

