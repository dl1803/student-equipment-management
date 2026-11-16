namespace QuanLyMuonTra.Modules.TaiKhoan.Domain;

public sealed class NhanVienQuanLy : NguoiDungHeThong
{
    private NhanVienQuanLy()
    {
    }

    public string MaNv { get; private set; } = string.Empty;

    public override string VaiTro => MaVaiTro.NhanVien;
}

