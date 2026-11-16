using System.Security.Cryptography;
using System.Text;

namespace QuanLyMuonTra.Modules.TaiKhoan.Domain;

public sealed class RefreshToken
{
    public const int ThoiHanNgay = 7;

    private RefreshToken()
    {
    }

    public Guid Ma { get; private set; }

    public string ChuSoHuuLoai { get; private set; } = string.Empty;

    public string ChuSoHuuMa { get; private set; } = string.Empty;

    public string MaBam { get; private set; } = string.Empty;

    public DateTimeOffset HetHan { get; private set; }

    public bool DaThuHoi { get; private set; }

    public Guid? ThayTheBoi { get; private set; }

    public static (RefreshToken BanGhi, string TokenGoc) TaoMoi(string chuSoHuuLoai, string chuSoHuuMa, DateTimeOffset bayGio)
    {
        var tokenGoc = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var banGhi = new RefreshToken
        {
            Ma = Guid.CreateVersion7(),
            ChuSoHuuLoai = chuSoHuuLoai,
            ChuSoHuuMa = chuSoHuuMa,
            MaBam = Bam(tokenGoc),
            HetHan = bayGio.AddDays(ThoiHanNgay)
        };

        return (banGhi, tokenGoc);
    }

    public static string Bam(string tokenGoc) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokenGoc)));
}

