namespace QuanLyMuonTra.Modules.MuonTra.Contracts;

public interface IPhieuMuonReader
{
    Task<PhieuMuonDto?> LayPhieuMuonAsync(Guid maPhieu, CancellationToken ct);

    Task<ChiTietPhieuMuonDto?> LayChiTietAsync(Guid maChiTiet, CancellationToken ct);

    Task<IReadOnlyList<ChiTietPhieuMuonDto>> LayChiTietTheoPhieuAsync(Guid maPhieu, CancellationToken ct);

    Task<IReadOnlyList<PhieuMuonDto>> LayDsQuaHan30NgayAsync(CancellationToken ct);
}
