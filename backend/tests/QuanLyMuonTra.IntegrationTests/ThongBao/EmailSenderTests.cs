using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyMuonTra.Infrastructure.Email;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.ThongBao;

public sealed class EmailSenderTests
{
    [Fact]
    public async Task LogEmailSender_KhongNemLoi()
    {
        var boGui = new LogEmailSender(NullLogger<LogEmailSender>.Instance);

        await boGui.GuiAsync("sv@example.test", "Tiêu đề", "<p>Nội dung</p>", CancellationToken.None);
    }

    [Fact]
    public async Task HttpEmailSender_GuiDungDiaChiHeaderVaThan()
    {
        var xuLy = new XuLyGia(HttpStatusCode.Created);
        var boGui = new HttpEmailSender(new HttpClient(xuLy), "khoa-kiem-thu", "gui@example.test");

        await boGui.GuiAsync("sv@example.test", "Tiêu đề", "<p>Nội dung</p>", CancellationToken.None);

        Assert.Equal(HttpMethod.Post, xuLy.PhuongThuc);
        Assert.Equal(HttpEmailSender.DiaChiApi, xuLy.DiaChi);
        Assert.Equal("khoa-kiem-thu", xuLy.KhoaApi);
        using var json = JsonDocument.Parse(xuLy.Than!);
        var goc = json.RootElement;
        Assert.Equal("gui@example.test", goc.GetProperty("sender").GetProperty("email").GetString());
        Assert.Equal("sv@example.test", goc.GetProperty("to")[0].GetProperty("email").GetString());
        Assert.Equal("Tiêu đề", goc.GetProperty("subject").GetString());
        Assert.Equal("<p>Nội dung</p>", goc.GetProperty("htmlContent").GetString());
    }

    [Fact]
    public async Task HttpEmailSender_NhaCungCapTraLoi_NemLoiChiCoMaTrangThai()
    {
        var boGui = new HttpEmailSender(new HttpClient(new XuLyGia(HttpStatusCode.BadRequest)), "khoa-bi-mat-123", "gui@example.test");

        var loi = await Assert.ThrowsAsync<InvalidOperationException>(
            () => boGui.GuiAsync("sv@example.test", "T", "<p>N</p>", CancellationToken.None));

        Assert.Contains("400", loi.Message);
        Assert.DoesNotContain("khoa-bi-mat-123", loi.Message);
    }

    [Fact]
    public void Di_ProviderTrongHoacLog_DungLogEmailSender()
    {
        foreach (var provider in new string?[] { null, "", "  ", "Log", "log" })
        {
            using var dichVu = new ServiceCollection()
                .AddLogging()
                .AddEmailSender(new EmailOptions(provider, null, null))
                .BuildServiceProvider();

            Assert.IsType<LogEmailSender>(dichVu.GetRequiredService<IEmailSender>());
        }
    }

    [Fact]
    public void Di_ProviderHttpDayDu_DungHttpEmailSender()
    {
        using var dichVu = new ServiceCollection()
            .AddLogging()
            .AddEmailSender(new EmailOptions("Http", "khoa-kiem-thu", "gui@example.test"))
            .BuildServiceProvider();

        Assert.IsType<HttpEmailSender>(dichVu.GetRequiredService<IEmailSender>());
    }

    [Fact]
    public void Di_ProviderHttpThieuCauHinh_NemKhiDangKy()
    {
        Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddEmailSender(new EmailOptions("Http", null, "gui@example.test")));
        Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddEmailSender(new EmailOptions("Http", "khoa", " ")));
    }

    [Fact]
    public void Di_ProviderKhongHopLe_NemKhiDangKy()
    {
        Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddEmailSender(new EmailOptions("Smtp", "khoa", "gui@example.test")));
    }

    private sealed class XuLyGia(HttpStatusCode maTrangThai) : HttpMessageHandler
    {
        public HttpMethod? PhuongThuc { get; private set; }

        public string? DiaChi { get; private set; }

        public string? KhoaApi { get; private set; }

        public string? Than { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            PhuongThuc = request.Method;
            DiaChi = request.RequestUri?.ToString();
            KhoaApi = request.Headers.TryGetValues("api-key", out var cacGiaTri) ? cacGiaTri.Single() : null;
            Than = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(maTrangThai);
        }
    }
}

