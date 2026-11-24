namespace QuanLyMuonTra.Modules.ThongBao.Application;

public sealed record ThongBaoDto(
    Guid MaThongBao,
    string LoaiThongBao,
    string NoiDung,
    DateTimeOffset ThoiDiemGui,
    string? DoiTuongLoai,
    string? DoiTuongId);

