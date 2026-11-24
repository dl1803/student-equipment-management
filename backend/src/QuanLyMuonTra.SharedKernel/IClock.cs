namespace QuanLyMuonTra.SharedKernel;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

