namespace QuanLyMuonTra.Modules.MuonTra.Contracts;

public interface IFifoService
{
    Task<DangKyChoDto?> CapLuotChoAsync(Guid maPhong, Guid maLoai, CancellationToken ct);
}
