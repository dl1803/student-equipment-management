using FluentValidation;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.MuonTra;

public sealed record KetQuaKiemTraThietBiRequest(
    string? MaThietBi,
    string? KetQua,
    string? MoTa,
    Guid? MaKhungPhat,
    IReadOnlyList<Guid> DsMaAnh)
{
    public const string NguyenVen = "NGUYEN_VEN";
    public const string HuHong = "HU_HONG";
    public const string ThieuPhuKien = "THIEU_PHU_KIEN";
    public const string Mat = "MAT";
}

public sealed record GhiKetQuaKiemTraRequest(IReadOnlyList<KetQuaKiemTraThietBiRequest> DsKetQua);

public sealed class GhiKetQuaKiemTraValidator : AbstractValidator<GhiKetQuaKiemTraRequest>
{
    public GhiKetQuaKiemTraValidator()
    {
        RuleFor(x => x.DsKetQua).NotEmpty();
        RuleForEach(x => x.DsKetQua).NotNull();
        RuleForEach(x => x.DsKetQua).Where(d => d is not null).ChildRules(dong =>
        {
            dong.RuleFor(d => d.MaThietBi).NotEmpty();
            dong.RuleFor(d => d.KetQua)
                .NotEmpty()
                .Must(k => k is KetQuaKiemTraThietBiRequest.NguyenVen
                    or KetQuaKiemTraThietBiRequest.HuHong
                    or KetQuaKiemTraThietBiRequest.ThieuPhuKien
                    or KetQuaKiemTraThietBiRequest.Mat)
                .WithMessage("Kết quả kiểm tra phải là NGUYEN_VEN, HU_HONG, THIEU_PHU_KIEN hoặc MAT.");
            dong.RuleFor(d => d.MaKhungPhat)
                .Null()
                .When(d => d.KetQua is KetQuaKiemTraThietBiRequest.NguyenVen or KetQuaKiemTraThietBiRequest.Mat)
                .WithMessage("Khung phạt phải bỏ trống khi kết quả là nguyên vẹn hoặc mất.");
            dong.RuleFor(d => d.DsMaAnh)
                .Must(ds => ds is null || ds.Count <= GioiHanAnh.SoAnhToiDa)
                .WithMessage("Mỗi thiết bị tối đa 5 ảnh.");
            dong.RuleFor(d => d.DsMaAnh)
                .Must(ds => ds is null || ds.Distinct().Count() == ds.Count)
                .WithMessage("Mã ảnh không được trùng nhau.");
            dong.RuleFor(d => d.DsMaAnh)
                .Must(ds => ds is null || ds.All(maAnh => maAnh != Guid.Empty))
                .WithMessage("Mã ảnh không hợp lệ.");
        });

        RuleFor(x => x.DsKetQua)
            .Must(ds => ds.Where(d => d is not null).Select(d => d.MaThietBi).Distinct().Count() == ds.Count(d => d is not null))
            .When(x => x.DsKetQua is { Count: > 0 })
            .WithMessage("Mỗi thiết bị chỉ xuất hiện một dòng.");
    }
}
