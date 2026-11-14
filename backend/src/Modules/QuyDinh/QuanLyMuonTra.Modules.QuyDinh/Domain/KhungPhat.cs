namespace QuanLyMuonTra.Modules.QuyDinh.Domain;

public sealed class KhungPhat
{
    private KhungPhat()
    {
    }

    public Guid MaKhung { get; private set; }

    public string TenKhung { get; private set; } = string.Empty;

    public decimal TyLePhanTram { get; private set; }

    public bool DangSuDung { get; private set; }
}
