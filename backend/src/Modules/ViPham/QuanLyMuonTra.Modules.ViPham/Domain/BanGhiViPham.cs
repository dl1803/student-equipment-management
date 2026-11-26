namespace QuanLyMuonTra.Modules.ViPham.Domain;

public sealed class BanGhiViPham
{
    private BanGhiViPham()
    {
    }

    public Guid MaViPham { get; private set; }

    public string MaSv { get; private set; } = string.Empty;

    public string LoaiViPham { get; private set; } = string.Empty;

    public DateTimeOffset ThoiDiemGhiNhan { get; private set; }

    public string HocKy { get; private set; } = string.Empty;

    public string? MaBienBan { get; private set; }

    public string? MaYeuCau { get; private set; }

    public string? MaDangKy { get; private set; }
}

