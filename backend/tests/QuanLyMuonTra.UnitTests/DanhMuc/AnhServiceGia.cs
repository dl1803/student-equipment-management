using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

internal sealed class AnhServiceGia : IAnhService
{
    public List<Guid> DsAnhHienCo { get; } = [];

    public List<string> LichSuGoi { get; } = [];

    public List<(string DoiTuongLoai, string DoiTuongId, string GiaiDoan, Guid[] DsMaAnh)> CacLanGan { get; } = [];

    public List<(string MaThietBi, Guid[] DsMaAnh)> CacLanGo { get; } = [];

    public Task<Guid> TaiLenAsync(string giaiDoan, Stream noiDung, CancellationToken ct) =>
        Task.FromResult(Guid.NewGuid());

    public Task GanAsync(
        IReadOnlyCollection<Guid> dsMaAnh,
        string doiTuongLoai,
        string doiTuongId,
        string giaiDoan,
        CancellationToken ct)
    {
        LichSuGoi.Add("GAN");
        CacLanGan.Add((doiTuongLoai, doiTuongId, giaiDoan, dsMaAnh.ToArray()));
        return Task.CompletedTask;
    }

    public Task GoAnhThietBiAsync(string maThietBi, IReadOnlyCollection<Guid> dsMaAnh, CancellationToken ct)
    {
        LichSuGoi.Add("GO");
        CacLanGo.Add((maThietBi, dsMaAnh.ToArray()));
        return Task.CompletedTask;
    }

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
        Task.FromResult<IReadOnlyList<AnhRefDto>>(
            DsAnhHienCo.Select(ma => new AnhRefDto(ma, $"https://luu-tru-gia.test/{ma}")).ToList());
}
