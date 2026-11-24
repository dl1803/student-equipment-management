namespace QuanLyMuonTra.Modules.ThongBao.Contracts;

public interface IThongBaoOutbox
{
    Task GhiAsync(YeuCauGhiThongBao yeuCau, CancellationToken ct);
}

