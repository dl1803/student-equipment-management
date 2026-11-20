namespace QuanLyMuonTra.SharedKernel;

public sealed record AnhRefDto(Guid MaAnh, string Url);

public interface IAnhService
{
    Task<Guid> TaiLenAsync(string giaiDoan, Stream noiDung, CancellationToken ct);

    Task GanAsync(
        IReadOnlyCollection<Guid> dsMaAnh,
        string doiTuongLoai,
        string doiTuongId,
        string giaiDoan,
        CancellationToken ct);

    Task GoAnhThietBiAsync(string maThietBi, IReadOnlyCollection<Guid> dsMaAnh, CancellationToken ct);

    Task<IReadOnlyList<AnhRefDto>> LayDsAnhKyAsync(
        string doiTuongLoai,
        string doiTuongId,
        string giaiDoan,
        CancellationToken ct);

    Task<IReadOnlyList<AnhRefDto>> LayDsAnhCongKhaiAsync(
        string doiTuongLoai,
        string doiTuongId,
        CancellationToken ct);
}

