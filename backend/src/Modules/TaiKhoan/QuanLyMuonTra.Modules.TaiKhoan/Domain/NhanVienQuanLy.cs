namespace QuanLyMuonTra.Modules.TaiKhoan.Domain;

public sealed class NhanVienQuanLy : NguoiDungHeThong
{
    private NhanVienQuanLy()
    {
    }

    public string MaNv { get; private set; } = string.Empty;

    public override string VaiTro => MaVaiTro.NhanVien;

    public static NhanVienQuanLy Tao(
        Guid maNguoiDung,
        string tenDangNhap,
        string matKhauBam,
        string hoTen,
        string maNv,
        string? email = null,
        string trangThaiKhoa = MaTrangThaiKhoa.HoatDong) =>
        new()
        {
            MaNguoiDung = maNguoiDung,
            TenDangNhap = tenDangNhap,
            MatKhauBam = matKhauBam,
            HoTen = hoTen,
            MaNv = maNv,
            Email = email,
            TrangThaiKhoa = trangThaiKhoa
        };
}

