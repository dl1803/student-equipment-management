// Phân trang danh sách (FA 3.10; FC 7.1; UI-Screens 3.7). Sở hữu: Thịnh (shared core).
/** Kích thước trang mặc định của API (tối đa 100). */
export const KICH_THUOC_TRANG_MAC_DINH = 20;

/** Số trang theo tổng số dòng máy chủ trả về; ít nhất 1. */
export function soTrang(tongSo: number, kichThuoc: number): number {
  return Math.max(1, Math.ceil(tongSo / kichThuoc));
}

/**
 * Trang cần lùi về khi trang hiện tại vượt quá số trang (vd. xóa dòng cuối cùng của trang cuối,
 * người khác xóa bớt). Trả null nếu trang vẫn hợp lệ. Chỉ dùng với dữ liệu thật của trang hiện
 * tại, không dùng dữ liệu tạm (placeholderData) của trang trước.
 */
export function trangCanLuiVe(
  trang: number,
  tongSo: number,
  kichThuoc: number,
): number | null {
  const trangCuoi = soTrang(tongSo, kichThuoc);
  return trang > trangCuoi ? trangCuoi : null;
}
