using FluentValidation;

namespace QuanLyMuonTra.Api.MuonTra;

public sealed record LoaiVaSoLuongRequest(Guid MaLoai, int SoLuong);

public sealed record GuiYeuCauMuonRequest(
    Guid MaPhong,
    int ThoiGianMuonDuKienGio,
    IReadOnlyList<LoaiVaSoLuongRequest> DsLoaiVaSoLuong);

public sealed class GuiYeuCauMuonValidator : AbstractValidator<GuiYeuCauMuonRequest>
{
    public GuiYeuCauMuonValidator()
    {
        RuleFor(x => x.MaPhong).NotEmpty();
        RuleFor(x => x.ThoiGianMuonDuKienGio).GreaterThan(0);
        RuleFor(x => x.DsLoaiVaSoLuong).NotEmpty();
        RuleForEach(x => x.DsLoaiVaSoLuong).NotNull();
        RuleForEach(x => x.DsLoaiVaSoLuong).Where(x => x is not null).ChildRules(dong =>
        {
            dong.RuleFor(d => d.MaLoai).NotEmpty();
            dong.RuleFor(d => d.SoLuong).GreaterThan(0);
        });
        RuleFor(x => x.DsLoaiVaSoLuong)
            .Must(ds => ds.Where(d => d is not null).Select(d => d.MaLoai).Distinct().Count() == ds.Count(d => d is not null))
            .When(x => x.DsLoaiVaSoLuong is { Count: > 0 } && x.DsLoaiVaSoLuong.All(d => d is not null))
            .WithMessage("Mỗi loại thiết bị chỉ xuất hiện một dòng.");
    }
}
