namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record BanGiaoThietBiDto(
    string MaThietBi,
    string TenThietBi,
    string MaPhieu,
    SinhVienTomTatDto SinhVien,
    DateTimeOffset NgayGioMuon,
    DateTimeOffset NgayGioHenTra,
    string TinhTrangBanDau,
    IReadOnlyList<QuanLyMuonTra.SharedKernel.AnhRefDto> DsAnhBanDau,
    string? PhanHoiSV);
