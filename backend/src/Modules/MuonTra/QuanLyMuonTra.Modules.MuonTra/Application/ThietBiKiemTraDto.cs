namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record ThietBiKiemTraDto(
    string MaThietBi,
    string TenThietBi,
    Guid MaLoai,
    string TenLoai,
    Guid MaPhong,
    string TenPhong,
    string? ViTriKe,
    string? ViTriTu);
