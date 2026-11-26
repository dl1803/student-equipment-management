namespace QuanLyMuonTra.Modules.ViPham.Domain;

public sealed class ChiTietBienBan
{
    private ChiTietBienBan()
    {
    }

    public string MaBienBan { get; private set; } = string.Empty;

    public string MaPhieuMuon { get; private set; } = string.Empty;

    public string MaThietBi { get; private set; } = string.Empty;

    public string LoaiViPham { get; private set; } = string.Empty;

    public int? SoNgayTre { get; private set; }

    public decimal SoTien { get; private set; }

    public Guid? MaKhung { get; private set; }
}

