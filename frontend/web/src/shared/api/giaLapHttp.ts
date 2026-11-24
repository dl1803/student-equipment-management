// Giả lập HTTP cho kiểm thử (FC 9.1: thay bộ chuyển đổi của Axios, không thêm thư viện giả lập).
// Sở hữu: Thịnh (shared core). CHỈ DÙNG TRONG TỆP *.test.*.
// Đặt trong shared/api vì chỉ thư mục này được dùng axios (FC 4.1); tính năng import từ đây.
//
//   ganBoChuyenDoi({
//     api: vi.fn<BoChuyenDoi>((cfg) => traVe(cfg, 200, { ... })),
//     phien: vi.fn<BoChuyenDoi>((cfg) => traVe(cfg, 200, tokenMoi)),
//   });
import {
  AxiosError,
  type AxiosAdapter,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";
import { apiClient } from "./client";
import { khachPhien } from "./session/lamMoi";

export type CauHinhYeuCau = InternalAxiosRequestConfig;
export type PhanHoiHttp = AxiosResponse;
export type BoChuyenDoi = AxiosAdapter;

/** Phản hồi giả: 4xx/5xx bị từ chối bằng AxiosError có response, như bộ chuyển đổi thật. */
export function traVe(
  config: CauHinhYeuCau,
  status: number,
  data: unknown = {},
): Promise<PhanHoiHttp> {
  const res: PhanHoiHttp = {
    data,
    status,
    statusText: "",
    headers: {
      "content-type":
        status >= 400 ? "application/problem+json" : "application/json",
    },
    config,
  };
  if (status >= 400) {
    return Promise.reject(
      new AxiosError("loi", AxiosError.ERR_BAD_REQUEST, config, null, res),
    );
  }
  return Promise.resolve(res);
}

/** Mất kết nối (không có phản hồi). */
export function matKetNoi(config: CauHinhYeuCau): Promise<never> {
  return Promise.reject(
    new AxiosError("Network Error", AxiosError.ERR_NETWORK, config),
  );
}

/** Quá thời gian chờ. */
export function quaThoiGian(config: CauHinhYeuCau): Promise<never> {
  return Promise.reject(
    new AxiosError("timeout", AxiosError.ETIMEDOUT, config),
  );
}

/**
 * Gắn bộ chuyển đổi giả: `api` cho apiClient (yêu cầu nghiệp vụ, /auth/toi, đăng nhập),
 * `phien` cho client phiên (/auth/lam-moi, /auth/dang-xuat).
 */
export function ganBoChuyenDoi(boChuyenDoi: {
  api?: BoChuyenDoi;
  phien?: BoChuyenDoi;
}): void {
  if (boChuyenDoi.api !== undefined)
    apiClient.defaults.adapter = boChuyenDoi.api;
  if (boChuyenDoi.phien !== undefined) {
    khachPhien.defaults.adapter = boChuyenDoi.phien;
  }
}
