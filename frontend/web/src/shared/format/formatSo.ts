// Định dạng tiền và tỷ lệ phần trăm (FC 7.2 [ĐX-FC-12], DS 9.7). Sở hữu: Thịnh (shared core).
// Client không tính tiền (FA-07); chỉ định dạng số do máy chủ trả.
import { GIA_TRI_THIEU } from "./formatThoiGian";

/** Khoảng trắng không ngắt giữa số và đơn vị (DS 9.7). */
const KHOANG_TRANG_KHONG_NGAT = " ";

/**
 * Số nguyên VNĐ → `240.000 VNĐ`. Nhóm hàng nghìn bằng biểu thức chính quy để kết quả không
 * phụ thuộc dữ liệu locale của môi trường chạy (FC 7.2).
 */
export function formatTien(soTien: number | null | undefined): string {
  if (soTien == null || !Number.isFinite(soTien)) return GIA_TRI_THIEU;
  const chuoiSo = Number.isInteger(soTien)
    ? String(soTien).replace(/\B(?=(\d{3})+(?!\d))/g, ".")
    : String(soTien);
  return `${chuoiSo}${KHOANG_TRANG_KHONG_NGAT}VNĐ`;
}

/**
 * Tỷ lệ phần trăm từ API (ví dụ `15.5`) → `15,5%`. Giữ nguyên giá trị: không nhân 100,
 * không làm tròn; chỉ đổi dấu thập phân thành dấu phẩy (FC 7.2, DS 9.7).
 */
export function formatTyLePhanTram(tyLe: number | null | undefined): string {
  if (tyLe == null || !Number.isFinite(tyLe)) return GIA_TRI_THIEU;
  return `${String(tyLe).replace(".", ",")}%`;
}
