// Kho token của web (FA 3.5, 4.5; FC 5.10). Sở hữu: Thịnh (shared core).
// Đây là NƠI DUY NHẤT đọc, ghi, xóa token:
// - access token: biến của module, chỉ trong bộ nhớ; mất khi tải lại trang (FA-06).
// - refresh token: sessionStorage của từng tab (ĐX-FA-02). Không cookie, không localStorage.
import type { ApiError } from "../apiError";
import type { LyDoKetThucPhien } from "./trangThaiPhien";

/** Khóa lưu refresh token (FC 7.1 [ĐX-FC-03]). */
export const KHOA_REFRESH_TOKEN = "refreshToken";

export type SuKienKetThucPhien = {
  /** Không có lyDo khi người dùng chủ động đăng xuất. */
  lyDo?: LyDoKetThucPhien;
  /** Lỗi gây kết thúc phiên, giữ nguyên để màn hình đăng nhập báo đúng nguyên nhân (FA 3.5). */
  loi?: ApiError;
};

type LangNghe = (suKien: SuKienKetThucPhien) => void;

let accessToken: string | null = null;
const dsLangNghe = new Set<LangNghe>();

export function layAccessToken(): string | null {
  return accessToken;
}

/** sessionStorage có thể không dùng được (chế độ riêng tư, SSR); khi đó coi như không có phiên. */
export function layRefreshToken(): string | null {
  try {
    return typeof sessionStorage === "undefined"
      ? null
      : sessionStorage.getItem(KHOA_REFRESH_TOKEN);
  } catch {
    return null;
  }
}

/**
 * Ghi cặp token mới. Refresh token xuống sessionStorage TRƯỚC, rồi mới đặt access token, để mọi
 * yêu cầu được gọi lại sau đó đều thấy cặp token mới (FA 3.5, mục Ghi token mới).
 */
export function ghiCapToken(cap: {
  accessToken: string;
  refreshToken: string;
}): void {
  sessionStorage.setItem(KHOA_REFRESH_TOKEN, cap.refreshToken);
  accessToken = cap.accessToken;
}

/**
 * Xóa token khỏi bộ nhớ và sessionStorage rồi báo cho người lắng nghe (AuthProvider đặt
 * TrangThaiPhien, xóa bộ nhớ đệm truy vấn, chuyển tới /dang-nhap — FA 4.5).
 */
export function xoaPhien(lyDo?: LyDoKetThucPhien, loi?: ApiError): void {
  accessToken = null;
  try {
    sessionStorage.removeItem(KHOA_REFRESH_TOKEN);
  } catch {
    // sessionStorage không dùng được thì cũng không còn gì để xóa.
  }
  const suKien: SuKienKetThucPhien = {};
  if (lyDo !== undefined) suKien.lyDo = lyDo;
  if (loi !== undefined) suKien.loi = loi;
  for (const langNghe of [...dsLangNghe]) langNghe(suKien);
}

/** Đăng ký lắng nghe phiên kết thúc; trả hàm hủy để gọi trong hàm dọn của useEffect (FC 5.3). */
export function dangKyKetThucPhien(langNghe: LangNghe): () => void {
  dsLangNghe.add(langNghe);
  return () => {
    dsLangNghe.delete(langNghe);
  };
}
