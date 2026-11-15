// apiClient — thể hiện Axios chung của web (FA 3.5, 4.6; FC 5.4 [ĐX-FC-02]). Sở hữu: Thịnh.
// Hàm API của tính năng gọi qua đây với đường dẫn tương đối không có /api/v1, ví dụ "/sv/toi".
// Mọi lỗi ra khỏi đây là ApiError. Không retry: chỉ gọi lại đúng một lần sau khi làm mới phiên.
import axios, { type InternalAxiosRequestConfig } from "axios";
import { chuanHoaLoi } from "./chuanHoaLoi";
import { lamMoiPhien } from "./session/lamMoi";
import { layAccessToken, xoaPhien } from "./session/phien";
import { taoKhachAxios } from "./taoKhachAxios";

declare module "axios" {
  interface InternalAxiosRequestConfig {
    /** Đã gọi lại sau làm mới; không làm mới lần nữa (FA 3.5). */
    _daGoiLai?: boolean;
    /** Access token đã gắn khi gửi, để biết phiên đã được làm mới bởi yêu cầu khác hay chưa. */
    _tokenDaGui?: string | null;
  }
}

/** Bốn endpoint không yêu cầu xác thực: không gắn token, 401 của chúng không kích hoạt làm mới. */
const DUONG_DAN_KHONG_XAC_THUC = new Set([
  "/auth/sinh-vien/dang-nhap",
  "/auth/nhan-vien/dang-nhap",
  "/auth/lam-moi",
  "/auth/dang-xuat",
]);

function laKhongXacThuc(config: InternalAxiosRequestConfig): boolean {
  const duongDan = (config.url ?? "").split("?")[0] ?? "";
  return DUONG_DAN_KHONG_XAC_THUC.has(duongDan);
}

export const apiClient = taoKhachAxios();

apiClient.interceptors.request.use((config) => {
  if (laKhongXacThuc(config)) return config;
  const token = layAccessToken();
  config._tokenDaGui = token;
  if (token !== null) config.headers.set("Authorization", `Bearer ${token}`);
  return config;
});

apiClient.interceptors.response.use(undefined, async (loiGoc: unknown) => {
  const loi = await chuanHoaLoi(loiGoc);
  const config = axios.isAxiosError(loiGoc) ? loiGoc.config : undefined;

  const canLamMoi =
    config !== undefined &&
    loi.status === 401 &&
    loi.maLoi === "CHUA_XAC_THUC" &&
    !laKhongXacThuc(config) &&
    config._daGoiLai !== true;

  if (canLamMoi) {
    config._daGoiLai = true;
    const tokenHienTai = layAccessToken();
    // Yêu cầu khác đã làm mới thành công sau khi yêu cầu này được gửi: dùng token mới, không làm
    // mới lại bằng refresh token đã xoay vòng.
    if (tokenHienTai === null || tokenHienTai === config._tokenDaGui) {
      await lamMoiPhien(); // Thất bại: phiên đã bị xóa, ném ApiError của lần làm mới, không gọi lại.
    }
    return apiClient.request(config);
  }

  if (
    loi.maLoi === "TAI_KHOAN_BI_KHOA" &&
    config !== undefined &&
    !laKhongXacThuc(config)
  ) {
    xoaPhien("BI_KHOA", loi);
  }
  throw loi;
});
