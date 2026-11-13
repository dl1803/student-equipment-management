namespace QuanLyMuonTra.Modules.MuonTra.Contracts;

public interface IMuonTraReader
{
    Task<int> DemThietBiDangCamAsync(string maSv, CancellationToken ct);
}
