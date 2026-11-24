namespace QuanLyMuonTra.SharedKernel.Contracts;

public interface IAvailabilityService
{
    Task<int> TinhKhaDungAsync(Guid maPhong, Guid maLoai, CancellationToken ct);

    Task<IReadOnlyList<ViTriSanSangDto>> DsViTriSanSangAsync(Guid maPhong, Guid maLoai, CancellationToken ct);
}
