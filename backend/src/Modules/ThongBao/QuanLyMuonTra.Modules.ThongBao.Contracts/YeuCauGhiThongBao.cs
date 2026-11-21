namespace QuanLyMuonTra.Modules.ThongBao.Contracts;

public sealed record YeuCauGhiThongBao(
    string LoaiThongBao,
    string? MaSvNhan,
    Guid? MaNvNhan,
    IReadOnlyDictionary<string, string> ThamSo,
    string? DoiTuongLoai = null,
    string? DoiTuongId = null,
    string? KhoaTrungLap = null);

