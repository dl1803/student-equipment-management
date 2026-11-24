using System.Text.Json.Serialization;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public sealed record NhanVienDto(
    Guid MaNguoiDung,
    [property: JsonPropertyName("maNV")] string MaNv,
    string TenDangNhap,
    string HoTen,
    string? Email,
    string TrangThaiKhoa,
    IReadOnlyList<PhongThietBiDto> DsPhong);

public sealed record NhanVienTrangDto(
    int TongSo,
    int Trang,
    int KichThuoc,
    IReadOnlyList<NhanVienDto> DanhSach);

