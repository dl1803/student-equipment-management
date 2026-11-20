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

/** Kích thước trang tối đa API nhận (UI-Screens 3.7; `KichThuoc` của openapi.yaml: maximum 100). */
export const KICH_THUOC_TRANG_TOI_DA = 100;

/** Phản hồi phân trang chuẩn (KT 6.1): mọi `*Page` của openapi.yaml có hai trường này. */
type TrangDuLieu<T> = { tongSo: number; danhSach: ReadonlyArray<T> };

/**
 * Tải đủ mọi trang của một danh sách làm nguồn lựa chọn (loại, phòng, nhà cung cấp…), lần lượt
 * từng trang cỡ KICH_THUOC_TRANG_TOI_DA, để ô chọn không thiếu mục nằm sau trang 1.
 * Dừng khi đủ `tongSo` của trang đầu, hoặc khi gặp trang rỗng (dữ liệu bị xóa bớt giữa chừng),
 * nên không lặp vô hạn. Thứ tự giữ nguyên thứ tự máy chủ (FA 3.10: client không sắp xếp lại).
 */
export async function taiTatCaTrang<T>(
  goiTrang: (trang: number, kichThuoc: number) => Promise<TrangDuLieu<T>>,
): Promise<T[]> {
  const dau = await goiTrang(1, KICH_THUOC_TRANG_TOI_DA);
  const tatCa: T[] = [...dau.danhSach];
  const tongSoTrang = soTrang(dau.tongSo, KICH_THUOC_TRANG_TOI_DA);
  for (let trang = 2; trang <= tongSoTrang; trang++) {
    const tiep = await goiTrang(trang, KICH_THUOC_TRANG_TOI_DA);
    if (tiep.danhSach.length === 0) break;
    tatCa.push(...tiep.danhSach);
  }
  return tatCa;
}
