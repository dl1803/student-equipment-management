namespace QuanLyMuonTra.Modules.DanhMuc.Contracts;

public interface IDanhMucReader
{
    Task<ThietBiDto?> LayThietBiAsync(string maThietBi, CancellationToken ct);

    Task<LoaiThietBiDto?> LayLoaiAsync(Guid maLoai, CancellationToken ct);

    Task<PhongThietBiDto?> LayPhongAsync(Guid maPhong, CancellationToken ct);

    Task<IReadOnlyList<PhongThietBiDto>> DsPhongAsync(CancellationToken ct);

    Task<IReadOnlyList<LoaiThietBiDto>> DsLoaiDangSuDungAsync(CancellationToken ct);
}

