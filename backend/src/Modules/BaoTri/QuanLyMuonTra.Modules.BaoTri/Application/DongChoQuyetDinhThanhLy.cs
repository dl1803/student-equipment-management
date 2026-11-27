namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class DongChoQuyetDinhThanhLy
{
    public string MaThietBi { get; set; } = string.Empty;

    public string TenThietBi { get; set; } = string.Empty;

    public Guid MaLoai { get; set; }

    public Guid MaPhong { get; set; }

    public string TenPhong { get; set; } = string.Empty;

    public Guid MaBaoTri { get; set; }

    public string? NoiDungThucHien { get; set; }

    public DateTimeOffset NgayHoanThanh { get; set; }

    public long GiaTriHienHanh { get; set; }
}
