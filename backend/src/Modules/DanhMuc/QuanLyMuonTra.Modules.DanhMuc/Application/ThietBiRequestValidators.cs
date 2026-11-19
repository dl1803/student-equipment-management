using FluentValidation;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

internal static class DanhSachMaRule
{
    internal const int SoAnhToiDa = 5;

    internal static bool KhongTrung(IReadOnlyList<Guid>? danhSach) =>
        danhSach is null || danhSach.Distinct().Count() == danhSach.Count;

    internal static bool KhongQuaSoAnh(IReadOnlyList<Guid>? danhSach) =>
        danhSach is null || danhSach.Count <= SoAnhToiDa;
}

internal sealed class ThietBiTaoMoiRequestValidator : AbstractValidator<ThietBiTaoMoiRequest>
{
    public ThietBiTaoMoiRequestValidator()
    {
        RuleFor(x => x.MaThietBi).NotEmpty().WithMessage("Mã thiết bị không được để trống.");
        RuleFor(x => x.TenThietBi).NotEmpty().WithMessage("Tên thiết bị không được để trống.");
        RuleFor(x => x.GiaTri)
            .InclusiveBetween(0L, LichSuGiaTri.GiaTriToiDa)
            .WithMessage("Giá trị thiết bị phải từ 0 đến 99.999.999.999.999.");
        RuleFor(x => x.DsMaNCC).Must(DanhSachMaRule.KhongTrung).WithMessage("Danh sách nhà cung cấp không được trùng.");
        RuleFor(x => x.DsMaAnh).Must(DanhSachMaRule.KhongTrung).WithMessage("Danh sách ảnh không được trùng.");
        RuleFor(x => x.DsMaAnh).Must(DanhSachMaRule.KhongQuaSoAnh).WithMessage("Mỗi thiết bị tối đa 5 ảnh.");
    }
}

internal sealed class ThietBiCapNhatRequestValidator : AbstractValidator<ThietBiCapNhatRequest>
{
    public ThietBiCapNhatRequestValidator()
    {
        RuleFor(x => x.TenThietBi).NotEmpty().WithMessage("Tên thiết bị không được để trống.");
        RuleFor(x => x.DsMaNCC).Must(DanhSachMaRule.KhongTrung).WithMessage("Danh sách nhà cung cấp không được trùng.");
        RuleFor(x => x.DsMaAnh).Must(DanhSachMaRule.KhongTrung).WithMessage("Danh sách ảnh không được trùng.");
        RuleFor(x => x.DsMaAnh).Must(DanhSachMaRule.KhongQuaSoAnh).WithMessage("Mỗi thiết bị tối đa 5 ảnh.");
    }
}

internal sealed class GiaTriThietBiRequestValidator : AbstractValidator<GiaTriThietBiRequest>
{
    public GiaTriThietBiRequestValidator()
    {
        RuleFor(x => x.GiaTri)
            .InclusiveBetween(0L, LichSuGiaTri.GiaTriToiDa)
            .WithMessage("Giá trị thiết bị phải từ 0 đến 99.999.999.999.999.");
    }
}
