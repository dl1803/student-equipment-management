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
        RuleFor(x => x.DsLoaiVaSoLuong)
            .Must(ds => ds.Select(d => d.MaLoai).Distinct().Count() == ds.Count)
            .When(x => x.DsLoaiVaSoLuong is { Count: > 0 })
            .WithMessage("Mỗi loại thiết bị chỉ xuất hiện một dòng.");
        RuleForEach(x => x.DsLoaiVaSoLuong).ChildRules(dong =>
        {
            dong.RuleFor(d => d.MaLoai).NotEmpty();
            dong.RuleFor(d => d.SoLuong).GreaterThan(0);
        });
    }
}
