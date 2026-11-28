// Thông điệp riêng của luồng Giao thiết bị (FC 5.5; UI-Screens 6.4; DS 11.1). Sở hữu: Thịnh.
export const THONG_DIEP_GIAO_THIET_BI = {
  KHONG_TIM_THAY_SV: "Không tìm thấy sinh viên",
  MA_THE_KHONG_HOP_LE: "Mã thẻ không hợp lệ",
} as const;

/** Nhãn bước của thanh tiến trình (DS 11.1, CMP-31). */
export const NHAN_BUOC = [
  "Thẻ SV",
  "Nguồn giao",
  "Thiết bị",
  "Phiếu mượn",
] as const;
