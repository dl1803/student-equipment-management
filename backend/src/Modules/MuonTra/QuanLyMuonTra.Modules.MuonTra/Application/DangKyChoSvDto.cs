namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record DangKyChoSvDto(
    string MaDangKy,
    Guid MaPhong,
    string TenPhong,
    Guid MaLoai,
    string TenLoai,
    DateTimeOffset ThoiDiemDangKy,
    int? SoThuTuCho,
    string TrangThai,
    DateTimeOffset? HanGiuCho);
