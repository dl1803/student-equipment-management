namespace QuanLyMuonTra.Modules.DanhMuc.Contracts;

public interface IThietBiService
{
    Task<bool> DatDangDuocMuonAsync(string maThietBi, Guid maPhong, Guid maLoai, CancellationToken ct);

    Task<bool> NhanTraNguyenVenAsync(string maThietBi, CancellationToken ct);

    Task<bool> NhanTraHuHongAsync(string maThietBi, CancellationToken ct);

    Task<bool> DanhDauMatAsync(string maThietBi, CancellationToken ct);

    Task<bool> ChuyenBaoTriAsync(string maThietBi, CancellationToken ct);

    Task<bool> HoanThanhBaoTriAsync(string maThietBi, CancellationToken ct);

    /// <summary>
    /// Chuyển tình trạng thiết bị sang ĐÃ THANH LÝ nếu đang SẴN SÀNG hoặc ĐANG BẢO TRÌ SỬA CHỮA.
    /// Yêu cầu bên gọi chạy trong giao dịch và giữ khóa tư vấn cặp (phòng, loại) trước khi gọi.
    /// </summary>
    Task<bool> ThanhLyAsync(string maThietBi, CancellationToken ct);
}

