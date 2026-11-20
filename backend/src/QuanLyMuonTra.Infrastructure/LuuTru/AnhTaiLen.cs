namespace QuanLyMuonTra.Infrastructure.LuuTru;

public class AnhTaiLen
{
    public Guid MaAnh { get; set; }

    public string Bucket { get; set; } = string.Empty;

    public string KhoaLuuTru { get; set; } = string.Empty;

    public string LoaiNoiDung { get; set; } = string.Empty;

    public int KichThuoc { get; set; }

    public Guid NguoiTaiLen { get; set; }

    public DateTimeOffset ThoiDiem { get; set; }

    public string? DoiTuongLoai { get; set; }

    public string? DoiTuongId { get; set; }

    public string? GiaiDoan { get; set; }
}

