using QuanLyMuonTra.Modules.MuonTra.Application;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

internal sealed class ThongBaoDuyetOutboxChuaNoi : IThongBaoDuyetOutbox
{
    public Task GhiAsync(ThongBaoDuyetHoacTuChoi thongBao, CancellationToken ct)
    {
        throw new InvalidOperationException(
            "Chưa nối cổng thông báo C7 (D09). Duyệt và từ chối phiếu yêu cầu bị chặn ở runtime cho đến khi D10-V nối lại.");
    }
}
