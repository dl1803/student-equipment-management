namespace QuanLyMuonTra.Modules.TaiKhoan.Domain;

public sealed class NhanVienPhong
{
    private NhanVienPhong()
    {
    }

    public Guid MaNguoiDung { get; private set; }

    public Guid MaPhong { get; private set; }

    public static NhanVienPhong Tao(Guid maNguoiDung, Guid maPhong) =>
        new()
        {
            MaNguoiDung = maNguoiDung,
            MaPhong = maPhong
        };
}

