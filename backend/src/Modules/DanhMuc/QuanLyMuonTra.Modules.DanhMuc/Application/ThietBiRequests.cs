using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed record ThietBiTaoMoiRequest(
    [property: JsonRequired] string MaThietBi,
    [property: JsonRequired] Guid MaLoai,
    [property: JsonRequired] Guid MaPhong,
    [property: JsonRequired] string TenThietBi,
    string? MoTa,
    string? ThongSoKyThuat,
    [property: JsonRequired] DateOnly NgayNhapKho,
    DateOnly? ThoiHanBaoHanh,
    string? ViTriKe,
    string? ViTriTu,
    IReadOnlyList<Guid>? DsMaNCC,
    IReadOnlyList<Guid>? DsMaAnh,
    [property: JsonRequired] long GiaTri);

public sealed record ThietBiCapNhatRequest(
    [property: JsonRequired] Guid MaLoai,
    [property: JsonRequired] Guid MaPhong,
    [property: JsonRequired] string TenThietBi,
    string? MoTa,
    string? ThongSoKyThuat,
    [property: JsonRequired] DateOnly NgayNhapKho,
    DateOnly? ThoiHanBaoHanh,
    string? ViTriKe,
    string? ViTriTu,
    IReadOnlyList<Guid>? DsMaNCC,
    IReadOnlyList<Guid>? DsMaAnh);

public sealed record GiaTriThietBiRequest([property: JsonRequired] long GiaTri);
