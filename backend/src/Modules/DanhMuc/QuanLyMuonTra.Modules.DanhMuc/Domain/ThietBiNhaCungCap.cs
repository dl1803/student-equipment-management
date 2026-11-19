using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class ThietBiNhaCungCap : IAuditable
{
    private ThietBiNhaCungCap()
    {
    }

    internal ThietBiNhaCungCap(string maThietBi, Guid maNCC)
    {
        MaThietBi = maThietBi;
        MaNCC = maNCC;
    }

    public string MaThietBi { get; private set; } = string.Empty;

    public Guid MaNCC { get; private set; }
}
