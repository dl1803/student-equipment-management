namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class ThietBiNhaCungCap
{
    private ThietBiNhaCungCap()
    {
    }

    public string MaThietBi { get; private set; } = string.Empty;

    public Guid MaNCC { get; private set; }
}
