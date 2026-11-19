namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record SinhVienTomTatDto(string MaSV, string HoTen, string? Lop);

public sealed record PhieuYeuCauMuonQuayDto(
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
    IReadOnlyList<LoaiVaSoLuongDto> DsLoaiVaSoLuong,
    SinhVienTomTatDto SinhVien);

public sealed record LyDoDto(string MaLoi, string ThongDiep);

public sealed record KhaDungTheoLoaiDto(Guid MaLoai, string TenLoai, int SoLuongYeuCau, int SoLuongKhaDung);

public sealed record KetQuaKiemTraPhieuDto(
    bool DuocDuyet,
    IReadOnlyList<LyDoDto> DsLyDo,
    IReadOnlyList<KhaDungTheoLoaiDto> DsKhaDung);
