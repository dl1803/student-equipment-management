namespace QuanLyMuonTra.Modules.MuonTra.Contracts;

/// <summary>
/// Bộ đọc dữ liệu phiếu mượn dành cho các module khác (ViPham, BaoCao).
/// </summary>
public interface IPhieuMuonReader
{
    /// <summary>
    /// Lấy thông tin phiếu mượn theo mã phiếu.
    /// </summary>
    Task<PhieuMuonDto?> LayPhieuMuonAsync(string maPhieu, CancellationToken ct);

    /// <summary>
    /// Lấy chi tiết phiếu mượn theo mã chi tiết.
    /// </summary>
    Task<ChiTietPhieuMuonDto?> LayChiTietAsync(Guid maChiTiet, CancellationToken ct);

    /// <summary>
    /// Lấy danh sách chi tiết thiết bị theo mã phiếu mượn.
    /// </summary>
    Task<IReadOnlyList<ChiTietPhieuMuonDto>> LayChiTietTheoPhieuAsync(string maPhieu, CancellationToken ct);

    /// <summary>
    /// Lấy danh sách phiếu mượn có thiết bị chưa trả quá hạn trên 30 ngày lịch VN.
    /// </summary>
    Task<IReadOnlyList<PhieuMuonDto>> LayDsQuaHan30NgayAsync(CancellationToken ct);
}
