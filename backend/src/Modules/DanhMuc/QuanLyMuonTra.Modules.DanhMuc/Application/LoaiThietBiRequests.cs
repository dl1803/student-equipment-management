namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed record LoaiThietBiTaoMoiRequest(
    string TenLoai,
    string? MoTa,
    string? HangSanXuat,
    int ChuKyBaoTri,
    long PhiTreMoiNgay);

public sealed record LoaiThietBiCapNhatRequest(
    string TenLoai,
    string? MoTa,
    string? HangSanXuat,
    int ChuKyBaoTri);
