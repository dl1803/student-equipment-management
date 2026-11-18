namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DongPhieuYeuCauMuon
{
    public string MaYeuCau { get; set; } = string.Empty;

    public string MaSv { get; set; } = string.Empty;

    public Guid MaPhong { get; set; }

    public DateTimeOffset ThoiGianGui { get; set; }

    public int ThoiGianMuonDuKienGio { get; set; }

    public string TrangThai { get; set; } = string.Empty;

    public string? LyDoTuChoi { get; set; }

    public DateTimeOffset? ThoiDiemDuyet { get; set; }

    public DateTimeOffset? HanGiuCho { get; set; }
}

public sealed class DongChiTietYeuCauMuon
{
    public string MaYeuCau { get; set; } = string.Empty;

    public Guid MaLoai { get; set; }

    public int SoLuong { get; set; }
}
