namespace QuanLyMuonTra.Modules.DanhMuc.Contracts;

public sealed record PhongThietBiDto(Guid MaPhong, string TenPhong, string CoSo);

public sealed record LoaiThietBiDto(
    Guid MaLoai,
    string TenLoai,
    int ChuKyBaoTri,
    decimal PhiTreMoiNgay,
    bool NgungSuDung);

public sealed record ThietBiDto(
    string MaThietBi,
    Guid MaLoai,
    Guid MaPhong,
    string TenThietBi,
    DateOnly NgayNhapKho,
    string TinhTrang,
    bool NgungSuDung,
    string? ViTriKe,
    string? ViTriTu,
    decimal GiaTriHienHanh);

