namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record TraCuuThietBiItemDto(
    Guid MaLoai,
    string TenLoai,
    string? HangSanXuat,
    Guid MaPhong,
    string TenPhong,
    string CoSo,
    int SoLuongKhaDung);

public sealed record AnhRefDto(Guid MaAnh, string Url);

public sealed record TraCuuThietBiChiTietDto(
    Guid MaLoai,
    string TenLoai,
    string? HangSanXuat,
    Guid MaPhong,
    string TenPhong,
    string CoSo,
    int SoLuongKhaDung,
    string? MoTa,
    IReadOnlyList<string> DsThongSoKyThuat,
    IReadOnlyList<AnhRefDto> DsAnh,
    DateTimeOffset? NgayDuKienCoSan);

public sealed record LoaiThietBiTomTatDto(Guid MaLoai, string TenLoai, string? MoTa, string? HangSanXuat);
