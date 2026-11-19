namespace QuanLyMuonTra.Modules.SinhVien.Contracts;

public sealed record HoSoSinhVienDto(
    string MaSv,
    string HoTen,
    string? Lop,
    string? Khoa,
    string MaNhom,
    string TenNhom,
    DateOnly HanThe,
    string TrangThaiHoc,
    string TrangThaiQuyenMuon);

public sealed record LyDoKhongHopLeDto(string MaLoi, string ThongDiep);

public interface IHoSoSinhVienReader
{
    Task<HoSoSinhVienDto?> LayTheoMaSvAsync(string maSv, CancellationToken ct);
}

