namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed record ThietBiChoQuyetDinhThanhLyDto(
    string MaThietBi,
    string TenThietBi,
    Guid MaLoai,
    Guid MaPhong,
    string TenPhong,
    Guid MaBaoTri,
    string? NoiDungThucHien,
    DateTimeOffset NgayHoanThanh,
    long GiaTriHienHanh);

public sealed record ThietBiThanhLyDto(string MaThietBi, string TenThietBi);

public sealed record QuyetDinhThanhLyDto(
    string MaQuyetDinh,
    Guid MaQTLap,
    DateOnly NgayThanhLy,
    IReadOnlyList<ThietBiThanhLyDto> DsThietBi);
