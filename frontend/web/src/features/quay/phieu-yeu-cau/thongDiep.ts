// Thông điệp riêng của SCR-NV-02, SCR-NV-03 (UI-Screens 6.2–6.3; FC 5.5 [ĐX-FC-11]). Sở hữu: Thịnh.
export const THONG_DIEP_PHIEU_YEU_CAU = {
  /** UI-Screens 6.2 trạng thái đặc thù. */
  RONG: "Không có phiếu yêu cầu chờ duyệt",
  /** Cùng câu với SCR-NV-01 khi NV chưa được cấp phòng nào. */
  CHUA_CAP_PHONG:
    "Bạn chưa được cấp quyền phòng thiết bị nào; liên hệ quản trị viên",
  /** UF-NV-01 E1: phiếu sang ngày khác chưa duyệt đã tự Hết hạn, không còn trong danh sách. */
  GHI_CHU_HET_HAN:
    "Phiếu chưa được duyệt trong ngày gửi sẽ tự hết hạn và không còn trong danh sách; bấm Làm mới để cập nhật.",
  /** 404 khi kiểm tra: phiếu không còn (đã xử lý hoặc đã hết hạn). */
  KHONG_CON_CHO_DUYET:
    "Phiếu yêu cầu không còn chờ duyệt (đã được xử lý hoặc đã hết hạn). Danh sách đã được tải lại.",
} as const;
