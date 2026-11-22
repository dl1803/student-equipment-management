using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

internal sealed class AnhServiceRong : IAnhService
{
    public Task<Guid> TaiLenAsync(string giaiDoan, Stream noiDung, CancellationToken ct) =>
        Task.FromResult(Guid.NewGuid());

    public Task GanAsync(
        IReadOnlyCollection<Guid> dsMaAnh,
        string doiTuongLoai,
        string doiTuongId,
        string giaiDoan,
        CancellationToken ct) =>
        Task.CompletedTask;

    public Task GoAnhThietBiAsync(string maThietBi, IReadOnlyCollection<Guid> dsMaAnh, CancellationToken ct) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<AnhRefDto>> LayDsAnhKyAsync(
        string doiTuongLoai,
        string doiTuongId,
        string giaiDoan,
        CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<AnhRefDto>>([]);

    public Task<IReadOnlyList<AnhRefDto>> LayDsAnhCongKhaiAsync(
        string doiTuongLoai,
        string doiTuongId,
        CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<AnhRefDto>>([]);
}
