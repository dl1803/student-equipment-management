namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed record ThietBiDenHanBaoTriDto(
    string MaThietBi,
    string TenThietBi,
    Guid MaLoai,
    string TenLoai,
    Guid MaPhong,
    string TenPhong,
    string TinhTrang,
    DateOnly NgayDenHan);

public sealed record ThietBiChoTiepNhanSuaChuaDto(
    string MaThietBi,
    string TenThietBi,
    Guid MaPhong,
    string TenPhong,
    string? MaPhieuMuon);

public sealed record BaoTriSuaChuaDto(
    Guid MaBaoTri,
    string MaThietBi,
    string TenThietBi,
    Guid MaNVThucHien,
    string? MaPhieuMuon,
    string LoaiThucHien,
    DateTimeOffset NgayLap,
    string? NoiDungThucHien,
    string? KetQua,
    DateTimeOffset? NgayHoanThanh);
