namespace QuanLyMuonTra.SharedKernel.Contracts;

public interface IAdvisoryLockService
{
    Task LockPhongLoaiAsync(Guid maPhong, Guid maLoai, CancellationToken ct);

    Task LockDanhSachPhongLoaiAsync(IEnumerable<(Guid MaPhong, Guid MaLoai)> danhSach, CancellationToken ct);
}
