namespace QuanLyMuonTra.Modules.TaiKhoan.Contracts;

public interface IQuyenPhongService
{
    Task<IReadOnlyCollection<Guid>> LayDsMaPhongAsync(Guid maNguoiDung, CancellationToken ct);

    Task EnsureAsync(Guid maNguoiDung, Guid maPhong, CancellationToken ct);
}

