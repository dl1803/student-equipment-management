namespace QuanLyMuonTra.SharedKernel;

public interface IAuditLogger
{
    Task GhiAsync(
        string thaoTac,
        string doiTuong,
        string? doiTuongId,
        IReadOnlyDictionary<string, object?>? giaTriTruoc,
        IReadOnlyDictionary<string, object?>? giaTriSau,
        CancellationToken ct);
}

