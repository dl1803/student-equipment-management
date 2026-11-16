namespace QuanLyMuonTra.Modules.TaiKhoan.Domain;

public sealed class QuanTriVien : NguoiDungHeThong
{
    private QuanTriVien()
    {
    }

    public string MaQt { get; private set; } = string.Empty;

    public override string VaiTro => MaVaiTro.QuanTri;

    public static QuanTriVien Tao(Guid maNguoiDung, string tenDangNhap, string matKhauBam, string hoTen, string maQt) =>
        new()
        {
            MaNguoiDung = maNguoiDung,
            TenDangNhap = tenDangNhap,
            MatKhauBam = matKhauBam,
            HoTen = hoTen,
            MaQt = maQt
        };
}

