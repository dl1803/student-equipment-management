// Cấu hình Axios dùng chung cho mọi thể hiện (FA 4.6, 4.14; FC 5.12, 7.1). Sở hữu: Thịnh.
import axios, { type AxiosInstance } from "axios";

/** Thời gian chờ HTTP (ĐX-FA-04): máy chủ gói miễn phí có thể "ngủ" khoảng một phút. */
export const THOI_GIAN_CHO_HTTP_MS = 90_000;

/** Tiền tố đường dẫn là hằng số của client (FA 3.2); NEXT_PUBLIC_API_BASE_URL không chứa phần này. */
export const TIEN_TO_API = "/api/v1";

/**
 * Đọc địa chỉ gốc API ở một nơi duy nhất (FC 5.12). Đọc lúc gửi yêu cầu, không lúc nạp module, để
 * build các trang chưa gọi API không phụ thuộc biến môi trường; thiếu biến thì ném lỗi ngay ở yêu
 * cầu đầu tiên.
 */
export function layDiaChiGocApi(): string {
  const giaTri = process.env.NEXT_PUBLIC_API_BASE_URL;
  if (giaTri === undefined || giaTri.trim() === "") {
    throw new Error(
      "Thiếu biến môi trường NEXT_PUBLIC_API_BASE_URL (FA 4.14, xem .env.example)",
    );
  }
  return giaTri.trim().replace(/\/+$/, "");
}

/**
 * Tạo một thể hiện Axios: timeout 90 giây, báo quá thời gian bằng mã ETIMEDOUT, baseURL gồm
 * /api/v1. Không đặt tiêu đề Accept (FA 4.6) và không cấu hình retry.
 */
export function taoKhachAxios(): AxiosInstance {
  const khach = axios.create({
    timeout: THOI_GIAN_CHO_HTTP_MS,
    transitional: { clarifyTimeoutError: true },
  });
  khach.interceptors.request.use((config) => {
    config.baseURL = `${layDiaChiGocApi()}${TIEN_TO_API}`;
    return config;
  });
  return khach;
}
