namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed record KhoanDeXuatDto(
    string MaThietBi,
    string TenThietBi,
    string LoaiViPham,
    int? SoNgayTre,
    long SoTien,
    Guid? MaKhungPhat);

public sealed record DeXuatBienBanDto(
    string MaPhieu,
    IReadOnlyList<KhoanDeXuatDto> DsKhoan,
    long TongTien);

