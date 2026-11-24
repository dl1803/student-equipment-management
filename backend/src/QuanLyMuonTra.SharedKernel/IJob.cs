namespace QuanLyMuonTra.SharedKernel;

public interface IJob
{
    string MaJob { get; }

    string TenJob { get; }

    TimeSpan ChuKy { get; }

    Task<int> ChayAsync(CancellationToken ct);
}

