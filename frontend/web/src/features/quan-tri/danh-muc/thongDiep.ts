// Thông điệp riêng của SCR-QT-02..04 (UI-Screens 7.2–7.4; FC 5.5 [ĐX-FC-11]). Sở hữu: Thịnh.
export const THONG_DIEP_DANH_MUC = {
  PHONG_RONG: "Chưa có phòng thiết bị",
  LOAI_RONG: "Chưa có loại thiết bị",
  LOAI_RONG_BO_LOC: "Không có loại thiết bị phù hợp bộ lọc",
  /** UI-Screens 7.4 chỉ quy định thông điệp rỗng khi có bộ lọc. */
  THIET_BI_RONG_BO_LOC: "Không có thiết bị phù hợp bộ lọc",
  /** Chưa lọc gì mà rỗng: theo cùng mẫu "Chưa có …" của SCR-QT-02, QT-03. */
  THIET_BI_RONG: "Chưa có thiết bị",
  /** UF-QT-01 E1: lỗi đặt dưới ô Mã thiết bị (Wireframes SCR-QT-04). */
  MA_THIET_BI_TRUNG: "Mã thiết bị đã tồn tại. Nhập mã khác.",
  XUNG_DOT:
    "Dữ liệu vừa được người khác thay đổi. Biểu mẫu đã nạp giá trị mới nhất, vui lòng sửa lại rồi lưu.",
  XUNG_DOT_KHONG_NAP:
    "Dữ liệu vừa được người khác thay đổi. Danh sách đã được tải lại; kiểm tra giá trị hiện tại trước khi lưu lại.",
  CHUA_BIET_KET_QUA:
    "Chưa xác định được kết quả. Bấm Kiểm tra lại để tải danh sách mới nhất trước khi lưu lại.",
  KHONG_CON_TON_TAI:
    "Không tìm thấy. Đối tượng có thể đã bị thay đổi; danh sách đã được tải lại.",
  CHI_DUOC_NGUNG:
    "Loại này đã có lịch sử nên không xóa được; chỉ có thể ngưng sử dụng.",
  DA_NGUNG_KHONG_XOA:
    "Loại này đã có lịch sử nên không xóa được và đang ở trạng thái ngưng sử dụng.",
  /** 409 DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG của thiết bị (UF-QT-01 E2; BR-18). */
  THIET_BI_CHI_DUOC_NGUNG:
    "Thiết bị này đã có lịch sử hoặc còn đối tượng phụ thuộc nên không xóa được; chỉ có thể ngưng sử dụng.",
  THIET_BI_DA_NGUNG_KHONG_XOA:
    "Thiết bị này đã có lịch sử hoặc còn đối tượng phụ thuộc nên không xóa được và đang ở trạng thái ngưng sử dụng.",
  /** Ngưng sử dụng gặp 409 XUNG_DOT_CAP_NHAT: không có biểu mẫu để nạp lại, chỉ tải lại danh sách. */
  XUNG_DOT_NGUNG:
    "Dữ liệu vừa được người khác thay đổi. Danh sách đã được tải lại; kiểm tra thiết bị trước khi ngưng sử dụng lại.",
  /** BR-17: nêu trong hộp Cập nhật giá trị (UI-Screens 7.4). */
  GIA_TRI_DUNG_BOI_THUONG:
    "Giá trị hiện hành dùng để tính bồi thường. Biên bản đã lập không đổi; lịch sử giá trị được giữ lại.",
} as const;

export function daXoaLoai(tenLoai: string): string {
  return `Đã xóa loại thiết bị ${tenLoai}`;
}

export function daNgungLoai(tenLoai: string): string {
  return `Đã ngưng sử dụng loại thiết bị ${tenLoai}`;
}

export function daXoaThietBi(tenThietBi: string): string {
  return `Đã xóa thiết bị ${tenThietBi}`;
}

export function daNgungThietBi(tenThietBi: string): string {
  return `Đã ngưng sử dụng thiết bị ${tenThietBi}`;
}

/** giaTriDaDinhDang: giá trị máy chủ trả về, đã qua formatTien. */
export function daCapNhatGiaTri(
  tenThietBi: string,
  giaTriDaDinhDang: string,
): string {
  return `Đã cập nhật giá trị thiết bị ${tenThietBi}: ${giaTriDaDinhDang}`;
}
