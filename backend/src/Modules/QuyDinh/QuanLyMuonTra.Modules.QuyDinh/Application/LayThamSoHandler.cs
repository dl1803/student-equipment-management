using QuanLyMuonTra.Modules.QuyDinh.Contracts;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

public sealed class LayThamSoHandler
{
    private readonly IQuyDinhReader _boDoc;

    public LayThamSoHandler(IQuyDinhReader boDoc)
    {
        _boDoc = boDoc;
    }

    public Task<ThamSoHeThongDto> XuLyAsync(CancellationToken ct)
    {
        return _boDoc.LayThamSoAsync(ct);
    }
}
