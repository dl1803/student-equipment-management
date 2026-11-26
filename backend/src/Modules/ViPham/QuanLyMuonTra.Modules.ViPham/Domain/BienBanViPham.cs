namespace QuanLyMuonTra.Modules.ViPham.Domain;

public sealed class BienBanViPham
{
    private BienBanViPham()
    {
    }

    public string MaBienBan { get; private set; } = string.Empty;

    public string MaPhieuMuon { get; private set; } = string.Empty;

    public Guid MaNvLap { get; private set; }

    public DateTimeOffset NgayLap { get; private set; }

    public decimal TongTien { get; private set; }

    public string TrangThai { get; private set; } = string.Empty;

    public DateTimeOffset? ThoiDiemNop { get; private set; }
}

