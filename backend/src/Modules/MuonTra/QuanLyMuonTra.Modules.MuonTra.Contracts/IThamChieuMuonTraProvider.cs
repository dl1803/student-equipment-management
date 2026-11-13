namespace QuanLyMuonTra.Modules.MuonTra.Contracts;

public interface IThamChieuMuonTraProvider
{
    Task<bool> CoThamChieuPhongAsync(Guid maPhong, CancellationToken ct);

    Task<bool> CoThamChieuLoaiAsync(Guid maLoai, CancellationToken ct);

    Task<bool> CoThamChieuThietBiAsync(string maThietBi, CancellationToken ct);

    Task<bool> CoThamChieuKhungPhatAsync(Guid maKhung, CancellationToken ct);
}
