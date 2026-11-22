using FluentValidation;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.MuonTra;

public sealed record ThietBiGiaoRequest(string? MaThietBi, string? MoTa, IReadOnlyList<Guid> DsMaAnh);

public sealed record LapPhieuMuonRequest(
    string? Nguon,
    string? MaYeuCau,
    string? MaDangKy,
    string? MaSV,
    Guid? MaPhong,
    IReadOnlyList<LoaiVaSoLuongRequest> DsLoaiVaSoLuong,
    IReadOnlyList<ThietBiGiaoRequest> DsThietBi)
{
    public const string PhieuYeuCau = "PHIEU_YEU_CAU";
    public const string DangKyCho = "DANG_KY_CHO";
    public const string TrucTiep = "TRUC_TIEP";
}

public sealed class LapPhieuMuonValidator : AbstractValidator<LapPhieuMuonRequest>
{
    public LapPhieuMuonValidator()
    {
        RuleFor(x => x.Nguon)
            .NotEmpty()
            .Must(n => n is LapPhieuMuonRequest.PhieuYeuCau or LapPhieuMuonRequest.DangKyCho or LapPhieuMuonRequest.TrucTiep);

        RuleFor(x => x.DsThietBi).NotEmpty();
        RuleForEach(x => x.DsThietBi).NotNull();
        RuleForEach(x => x.DsThietBi).Where(t => t is not null).ChildRules(dong =>
        {
            dong.RuleFor(t => t.MaThietBi).NotEmpty();
            dong.RuleFor(t => t.MoTa).NotEmpty();
            dong.RuleFor(t => t.DsMaAnh)
                .Must(ds => ds is null || ds.Count <= GioiHanAnh.SoAnhToiDa)
                .WithMessage("Mỗi thiết bị tối đa 5 ảnh.");
            dong.RuleFor(t => t.DsMaAnh)
                .Must(ds => ds is null || ds.Distinct().Count() == ds.Count)
                .WithMessage("Mã ảnh không được trùng nhau.");
            dong.RuleFor(t => t.DsMaAnh)
                .Must(ds => ds is null || ds.All(maAnh => maAnh != Guid.Empty))
                .WithMessage("Mã ảnh không hợp lệ.");
        });

        When(x => x.Nguon == LapPhieuMuonRequest.PhieuYeuCau, () =>
        {
            RuleFor(x => x.MaYeuCau).NotEmpty().Matches(@"\AYC-[0-9]{6}-[0-9]{6,}\z");
        });

        When(x => x.Nguon == LapPhieuMuonRequest.DangKyCho, () =>
        {
            RuleFor(x => x.MaDangKy).NotEmpty().Matches(@"\ADK-[0-9]{6}-[0-9]{6,}\z");
        });

        When(x => x.Nguon == LapPhieuMuonRequest.TrucTiep, () =>
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
