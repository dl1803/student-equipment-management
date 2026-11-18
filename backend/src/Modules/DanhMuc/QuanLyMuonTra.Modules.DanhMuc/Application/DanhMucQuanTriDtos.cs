namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed record LoaiThietBiQuanTriDto(
    Guid MaLoai,
    string TenLoai,
    string? MoTa,
    string? HangSanXuat,
    int ChuKyBaoTri,
    long PhiTreMoiNgay,
    bool NgungSuDung);

public sealed record NhaCungCapQuanTriDto(Guid MaNCC, string TenNCC, string? ThongTinLienHe);

public sealed record PhiTreDto(Guid MaLoai, long PhiTreMoiNgay);

public sealed record ThietBiQuanTriDto(
    string MaThietBi,
    Guid MaLoai,
    string TenLoai,
    Guid MaPhong,
    string TenPhong,
    string TenThietBi,
    string? MoTa,
    string? ThongSoKyThuat,
    DateOnly NgayNhapKho,
    DateOnly? ThoiHanBaoHanh,
    string TinhTrang,
    string? ViTriKe,
    string? ViTriTu,
    bool NgungSuDung,
    long GiaTriHienHanh,
    IReadOnlyList<NhaCungCapQuanTriDto> DsNhaCungCap);
