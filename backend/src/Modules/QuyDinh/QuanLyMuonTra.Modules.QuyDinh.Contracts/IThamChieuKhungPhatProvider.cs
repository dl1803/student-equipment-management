namespace QuanLyMuonTra.Modules.QuyDinh.Contracts;

public interface IThamChieuKhungPhatProvider
{
    Task<bool> CoThamChieuKhungAsync(Guid maKhung, CancellationToken ct);
}

