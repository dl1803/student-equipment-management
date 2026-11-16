namespace QuanLyMuonTra.Modules.TaiKhoan.Contracts;

public interface ITrangThaiTaiKhoanReader
{
    Task<string?> LayTrangThaiKhoaAsync(Guid maNguoiDung, CancellationToken ct);
}

