namespace QuanLyMuonTra.Modules.DanhMuc.Contracts;

public interface IThamChieuDanhMucProvider
{
    Task<bool> CoThamChieuThietBiAsync(string maThietBi, CancellationToken ct);

    Task<bool> CoThamChieuLoaiAsync(Guid maLoai, CancellationToken ct);
}

