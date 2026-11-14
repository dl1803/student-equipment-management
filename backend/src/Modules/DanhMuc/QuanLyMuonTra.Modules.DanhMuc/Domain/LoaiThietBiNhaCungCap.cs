namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class LoaiThietBiNhaCungCap
{
    private LoaiThietBiNhaCungCap()
    {
    }

    public Guid MaLoai { get; private set; }

    public Guid MaNCC { get; private set; }
}
