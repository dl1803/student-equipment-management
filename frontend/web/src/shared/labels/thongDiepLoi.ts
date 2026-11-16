// Thông điệp lỗi dùng chung (FC 5.5 [ĐX-FC-11]; UI-Screens 3.1–3.2; DS 9.4, 9.8). Sở hữu: Thịnh.
// Thông điệp riêng của màn hình nằm ở features/.../thongDiep.ts của tính năng đó.
// Không hiển thị maLoi hay detail làm thông điệp chính (DS 9.8).
import type { NhomLoi } from "@/shared/api/apiError";

export const THONG_DIEP_LOI_CHUNG = {
  PHIEN_HET_HAN: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại",
  KHONG_DU_QUYEN: "Bạn không có quyền thực hiện thao tác này",
  PHONG_CHUA_CAP_QUYEN: "Phòng này chưa được cấp quyền cho bạn",
  KHONG_TIM_THAY: "Không tìm thấy",
  CAN_TAI_LAI:
    "Dữ liệu vừa được thay đổi. Màn hình đã tải lại giá trị mới, vui lòng thực hiện lại",
  QUA_NHIEU_YEU_CAU:
    "Hệ thống đang nhận quá nhiều yêu cầu. Vui lòng chờ một lát rồi thử lại",
  MANG: "Không kết nối được tới hệ thống. Kiểm tra kết nối rồi thử lại",
  HE_THONG: "Đã xảy ra lỗi hệ thống.",
  CHUA_BIET_KET_QUA: "Chưa xác định được kết quả",
} as const;

/** Thông điệp mặc định theo nhóm lỗi, dùng khi màn hình không có thông điệp riêng. */
export function thongDiepTheoNhom(nhom: NhomLoi): string {
  switch (nhom) {
    case "PHIEN":
      return THONG_DIEP_LOI_CHUNG.PHIEN_HET_HAN;
    case "QUYEN":
      return THONG_DIEP_LOI_CHUNG.KHONG_DU_QUYEN;
    case "KHONG_TON_TAI":
      return THONG_DIEP_LOI_CHUNG.KHONG_TIM_THAY;
    case "CAN_TAI_LAI":
      return THONG_DIEP_LOI_CHUNG.CAN_TAI_LAI;
    case "TAN_SUAT":
      return THONG_DIEP_LOI_CHUNG.QUA_NHIEU_YEU_CAU;
    case "MANG":
      return THONG_DIEP_LOI_CHUNG.MANG;
    default:
      return THONG_DIEP_LOI_CHUNG.HE_THONG;
  }
}
