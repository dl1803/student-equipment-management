namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class NhaCungCap
{
    private NhaCungCap()
    {
    }

    public Guid MaNCC { get; private set; }

    public string TenNCC { get; private set; } = string.Empty;

    public string? ThongTinLienHe { get; private set; }
}
