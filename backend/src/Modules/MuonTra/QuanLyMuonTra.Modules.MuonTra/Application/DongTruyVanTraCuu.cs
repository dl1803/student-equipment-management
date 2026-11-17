namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DongTraCuuThietBi
{
    public Guid MaLoai { get; set; }

    public string TenLoai { get; set; } = string.Empty;

    public string? HangSanXuat { get; set; }

    public string? MoTa { get; set; }

    public Guid MaPhong { get; set; }

    public string TenPhong { get; set; } = string.Empty;

    public string CoSo { get; set; } = string.Empty;

    public int SoLuongKhaDung { get; set; }
}

public sealed class DongNgayDuKien
{
    public DateTimeOffset? NgayDuKien { get; set; }
}

public sealed class DongPhongThietBi
{
    public Guid MaPhong { get; set; }

    public string TenPhong { get; set; } = string.Empty;

    public string CoSo { get; set; } = string.Empty;
}

public sealed class DongLoaiThietBi
{
    public Guid MaLoai { get; set; }

    public string TenLoai { get; set; } = string.Empty;

    public string? MoTa { get; set; }

    public string? HangSanXuat { get; set; }
}
