namespace QuanLyMuonTra.Modules.MuonTra.Contracts;

/// <summary>
/// Thông tin phiếu mượn liên module.
/// </summary>
public record PhieuMuonDto(
    string MaPhieu,
    string MaSv,
    Guid MaNvLap,
    DateTimeOffset NgayGioMuon,
    DateTimeOffset NgayGioHenTra,
    string TrangThai
);

/// <summary>
/// Chi tiết thiết bị trong phiếu mượn liên module.
/// </summary>
public record ChiTietPhieuMuonDto(
    Guid MaChiTiet,
    string MaPhieu,
    string MaThietBi,
    decimal PhiTreMoiNgay,
    string TinhTrangBanDau,
    string? KetQuaKiemTra,
    string? TinhTrangKhiTra,
    DateTimeOffset? NgayGioTraThucTe,
    Guid? MaKhung
);

/// <summary>
/// Thông tin đăng ký chờ nhận thiết bị.
/// </summary>
public record DangKyChoDto(
    string MaDangKy,
    string MaSv,
    Guid MaPhong,
    Guid MaLoai,
    int? SoThuTu,
    DateTimeOffset? HanGiuCho,
    string TrangThai
);
