namespace QuanLyMuonTra.Modules.TaiKhoan.Domain;

public sealed class NhanVienPhong
{
    private NhanVienPhong()
    {
    }

    public Guid MaNguoiDung { get; private set; }

    public Guid MaPhong { get; private set; }
}

