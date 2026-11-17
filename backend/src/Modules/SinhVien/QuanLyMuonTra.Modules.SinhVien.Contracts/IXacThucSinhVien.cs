namespace QuanLyMuonTra.Modules.SinhVien.Contracts;

public interface IXacThucSinhVien
{
    Task<string?> XacThucAsync(string mssv, string matKhau, CancellationToken ct);
}

