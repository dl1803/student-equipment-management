using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

internal sealed class MaTrangThaiConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    internal static readonly Dictionary<string, TEnum> TheoMa = Enum.GetValues<TEnum>().ToDictionary(ChuyenSangMa);

    public MaTrangThaiConverter()
        : base(giaTri => ChuyenSangMa(giaTri), ma => TheoMa[ma])
    {
    }

    internal static string ChuyenSangMa(TEnum giaTri)
    {
        var ten = giaTri.ToString();
        var ketQua = new StringBuilder();
        for (var i = 0; i < ten.Length; i++)
        {
            if (i > 0 && char.IsUpper(ten[i]))
            {
                ketQua.Append('_');
            }

            ketQua.Append(char.ToUpperInvariant(ten[i]));
        }

        return ketQua.ToString();
    }
}
