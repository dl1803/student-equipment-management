using Microsoft.Extensions.Logging;
using QuanLyMuonTra.Modules.MuonTra.Application;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

internal sealed class ThongBaoDuyetOutboxChuaNoi(ILogger<ThongBaoDuyetOutboxChuaNoi>? logger = null) : IThongBaoDuyetOutbox
{
    public Task GhiAsync(ThongBaoDuyetHoacTuChoi thongBao, CancellationToken ct)
    {
        logger?.LogWarning(
            "Chưa nối cổng thông báo C7 (D09). Tạm thời bỏ qua outbox cho yêu cầu {MaYeuCau}.",
            thongBao.MaYeuCau);
        return Task.CompletedTask;
    }
}
