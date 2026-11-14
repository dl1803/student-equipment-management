namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class LoaiThietBi
{
    private LoaiThietBi()
    {
    }

    public Guid MaLoai { get; private set; }

    public string TenLoai { get; private set; } = string.Empty;

    public string? MoTa { get; private set; }

    public string? HangSanXuat { get; private set; }

    public int ChuKyBaoTri { get; private set; }

    public decimal PhiTreMoiNgay { get; private set; }

    public bool NgungSuDung { get; private set; }
}
