namespace QuanLyMuonTra.Modules.TaiKhoan.Contracts;

public interface INhanVienEmailReader
{
    Task<string?> LayEmailAsync(Guid maNv, CancellationToken ct);
}

