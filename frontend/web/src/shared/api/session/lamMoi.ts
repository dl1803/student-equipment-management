// Làm mới, khôi phục và đăng xuất phiên (FA 3.5, 4.5, 4.6; FC 5.10, ĐX-FA-23). Sở hữu: Thịnh.
import { chuanHoaLoi } from "../chuanHoaLoi";
import type { ApiError } from "../apiError";
import { taoKhachAxios } from "../taoKhachAxios";
import type { DangXuatRequest, LamMoiRequest, TokenResponse } from "../types";
import { ghiCapToken, layRefreshToken, xoaPhien } from "./phien";

/**
 * Thể hiện Axios RIÊNG, không có interceptor xác thực, cho các endpoint phiên: gọi làm mới không
 * bao giờ tự kích hoạt làm mới đệ quy (FA 4.6).
 */
export const khachPhien = taoKhachAxios();

/** Promise làm mới đang chạy; dùng chung cho interceptor và khôi phục phiên (single-flight). */
let dangLamMoi: Promise<string> | null = null;

async function thucHienLamMoi(): Promise<string> {
  const refreshToken = layRefreshToken();
  if (refreshToken === null) {
    const loi: ApiError = { loai: "HTTP", status: 401, maLoi: "CHUA_XAC_THUC" };
    xoaPhien("HET_PHIEN", loi);
    throw loi;
  }
  try {
    const yeuCau: LamMoiRequest = { refreshToken };
    const res = await khachPhien.post<TokenResponse>("/auth/lam-moi", yeuCau);
    ghiCapToken(res.data);
    return res.data.accessToken;
  } catch (e) {
    // Mọi thất bại (401, 429, mạng, quá thời gian, 500) kết thúc phiên; giữ nguyên ApiError để
    // màn hình đăng nhập báo đúng nguyên nhân (429 vẫn có status/maLoi, 500 vẫn có traceId).
    const loi = await chuanHoaLoi(e);
    xoaPhien(loi.maLoi === "TAI_KHOAN_BI_KHOA" ? "BI_KHOA" : "HET_PHIEN", loi);
    throw loi;
  }
}

/**
 * Làm mới phiên, trả access token mới. Nhiều người gọi cùng lúc nhận CHUNG một Promise nên chỉ
 * một yêu cầu POST /auth/lam-moi được gửi (refresh token xoay vòng, dùng lại token cũ thì cả chuỗi
 * bị thu hồi — KT 7.3). Thất bại thì phiên đã bị xóa và Promise bị từ chối bằng ApiError; không
 * gọi lại tự động.
 */
export function lamMoiPhien(): Promise<string> {
  if (dangLamMoi === null) {
    dangLamMoi = thucHienLamMoi().finally(() => {
      dangLamMoi = null;
    });
  }
  return dangLamMoi;
}

/**
 * Khôi phục phiên khi mở tab hoặc tải lại trang (UF-CM-03). Gọi sau khi dựng trang (useEffect),
 * dùng chung Promise với interceptor nên Strict Mode chạy effect hai lần cũng chỉ làm mới một lần.
 *
 * @returns true khi đã có access token; false khi tab chưa có refresh token
 * @throws ApiError khi làm mới thất bại (phiên đã bị xóa)
 */
export async function khoiPhucPhien(): Promise<boolean> {
  if (layRefreshToken() === null) return false;
  await lamMoiPhien();
  return true;
}

/**
 * Đăng xuất: gửi refresh token để thu hồi, bỏ qua kết quả, luôn xóa phiên cục bộ (FA 3.5).
 */
export async function dangXuat(): Promise<void> {
  const refreshToken = layRefreshToken();
  try {
    if (refreshToken !== null) {
      const yeuCau: DangXuatRequest = { refreshToken };
      await khachPhien.post("/auth/dang-xuat", yeuCau);
    }
  } catch {
    // FA 3.5: dù đăng xuất thành công hay lỗi vẫn xóa phiên cục bộ; kết quả được bỏ qua có chủ đích.
  } finally {
    xoaPhien();
  }
}
