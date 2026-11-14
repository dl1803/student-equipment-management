namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class LichSuGiaTri
{
    private LichSuGiaTri()
    {
    }

    public Guid MaLichSu { get; private set; }

    public string MaThietBi { get; private set; } = string.Empty;

    public decimal GiaTri { get; private set; }

    public DateTimeOffset NgayCapNhat { get; private set; }
}
