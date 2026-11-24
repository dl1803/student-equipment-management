// Phân nhóm maLoi ở tầng chung (FA 3.4, FC 5.5, 7.4). Sở hữu: Thịnh (shared core).
// Một nơi duy nhất cài bảng FA 3.4; màn hình không tự lặp lại bảng này.
import type { ApiError, NhomLoi } from "./apiError";
import type { MaLoi } from "./types";

// Record<MaLoi, NhomLoi>: openapi.yaml thêm mã mà bảng chưa có thì tsc báo lỗi khi sinh lại kiểu.
const BANG_NHOM_LOI: Record<MaLoi, NhomLoi> = {
  CHUA_XAC_THUC: "PHIEN",
  DANG_NHAP_SAI: "DANG_NHAP_SAI",
  TAI_KHOAN_BI_KHOA: "KHOA_TAI_KHOAN",
  KHONG_DU_QUYEN: "QUYEN",
  PHONG_CHUA_CAP_QUYEN: "QUYEN",
  DU_LIEU_KHONG_HOP_LE: "DU_LIEU_NHAP",
  KHONG_TIM_THAY: "KHONG_TON_TAI",
  XUNG_DOT_CAP_NHAT: "CAN_TAI_LAI",
  TRANG_THAI_KHONG_HOP_LE: "CAN_TAI_LAI",
  QUA_NHIEU_YEU_CAU: "TAN_SUAT",
  SV_THE_HET_HAN: "NGHIEP_VU_THEO_MAN_HINH",
  SV_KHONG_CON_THEO_HOC: "NGHIEP_VU_THEO_MAN_HINH",
  SV_BI_KHOA_QUYEN_MUON: "NGHIEP_VU_THEO_MAN_HINH",
  VUOT_GIOI_HAN_THIET_BI: "NGHIEP_VU_THEO_MAN_HINH",
  VUOT_THOI_GIAN_MUON_TOI_DA: "NGHIEP_VU_THEO_MAN_HINH",
  CON_HANG_KHONG_DANG_KY_CHO: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_KHONG_SAN_SANG: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_SAI_PHONG: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_SAI_LOAI: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_KHONG_THUOC_PHIEU: "NGHIEP_VU_THEO_MAN_HINH",
  THIEU_MO_TA_HOAC_KHUNG_PHAT: "NGHIEP_VU_THEO_MAN_HINH",
  GIA_TRI_CAU_HINH_KHONG_HOP_LE: "NGHIEP_VU_THEO_MAN_HINH",
  GIAO_SAU_GIO_DONG_CUA: "NGHIEP_VU_THEO_MAN_HINH",
  SO_LUONG_KHA_DUNG_THAY_DOI: "NGHIEP_VU_THEO_MAN_HINH",
  GIU_CHO_HET_HAN: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_DA_DUOC_GIAO: "NGHIEP_VU_THEO_MAN_HINH",
  MA_THIET_BI_TRUNG: "NGHIEP_VU_THEO_MAN_HINH",
  TEN_DANG_NHAP_TRUNG: "NGHIEP_VU_THEO_MAN_HINH",
  MA_NV_TRUNG: "NGHIEP_VU_THEO_MAN_HINH",
  KHUNG_PHAT_DA_DUNG_KHONG_XOA: "NGHIEP_VU_THEO_MAN_HINH",
  CHUA_CAU_HINH_GIO_QUAY: "NGHIEP_VU_THEO_MAN_HINH",
  DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG: "NGHIEP_VU_THEO_MAN_HINH",
};

function laMaLoiDaBiet(maLoi: string): maLoi is MaLoi {
  return Object.prototype.hasOwnProperty.call(BANG_NHOM_LOI, maLoi);
}

/**
 * Nhóm xử lý của một lỗi. Người gọi xử lý HUY và nhánh "chưa biết kết quả" của thao tác ghi
 * (laChuaBietKetQua) TRƯỚC khi phân nhóm (FC 5.5); ở đây mọi lỗi không có phản hồi HTTP thuộc MANG.
 * Mã lạ thuộc NGHIEP_VU_THEO_MAN_HINH; phản hồi không có maLoi (500, thân không phải
 * problem+json) thuộc HE_THONG (FA 3.3 quy tắc 1, FA 3.4).
 */
export function nhomCuaLoi(loi: ApiError): NhomLoi {
  if (loi.loai !== "HTTP") return "MANG";
  if (loi.maLoi === undefined) return "HE_THONG";
  return laMaLoiDaBiet(loi.maLoi)
    ? BANG_NHOM_LOI[loi.maLoi]
    : "NGHIEP_VU_THEO_MAN_HINH";
}
