using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Infrastructure.Auditing;

internal static class NhatKyJson
{
    private static readonly JsonSerializerOptions TuyChon = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly HashSet<string> TruongNhayCam =
    [
        "matkhaubam", "mabam", "matkhau", "matkhaumoi", "refreshtoken", "accesstoken"
    ];

    public static string? ChuyenSangJson(IEnumerable<KeyValuePair<string, object?>> cacTruong)
    {
        var duocGhi = cacTruong
            .Where(truong => !TruongNhayCam.Contains(ChuanHoaTen(truong.Key)))
            .ToDictionary(truong => truong.Key, truong => truong.Value);

        return duocGhi.Count == 0 ? null : JsonSerializer.Serialize(duocGhi, TuyChon);
    }

    private static string ChuanHoaTen(string ten) => ten.Replace("_", string.Empty).ToLowerInvariant();
}

