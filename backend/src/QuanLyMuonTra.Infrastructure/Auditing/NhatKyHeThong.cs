namespace QuanLyMuonTra.Infrastructure.Auditing;

public class NhatKyHeThong
{
    public Guid MaNhatKy { get; set; }

    public DateTimeOffset ThoiDiem { get; set; }

    public Guid? MaNguoiDung { get; set; }

    public string? MaSv { get; set; }

    public string ThaoTac { get; set; } = string.Empty;

    public string DoiTuong { get; set; } = string.Empty;

    public string? DoiTuongId { get; set; }

    public string? GiaTriTruoc { get; set; }

    public string? GiaTriSau { get; set; }
}

