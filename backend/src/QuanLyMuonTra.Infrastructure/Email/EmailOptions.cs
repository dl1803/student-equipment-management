namespace QuanLyMuonTra.Infrastructure.Email;

public sealed class EmailOptions(string? provider, string? apiKey, string? from)
{
    public string? Provider { get; } = provider;

    public string? ApiKey { get; } = apiKey;

    public string? From { get; } = from;
}

