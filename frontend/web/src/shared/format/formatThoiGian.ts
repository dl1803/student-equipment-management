// Định dạng thời gian (FA 3.7, FC 7.2 [ĐX-FC-12], DS 9.7). Sở hữu: Thịnh (shared core).
// Chỉ định dạng giá trị máy chủ trả về; không tính toán hạn, số ngày trễ (FA-07).

export const MUI_GIO_VN = "Asia/Ho_Chi_Minh";
/** Việt Nam không áp dụng giờ mùa hè nên độ lệch cố định (ĐX-FA-10). */
export const DO_LECH_GIO_VN = "+07:00";
/** Giá trị thiếu (ĐX-DS-12). */
export const GIA_TRI_THIEU = "—";

// Tạo một lần ở mức module; ghép phần thủ công để không phụ thuộc chuỗi mặc định của locale.
const BO_DINH_DANG_NGAY_GIO = new Intl.DateTimeFormat("vi-VN", {
  timeZone: MUI_GIO_VN,
  hourCycle: "h23",
  year: "numeric",
  month: "2-digit",
  day: "2-digit",
  hour: "2-digit",
  minute: "2-digit",
});

/** Thời điểm `date-time` (UTC từ API) → `dd/MM/yyyy HH:mm` giờ Việt Nam. Chuỗi không đọc được trả nguyên văn. */
export function formatNgayGio(iso: string | null | undefined): string {
  if (iso == null || iso === "") return GIA_TRI_THIEU;
  const thoiDiem = new Date(iso);
  if (Number.isNaN(thoiDiem.getTime())) return iso;
  const phan: Partial<Record<Intl.DateTimeFormatPartTypes, string>> = {};
  for (const { type, value } of BO_DINH_DANG_NGAY_GIO.formatToParts(thoiDiem)) {
    phan[type] = value;
  }
  return `${phan.day}/${phan.month}/${phan.year} ${phan.hour}:${phan.minute}`;
}

const MAU_NGAY = /^(\d{4})-(\d{2})-(\d{2})$/;

/** Ngày `date` (`yyyy-MM-dd`) → `dd/MM/yyyy`; tách chuỗi, không tạo Date để không lệch múi giờ. */
export function formatNgay(chuoi: string | null | undefined): string {
  if (chuoi == null || chuoi === "") return GIA_TRI_THIEU;
  const khop = MAU_NGAY.exec(chuoi);
  if (khop === null) return chuoi;
  return `${khop[3]}/${khop[2]}/${khop[1]}`;
}

const MAU_GIO = /^\d{2}:\d{2}(:\d{2})?$/;

/** Giờ quầy `HH:mm[:ss]` (đã là giờ Việt Nam) → `HH:mm`; không đổi múi giờ. */
export function formatGio(chuoi: string | null | undefined): string {
  if (chuoi == null || chuoi === "") return GIA_TRI_THIEU;
  if (!MAU_GIO.test(chuoi)) return chuoi;
  return chuoi.slice(0, 5);
}

const MAU_NGAY_GIO_NHAP = /^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2})(:\d{2})?$/;

/**
 * Giá trị ô `datetime-local` (hiểu là giờ Việt Nam) → chuỗi ISO có độ lệch +07:00 để gửi API
 * (FA 4.8, ĐX-FA-10). Giá trị không đúng dạng trả `null` để biểu mẫu báo lỗi trường.
 */
export function thanhChuoiGioVn(giaTri: string): string | null {
  const khop = MAU_NGAY_GIO_NHAP.exec(giaTri);
  if (khop === null) return null;
  return `${khop[1]}${khop[2] ?? ":00"}${DO_LECH_GIO_VN}`;
}
