namespace QuanLyMuonTra.Modules.SinhVien.Contracts;

public interface ISinhVienEmailReader
{
    Task<string?> LayEmailAsync(string maSv, CancellationToken ct);
}

