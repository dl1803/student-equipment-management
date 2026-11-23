namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record PhieuMuonTomTatDto(
    string MaPhieu,
    string MaSV,
    string? MaYeuCau,
    string? MaDangKy,
    DateTimeOffset NgayGioMuon,
    DateTimeOffset NgayGioHenTra,
    int SoLanGiaHan,
    string TrangThai,
    int SoThietBi);

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

public sealed record PhieuMuonSvDto(
    string MaPhieu,
    string MaSV,
    string? MaYeuCau,
    string? MaDangKy,
    DateTimeOffset NgayGioMuon,
    DateTimeOffset NgayGioHenTra,
    int SoLanGiaHan,
    string TrangThai,
    int SoThietBi,
    IReadOnlyList<ChiTietPhieuMuonDayDuDto> DsChiTiet,
    IReadOnlyList<BienBanViPhamDto> DsBienBan);
