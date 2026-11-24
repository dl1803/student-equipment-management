// Hợp đồng lỗi chuẩn hóa dùng chung (FA 3.3, FC 5.5, 7.4). Sở hữu: Thịnh (shared core).
// Mọi lỗi ra khỏi shared/api/ phải là ApiError; tầng trên không đọc phản hồi Axios thô.

/** HTTP: có phản hồi; MANG: mất kết nối; QUA_THOI_GIAN: quá 90 giây; HUY: yêu cầu bị hủy chủ động. */
export type LoaiLoi = "HTTP" | "MANG" | "QUA_THOI_GIAN" | "HUY";

export type ApiError = {
  loai: LoaiLoi;
  status?: number;
  /** Là string, không phải union MaLoi, để mã lạ từ máy chủ không làm vỡ client (FC 5.1). */
  maLoi?: string;
  /** Chỉ để ghi nhận; không phân nhánh theo detail (ĐX-CV-02). */
  detail?: string;
  traceId?: string;
  /** Lỗi theo trường của ValidationProblemDetails, khóa dạng `dsLoaiVaSoLuong[0].soLuong`. */
  errors?: Record<string, string[]>;
};

/** Nhóm xử lý ở tầng chung (FA 3.4, FC 5.5). Bảng ánh xạ cài ở shared/api/nhomLoi.ts (task sau). */
export type NhomLoi =
  | "PHIEN"
  | "DANG_NHAP_SAI"
  | "KHOA_TAI_KHOAN"
  | "QUYEN"
  | "DU_LIEU_NHAP"
  | "KHONG_TON_TAI"
  | "CAN_TAI_LAI"
  | "TAN_SUAT"
  | "NGHIEP_VU_THEO_MAN_HINH"
  | "HE_THONG"
  | "MANG";

const LOAI_LOI: readonly string[] = ["HTTP", "MANG", "QUA_THOI_GIAN", "HUY"];

export function laApiError(e: unknown): e is ApiError {
  if (typeof e !== "object" || e === null || !("loai" in e)) return false;
  return typeof e.loai === "string" && LOAI_LOI.includes(e.loai);
}

/** Lỗi sau khi thao tác ghi đã phát mà không biết máy chủ đã xử lý hay chưa (FA 3.11). */
export function laChuaBietKetQua(e: ApiError): boolean {
  return e.loai === "MANG" || e.loai === "QUA_THOI_GIAN" || e.loai === "HUY";
}
