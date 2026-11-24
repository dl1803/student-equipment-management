namespace QuanLyMuonTra.Infrastructure.LuuTru;

public static class DinhDangAnh
{
    public const string Jpeg = "image/jpeg";

    public const string Png = "image/png";

    public const string WebP = "image/webp";

    private static readonly byte[] ChuKyPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static string? NhanDang(ReadOnlySpan<byte> duLieu)
    {
        if (duLieu.Length >= 3 && duLieu[0] == 0xFF && duLieu[1] == 0xD8 && duLieu[2] == 0xFF)
        {
            return Jpeg;
        }

        if (duLieu.Length >= ChuKyPng.Length && duLieu[..ChuKyPng.Length].SequenceEqual(ChuKyPng))
        {
            return Png;
        }

        if (duLieu.Length >= 12 && duLieu[..4].SequenceEqual("RIFF"u8) && duLieu[8..12].SequenceEqual("WEBP"u8))
        {
            return WebP;
        }

        return null;
    }
}

