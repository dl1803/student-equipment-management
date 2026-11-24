using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

public sealed class GioQuayJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] CacDinhDang = ["HH:mm", "HH:mm:ss"];

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var chuoi = reader.GetString();
        if (chuoi is not null
            && TimeOnly.TryParseExact(chuoi, CacDinhDang, CultureInfo.InvariantCulture, DateTimeStyles.None, out var gio))
        {
            return gio;
        }

        throw new JsonException("Giờ phải có dạng HH:mm hoặc HH:mm:ss.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
    }
}
