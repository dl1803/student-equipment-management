// Hằng số ảnh (FC 7.1 [ĐX-FC-03], FC 5.9; UI-Screens 3.6; FA 4.10). Sở hữu: Thịnh (shared core).
// Mỗi hằng số có đúng một nơi định nghĩa; muốn đổi giá trị phải đổi tài liệu nguồn trước.

/** Tối đa 5 ảnh cho mỗi thiết bị mỗi lần (UI-Screens 3.6). */
export const SO_ANH_TOI_DA = 5;

/** Tối đa 2 MB mỗi ảnh sau nén: 2 × 1024 × 1024 = 2.097.152 byte (UI-Screens 3.6, ck_anh_kich_thuoc). */
export const DUNG_LUONG_ANH_TOI_DA_BYTE = 2 * 1024 * 1024;

/** Cạnh dài tối đa sau nén; ảnh nhỏ hơn không bị phóng to (UI-Screens 3.6, FA 4.10). */
export const CANH_DAI_TOI_DA_PX = 1280;

/** Chất lượng JPEG thử lần lượt cho tới khi không vượt dung lượng tối đa (FA 4.10). */
export const CHAT_LUONG_NEN: ReadonlyArray<number> = [0.8, 0.7, 0.6];

/** Định dạng ảnh chấp nhận khi chọn tệp (UI-Screens 3.6). */
export const KIEU_ANH_CHAP_NHAN: ReadonlyArray<string> = [
  "image/jpeg",
  "image/png",
  "image/webp",
];

/**
 * Đuôi tệp tương ứng KIEU_ANH_CHAP_NHAN, chỉ dùng khi trình duyệt không cho biết kiểu tệp
 * (`File.type` rỗng do cấu hình MIME của máy). Nội dung thật vẫn được kiểm khi giải mã ở nenAnh và
 * bằng chữ ký tệp ở máy chủ.
 */
export const DUOI_ANH_CHAP_NHAN: ReadonlyArray<string> = [
  ".jpg",
  ".jpeg",
  ".png",
  ".webp",
];

/** Kiểu tệp gửi lên sau nén: luôn JPEG (FA 4.10 bước 3). */
export const KIEU_ANH_SAU_NEN = "image/jpeg";
