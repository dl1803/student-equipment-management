namespace QuanLyMuonTra.Modules.SinhVien.Contracts;

public interface IBienBanChuaNopReader
{
    Task<IReadOnlyList<BienBanChuaNopDto>> LayDsChuaNopAsync(string maSv, CancellationToken ct);
}

