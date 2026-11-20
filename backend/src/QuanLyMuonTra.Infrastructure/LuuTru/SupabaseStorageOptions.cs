namespace QuanLyMuonTra.Infrastructure.LuuTru;

public sealed class SupabaseStorageOptions(string? url, string? serviceRoleKey)
{
    public string? Url { get; } = url;

    public string? ServiceRoleKey { get; } = serviceRoleKey;
}

