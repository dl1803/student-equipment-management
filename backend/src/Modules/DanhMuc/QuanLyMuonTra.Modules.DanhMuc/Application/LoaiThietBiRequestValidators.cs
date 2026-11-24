using FluentValidation;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

internal sealed class LoaiThietBiTaoMoiRequestValidator : AbstractValidator<LoaiThietBiTaoMoiRequest>
{
    public LoaiThietBiTaoMoiRequestValidator()
    {
        RuleFor(x => x.TenLoai).NotEmpty().WithMessage("Tên loại thiết bị không được để trống.");
        RuleFor(x => x.ChuKyBaoTri).GreaterThan(0).WithMessage("Chu kỳ bảo trì phải lớn hơn 0.");
        RuleFor(x => x.PhiTreMoiNgay).GreaterThanOrEqualTo(0).WithMessage("Phí trả trễ mỗi ngày không được âm.");
    }
}

internal sealed class LoaiThietBiCapNhatRequestValidator : AbstractValidator<LoaiThietBiCapNhatRequest>
{
    public LoaiThietBiCapNhatRequestValidator()
    {
        RuleFor(x => x.TenLoai).NotEmpty().WithMessage("Tên loại thiết bị không được để trống.");
        RuleFor(x => x.ChuKyBaoTri).GreaterThan(0).WithMessage("Chu kỳ bảo trì phải lớn hơn 0.");
    }
}
