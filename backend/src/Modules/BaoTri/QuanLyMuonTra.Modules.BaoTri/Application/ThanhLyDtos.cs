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
