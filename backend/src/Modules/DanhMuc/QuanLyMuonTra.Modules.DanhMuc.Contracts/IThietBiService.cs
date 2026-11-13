namespace QuanLyMuonTra.Modules.DanhMuc.Contracts;

public interface IThietBiService
{
    Task<bool> DatDangDuocMuonAsync(string maThietBi, Guid maPhong, Guid maLoai, CancellationToken ct);

    Task<bool> NhanTraNguyenVenAsync(string maThietBi, CancellationToken ct);

    Task<bool> NhanTraHuHongAsync(string maThietBi, CancellationToken ct);

    Task<bool> DanhDauMatAsync(string maThietBi, CancellationToken ct);

    Task<bool> ChuyenBaoTriAsync(string maThietBi, CancellationToken ct);

    Task<bool> HoanThanhBaoTriAsync(string maThietBi, CancellationToken ct);

    Task<bool> ThanhLyAsync(string maThietBi, CancellationToken ct);
}

