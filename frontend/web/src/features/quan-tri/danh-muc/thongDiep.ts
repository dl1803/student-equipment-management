// Thông điệp riêng của SCR-QT-02, QT-03 (UI-Screens 7.2–7.3; FC 5.5 [ĐX-FC-11]). Sở hữu: Thịnh.
export const THONG_DIEP_DANH_MUC = {
  PHONG_RONG: "Chưa có phòng thiết bị",
  LOAI_RONG: "Chưa có loại thiết bị",
  LOAI_RONG_BO_LOC: "Không có loại thiết bị phù hợp bộ lọc",
  XUNG_DOT:
    "Dữ liệu vừa được người khác thay đổi. Biểu mẫu đã nạp giá trị mới nhất, vui lòng sửa lại rồi lưu.",
  CHUA_BIET_KET_QUA:
    "Chưa xác định được kết quả. Bấm Kiểm tra lại để tải danh sách mới nhất trước khi lưu lại.",
  KHONG_CON_TON_TAI:
    "Không tìm thấy. Đối tượng có thể đã bị thay đổi; danh sách đã được tải lại.",
  CHI_DUOC_NGUNG:
    "Loại này đã có lịch sử nên không xóa được; chỉ có thể ngưng sử dụng.",
  DA_NGUNG_KHONG_XOA:
    "Loại này đã có lịch sử nên không xóa được và đang ở trạng thái ngưng sử dụng.",
} as const;

export function daXoaLoai(tenLoai: string): string {
  return `Đã xóa loại thiết bị ${tenLoai}`;
}

export function daNgungLoai(tenLoai: string): string {
  return `Đã ngưng sử dụng loại thiết bị ${tenLoai}`;
}
