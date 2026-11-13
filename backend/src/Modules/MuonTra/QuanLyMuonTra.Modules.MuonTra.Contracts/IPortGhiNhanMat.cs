namespace QuanLyMuonTra.Modules.MuonTra.Contracts;

public interface IPortGhiNhanMat
{
    Task<bool> GhiNhanMatAsync(Guid maChiTietPhieu, Guid maNvNhanTra, CancellationToken ct);
}
