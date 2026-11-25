namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DongDangKyCho
{
    public string MaDangKy { get; set; } = string.Empty;

    public string MaSv { get; set; } = string.Empty;

    public Guid MaPhong { get; set; }

    public Guid MaLoai { get; set; }

    public DateTimeOffset ThoiDiemDangKy { get; set; }

    public int? SoThuTuCho { get; set; }

    public string TrangThai { get; set; } = string.Empty;

    public DateTimeOffset? HanGiuCho { get; set; }
}
