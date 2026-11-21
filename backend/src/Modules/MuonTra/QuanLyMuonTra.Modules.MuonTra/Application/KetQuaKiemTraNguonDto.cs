namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record ViTriLayHangDto(
    Guid MaLoai,
    string TenLoai,
    string TenPhong,
    string? ViTriKe,
    string? ViTriTu,
    int SoThietBiSanSang);

public sealed record KetQuaKiemTraNguonDto(
    bool HopLe,
    IReadOnlyList<LyDoDto> DsLyDo,
    SinhVienTomTatDto SinhVien,
    Guid MaPhong,
    string TenPhong,
    IReadOnlyList<LoaiVaSoLuongDto> DsLoaiVaSoLuong,
    IReadOnlyList<ViTriLayHangDto> DsViTriLayHang);
