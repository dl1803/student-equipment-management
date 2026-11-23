namespace QuanLyMuonTra.Modules.SinhVien.Contracts;

public interface IQuyenMuonService
{
    Task<KetQuaDoiQuyenMuon> KhoaAsync(string maSv, CancellationToken ct);

    Task<KetQuaDoiQuyenMuon> MoKhoaAsync(string maSv, CancellationToken ct);
}

public sealed record KetQuaDoiQuyenMuon(string TrangThaiTruoc, string TrangThaiSau)
{
    public bool DaThayDoi => TrangThaiTruoc != TrangThaiSau;
}

