namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record ThongBaoDuyetHoacTuChoi(
    string MaYeuCau,
    string MaSv,
    bool DaDuyet,
    DateTimeOffset? HanGiuCho,
    string? LyDoTuChoi);

public interface IThongBaoDuyetOutbox
{
    Task GhiAsync(ThongBaoDuyetHoacTuChoi thongBao, CancellationToken ct);
}
