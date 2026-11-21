using System.Net.Http.Json;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure.Email;

public sealed class HttpEmailSender(HttpClient httpClient, string apiKey, string diaChiGui) : IEmailSender
{
    public const string DiaChiApi = "https://api.brevo.com/v3/smtp/email";

    public async Task GuiAsync(string nguoiNhan, string tieuDe, string noiDungHtml, CancellationToken ct)
    {
        using var yeuCau = new HttpRequestMessage(HttpMethod.Post, DiaChiApi);
        yeuCau.Headers.Add("api-key", apiKey);
        yeuCau.Content = JsonContent.Create(new
        {
            sender = new { email = diaChiGui },
            to = new[] { new { email = nguoiNhan } },
            subject = tieuDe,
            htmlContent = noiDungHtml
        });

        using var phanHoi = await httpClient.SendAsync(yeuCau, ct);
        if (!phanHoi.IsSuccessStatusCode)
        {
            var chiTietLoi = phanHoi.Content is not null ? await phanHoi.Content.ReadAsStringAsync(ct) : null;
            var thongBao = string.IsNullOrWhiteSpace(chiTietLoi)
                ? $"Dịch vụ email trả mã {(int)phanHoi.StatusCode}."
                : $"Dịch vụ email trả mã {(int)phanHoi.StatusCode}: {chiTietLoi.Trim()}";

            throw new InvalidOperationException(thongBao);
        }
    }
}

