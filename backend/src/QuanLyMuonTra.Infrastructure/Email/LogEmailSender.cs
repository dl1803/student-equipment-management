using Microsoft.Extensions.Logging;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure.Email;

public sealed class LogEmailSender(ILogger<LogEmailSender> nhatKy) : IEmailSender
{
    public Task GuiAsync(string nguoiNhan, string tieuDe, string noiDungHtml, CancellationToken ct)
    {
        nhatKy.LogInformation(
            "Email (chế độ Log, không gửi thật) tới {NguoiNhan}. Tiêu đề: {TieuDe}. Nội dung: {NoiDung}",
            nguoiNhan,
            tieuDe,
            noiDungHtml);

        return Task.CompletedTask;
    }
}

