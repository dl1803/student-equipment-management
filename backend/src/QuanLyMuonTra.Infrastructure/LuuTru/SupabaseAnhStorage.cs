using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Infrastructure.LuuTru;

public sealed class SupabaseAnhStorage(HttpClient httpClient, SupabaseStorageOptions tuyChon) : IAnhStorage
{
    public async Task UploadAsync(string bucket, string khoaLuuTru, string loaiNoiDung, byte[] noiDung, CancellationToken ct)
    {
        using var yeuCau = TaoYeuCau(HttpMethod.Post, $"object/{bucket}/{khoaLuuTru}");
        yeuCau.Headers.Add("x-upsert", "false");
        yeuCau.Content = new ByteArrayContent(noiDung);
        yeuCau.Content.Headers.ContentType = new MediaTypeHeaderValue(loaiNoiDung);

        using var phanHoi = await httpClient.SendAsync(yeuCau, ct);
        await DamBaoThanhCongAsync(phanHoi, "tải ảnh lên", ct);
    }

    public async Task<string> TaoUrlKyAsync(string bucket, string khoaLuuTru, TimeSpan hieuLuc, CancellationToken ct)
    {
        using var yeuCau = TaoYeuCau(HttpMethod.Post, $"object/sign/{bucket}/{khoaLuuTru}");
        yeuCau.Content = JsonContent.Create(new { expiresIn = (int)hieuLuc.TotalSeconds });

        using var phanHoi = await httpClient.SendAsync(yeuCau, ct);
        await DamBaoThanhCongAsync(phanHoi, "cấp URL có chữ ký", ct);

        var ketQua = await phanHoi.Content.ReadFromJsonAsync<KetQuaKy>(ct);
        var duongDanKy = ketQua?.SignedUrl;
        if (string.IsNullOrWhiteSpace(duongDanKy))
        {
            throw new InvalidOperationException("Supabase Storage không trả đường dẫn có chữ ký.");
        }

        return $"{GocStorage()}{duongDanKy}";
    }

    public async Task XoaAsync(string bucket, string khoaLuuTru, CancellationToken ct)
    {
        using var yeuCau = TaoYeuCau(HttpMethod.Delete, $"object/{bucket}/{khoaLuuTru}");

        using var phanHoi = await httpClient.SendAsync(yeuCau, ct);
        if (phanHoi.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        await DamBaoThanhCongAsync(phanHoi, "xóa ảnh", ct);
    }

    public string TaoUrlCongKhai(string bucket, string khoaLuuTru) =>
        $"{GocStorage()}/object/public/{bucket}/{khoaLuuTru}";

    private static async Task DamBaoThanhCongAsync(HttpResponseMessage phanHoi, string thaoTac, CancellationToken ct)
    {
        if (phanHoi.IsSuccessStatusCode)
        {
            return;
        }

        var noiDung = await phanHoi.Content.ReadAsStringAsync(ct);
        if (noiDung.Length > 300)
        {
            noiDung = noiDung[..300];
        }

        throw new InvalidOperationException($"Supabase Storage trả {(int)phanHoi.StatusCode} khi {thaoTac}: {noiDung}");
    }

    private (string Url, string Khoa) DocCauHinh()
    {
        if (string.IsNullOrWhiteSpace(tuyChon.Url) || string.IsNullOrWhiteSpace(tuyChon.ServiceRoleKey))
        {
            throw new InvalidOperationException("Thiếu cấu hình Supabase:Url hoặc Supabase:ServiceRoleKey.");
        }

        return (tuyChon.Url, tuyChon.ServiceRoleKey);
    }

    private string GocStorage() => $"{DocCauHinh().Url.TrimEnd('/')}/storage/v1";

    private HttpRequestMessage TaoYeuCau(HttpMethod phuongThuc, string duongDan)
    {
        var khoa = DocCauHinh().Khoa;
        var yeuCau = new HttpRequestMessage(phuongThuc, $"{GocStorage()}/{duongDan}");
        yeuCau.Headers.Authorization = new AuthenticationHeaderValue("Bearer", khoa);
        yeuCau.Headers.Add("apikey", khoa);

        return yeuCau;
    }

    private sealed record KetQuaKy([property: JsonPropertyName("signedURL")] string? SignedUrl);
}

