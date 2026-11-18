using QuanLyMuonTra.SharedKernel;

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

    public void CapNhatThongTin(string hoTen, string? email)
    {
        HoTen = hoTen;
        Email = email;
    }

    public void Khoa()
    {
        if (TrangThaiKhoa == MaTrangThaiKhoa.BiKhoa)
        {
            throw new LoiNghiepVuException("TRANG_THAI_KHONG_HOP_LE", "Tài khoản đã ở trạng thái bị khóa.");
        }

        TrangThaiKhoa = MaTrangThaiKhoa.BiKhoa;
    }

    public void MoKhoa()
    {
        if (DangHoatDong)
        {
            throw new LoiNghiepVuException("TRANG_THAI_KHONG_HOP_LE", "Tài khoản đang hoạt động, không cần mở khóa.");
        }

        TrangThaiKhoa = MaTrangThaiKhoa.HoatDong;
        SoLanDangNhapSai = 0;
    }

    public void DatLaiMatKhau(string matKhauBam)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(matKhauBam);

        MatKhauBam = matKhauBam;
    }
}
