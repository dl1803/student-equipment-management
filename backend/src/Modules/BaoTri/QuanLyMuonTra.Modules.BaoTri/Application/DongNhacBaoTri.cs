namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class DongNhacBaoTri
{
    public string MaThietBi { get; set; } = string.Empty;

    public Guid MaPhong { get; set; }

    public DateOnly NgayDenHan { get; set; }
}
