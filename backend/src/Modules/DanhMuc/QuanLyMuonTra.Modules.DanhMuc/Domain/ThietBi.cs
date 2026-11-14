namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class ThietBi
{
    private ThietBi()
    {
    }

    public string MaThietBi { get; private set; } = string.Empty;

    public Guid MaLoai { get; private set; }

    public Guid MaPhong { get; private set; }

    public string TenThietBi { get; private set; } = string.Empty;

    public string? MoTa { get; private set; }

    public string? ThongSoKyThuat { get; private set; }

    public DateOnly NgayNhapKho { get; private set; }

    public DateOnly? ThoiHanBaoHanh { get; private set; }

    public TinhTrangThietBi TinhTrang { get; private set; } = TinhTrangThietBi.SanSang;

    public string? ViTriKe { get; private set; }

    public string? ViTriTu { get; private set; }

    public bool NgungSuDung { get; private set; }
}
