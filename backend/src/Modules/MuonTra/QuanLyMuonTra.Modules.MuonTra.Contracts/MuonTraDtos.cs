namespace QuanLyMuonTra.Modules.MuonTra.Contracts;

public record PhieuMuonDto(
    Guid MaPhieu,
    string MaSv,
    Guid MaPhong,
    Guid MaNvGiao,
    DateTimeOffset NgayGioMuon,
    DateTimeOffset NgayGioHenTra,
    string TrangThai
);

public record ChiTietPhieuMuonDto(
    Guid MaChiTiet,
    Guid MaPhieu,
    string MaThietBi,
    decimal PhiTreMoiNgay,
    string TinhTrangBanDau,
    string? KetQuaKiemTra,
    string? TinhTrangKhiTra,
    DateTimeOffset? NgayGioTraThucTe,
    Guid? MaKhung
);

public record DangKyChoDto(
    Guid MaDangKy,
    string MaSv,
    Guid MaPhong,
    Guid MaLoai,
    int? SoThuTu,
    DateTimeOffset? HanGiuCho,
    string TrangThai
);
