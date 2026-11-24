namespace QuanLyMuonTra.Modules.SinhVien.Application;

public sealed record SinhVienTruongDto(
    string MaSv,
    string HoTen,
    string? Lop,
    string? Khoa,
    string Email,
    string MaNhom,
    DateOnly HanThe,
    string TrangThaiHoc,
    string HocKy,
    string? TenNhom = null,
    DateTimeOffset? CapNhatLuc = null);

public sealed record KetQuaXacThucTruong(bool ThanhCong, SinhVienTruongDto? ThongTin);

public interface ISchoolDirectory
{
    Task<KetQuaXacThucTruong> XacThucAsync(string mssv, string matKhau, CancellationToken ct);

    Task<SinhVienTruongDto?> LayTheoMssvAsync(string mssv, CancellationToken ct);

    Task<IReadOnlyList<SinhVienTruongDto>> LayThayDoiTuAsync(DateTimeOffset? moc, CancellationToken ct);
}
