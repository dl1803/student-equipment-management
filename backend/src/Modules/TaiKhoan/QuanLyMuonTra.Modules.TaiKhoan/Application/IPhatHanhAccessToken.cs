namespace QuanLyMuonTra.Modules.TaiKhoan.Application;

public interface IPhatHanhAccessToken
{
    string Tao(Guid maNguoiDung, string vaiTro);

    string Tao(string maDinhDanh, string vaiTro);
}
