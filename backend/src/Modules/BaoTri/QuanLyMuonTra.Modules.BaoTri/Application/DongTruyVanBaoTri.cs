namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class DongDenHanBaoTri
{
    public string MaThietBi { get; set; } = string.Empty;

    public string TenThietBi { get; set; } = string.Empty;

    public Guid MaLoai { get; set; }

    public string TenLoai { get; set; } = string.Empty;

    public Guid MaPhong { get; set; }

    public string TenPhong { get; set; } = string.Empty;

    public string TinhTrang { get; set; } = string.Empty;

    public DateOnly NgayDenHan { get; set; }
}

public sealed class DongChoTiepNhanSuaChua
{
    public string MaThietBi { get; set; } = string.Empty;

    public string TenThietBi { get; set; } = string.Empty;

    public Guid MaPhong { get; set; }

    public string TenPhong { get; set; } = string.Empty;

    public string? MaPhieuMuon { get; set; }
}
