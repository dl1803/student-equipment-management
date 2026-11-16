// Nhãn vai trò (DS 7.1): hiển thị bằng chữ thường, không dùng StatusBadge. Sở hữu: Thịnh.
const NHAN_VAI_TRO: Record<string, string> = {
  SINH_VIEN: "Sinh viên",
  NHAN_VIEN: "Nhân viên quản lý thiết bị",
  QUAN_TRI: "Quản trị viên",
};

/** Mã vai trò → chữ hiển thị; mã lạ trả nguyên mã (DS 7.1). */
export function nhanVaiTro(ma: string): string {
  return Object.prototype.hasOwnProperty.call(NHAN_VAI_TRO, ma)
    ? (NHAN_VAI_TRO[ma] ?? ma)
    : ma;
}
