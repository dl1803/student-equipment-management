using FluentValidation;

namespace QuanLyMuonTra.Api.MuonTra;

public sealed record KiemTraNguonRequest(
    string? Nguon,
    string? MaYeuCau,
    string? MaDangKy,
    string? MaSV,
    Guid? MaPhong,
    IReadOnlyList<LoaiVaSoLuongRequest> DsLoaiVaSoLuong)
{
    public const string PhieuYeuCau = "PHIEU_YEU_CAU";
    public const string DangKyCho = "DANG_KY_CHO";
    public const string TrucTiep = "TRUC_TIEP";
}

public sealed class KiemTraNguonValidator : AbstractValidator<KiemTraNguonRequest>
{
    public KiemTraNguonValidator()
    {
        RuleFor(x => x.Nguon)
            .NotEmpty()
            .Must(n => n is KiemTraNguonRequest.PhieuYeuCau or KiemTraNguonRequest.DangKyCho or KiemTraNguonRequest.TrucTiep);

        When(x => x.Nguon == KiemTraNguonRequest.PhieuYeuCau, () =>
        {
            RuleFor(x => x.MaYeuCau).NotEmpty().Matches(@"\AYC-[0-9]{6}-[0-9]{6,}\z");
        });

        When(x => x.Nguon == KiemTraNguonRequest.DangKyCho, () =>
        {
            RuleFor(x => x.MaDangKy).NotEmpty().Matches(@"\ADK-[0-9]{6}-[0-9]{6,}\z");
        });

        When(x => x.Nguon == KiemTraNguonRequest.TrucTiep, () =>
        {
            RuleFor(x => x.MaSV).NotEmpty();
            RuleFor(x => x.MaPhong).NotEmpty();
            RuleFor(x => x.DsLoaiVaSoLuong).NotEmpty();
            RuleForEach(x => x.DsLoaiVaSoLuong).NotNull();
            RuleForEach(x => x.DsLoaiVaSoLuong).Where(x => x is not null).ChildRules(dong =>
            {
                dong.RuleFor(d => d.MaLoai).NotEmpty();
                dong.RuleFor(d => d.SoLuong).GreaterThan(0);
            });
            RuleFor(x => x.DsLoaiVaSoLuong)
                .Must(ds => ds.Select(d => d.MaLoai).Distinct().Count() == ds.Count)
                .When(x => x.DsLoaiVaSoLuong is { Count: > 0 } && x.DsLoaiVaSoLuong.All(d => d is not null))
                .WithMessage("Mỗi loại thiết bị chỉ xuất hiện một dòng.");
        });
    }
}
