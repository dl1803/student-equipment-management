namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record LoaiVaSoLuongYeuCau(Guid MaLoai, int SoLuong);

public sealed record LoaiVaSoLuongDto(Guid MaLoai, string TenLoai, int SoLuong);

public sealed record PhieuYeuCauMuonDto(
    string MaYeuCau,
    string MaSV,
    Guid MaPhong,
    string TenPhong,
    DateTimeOffset ThoiGianGui,
    int ThoiGianMuonDuKienGio,
    string TrangThai,
    string? LyDoTuChoi,
    DateTimeOffset? ThoiDiemDuyet,
    DateTimeOffset? HanGiuCho,
    IReadOnlyList<LoaiVaSoLuongDto> DsLoaiVaSoLuong);
