using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

internal sealed class AvailabilityServiceGia(int khaDung = 10) : IAvailabilityService
{
    public int KhaDungTraVe { get; set; } = khaDung;

    public Task<int> TinhKhaDungAsync(Guid maPhong, Guid maLoai, CancellationToken ct) =>
        Task.FromResult(KhaDungTraVe);

    public Task<IReadOnlyList<ViTriSanSangDto>> DsViTriSanSangAsync(Guid maPhong, Guid maLoai, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<ViTriSanSangDto>>([]);
}
