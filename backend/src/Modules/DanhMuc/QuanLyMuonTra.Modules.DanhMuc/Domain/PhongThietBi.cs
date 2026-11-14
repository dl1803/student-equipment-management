namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class PhongThietBi
{
    private PhongThietBi()
    {
    }

    public Guid MaPhong { get; private set; }

    public string TenPhong { get; private set; } = string.Empty;

    public string CoSo { get; private set; } = string.Empty;
}
