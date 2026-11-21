namespace QuanLyMuonTra.SharedKernel;

public interface IEmailSender
{
    Task GuiAsync(string nguoiNhan, string tieuDe, string noiDungHtml, CancellationToken ct);
}

