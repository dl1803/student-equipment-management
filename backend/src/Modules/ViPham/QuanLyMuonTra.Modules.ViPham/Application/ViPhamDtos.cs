namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed record ChiTietBienBanDto(
    string MaThietBi,
    string LoaiViPham,
    int? SoNgayTre,
    long SoTien,
    Guid? MaKhungPhat);

public sealed record BienBanViPhamDto(
    string MaBienBan,
    string MaPhieuMuon,
    string MaSV,
    DateTimeOffset NgayLap,
    long TongTien,
    string TrangThai,
    DateTimeOffset? ThoiDiemNop,
    IReadOnlyList<ChiTietBienBanDto> DsChiTiet);

public sealed record ViPhamDto(
    Guid MaViPham,
    string LoaiViPham,
    DateTimeOffset ThoiDiemGhiNhan,
    string HocKy,
    string? MaBienBan,
    string? MaYeuCau,
    string? MaDangKy);

