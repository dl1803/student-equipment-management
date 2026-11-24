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
  /** 404 khi kiểm tra, hoặc 404/TRANG_THAI_KHONG_HOP_LE khi duyệt, từ chối (E1). */
  KHONG_CON_CHO_DUYET:
    "Phiếu yêu cầu không còn chờ duyệt (đã được xử lý hoặc đã hết hạn). Danh sách đã được tải lại.",
  /** E3: 409 SO_LUONG_KHA_DUNG_THAY_DOI hoặc 422 điều kiện SV khi bấm Duyệt. */
  DIEU_KIEN_DA_DOI:
    "Không duyệt được: điều kiện của sinh viên hoặc số lượng khả dụng đã thay đổi. Kết quả kiểm tra đã được tải lại; xem lý do bên dưới hoặc chọn Từ chối.",
  /** 409 XUNG_DOT_CAP_NHAT: giao dịch khác vừa đổi dữ liệu. */
  XUNG_DOT:
    "Dữ liệu vừa được thay đổi. Kết quả kiểm tra đã được tải lại, vui lòng xem lại rồi thực hiện lại.",
  /** FA 3.11: thao tác ghi đã gửi nhưng mất phản hồi. */
  CHUA_BIET_KET_QUA:
    "Chưa xác định được kết quả. Phiếu có thể đã được xử lý; kiểm tra danh sách phiếu chờ duyệt trước khi thao tác lại.",
  /** UI-Screens 6.3 hộp xác nhận Duyệt (ĐX-UF-05). */
  HE_QUA_DUYET:
    "Phiếu chuyển Đã duyệt; hệ thống giữ chỗ từng loại thiết bị trong phiếu (mặc định 30 phút) và thông báo sinh viên.",
  /** UI-Screens 6.3 hộp xác nhận Từ chối. */
  HE_QUA_TU_CHOI: "Phiếu chuyển Từ chối và sinh viên được thông báo.",
  NHAP_LY_DO: "Nhập lý do từ chối",
} as const;

/** Lời báo để lại cho danh sách khi NV rời SCR-NV-03 vì chưa biết kết quả thao tác ghi. */
export function chuaBietKetQuaCua(
  hanhDong: "duyệt" | "từ chối",
  maYeuCau: string,
): string {
  return `Chưa xác định được kết quả ${hanhDong} phiếu ${maYeuCau}. Phiếu còn trong danh sách là chưa được xử lý; mở lại để thao tác.`;
}

export function daDuyet(maYeuCau: string): string {
  return `Đã duyệt phiếu ${maYeuCau}`;
}

export function daTuChoi(maYeuCau: string): string {
  return `Đã từ chối phiếu ${maYeuCau}`;
}
