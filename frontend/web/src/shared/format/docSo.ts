// Đọc số do người dùng nhập (FC 5.6, 7.2; FA 4.8). Sở hữu: Thịnh (shared core).
// Chỉ kiểm tra cú pháp; miền giá trị nghiệp vụ do máy chủ kiểm tra (ĐX-FA-08).

const MAU_SO_NGUYEN = /^\d+$/;

/**
 * Chuỗi nhập → số nguyên không âm. Chỉ nhận toàn chuỗi chữ số (sau khi cắt khoảng trắng hai đầu);
 * từ chối số âm, phần thập phân, dấu phân cách, ký tự lạ → null. Không dùng parseFloat.
 */
export function docSoNguyen(chuoi: string): number | null {
  const giaTri = chuoi.trim();
  if (!MAU_SO_NGUYEN.test(giaTri)) return null;
  const so = Number(giaTri);
  return Number.isSafeInteger(so) ? so : null;
}
