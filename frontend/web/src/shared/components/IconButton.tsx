// IconButton — CMP-02 Nút biểu tượng (DS 8.2). Sở hữu: Thịnh (shared core).
// Chỉ cho hành động quen thuộc (Quay lại, Đóng, mở rộng dòng, Gỡ ảnh, xóa nội dung);
// hành động khác dùng Button có chữ.
import type { ButtonHTMLAttributes, ReactNode, Ref } from "react";
import { classNames } from "./classNames";

export type IconButtonProps = Omit<
  ButtonHTMLAttributes<HTMLButtonElement>,
  "type" | "children" | "aria-label"
> & {
  /** Tên truy cập tiếng Việt, bắt buộc (DS CMP-02, FC 5.8), ví dụ "Đóng", "Gỡ ảnh". */
  nhanTruyCap: string;
  /** Biểu tượng từ shared/components/icons, kích thước lg (hoặc md khi nằm trong ô nhập). */
  bieuTuong: ReactNode;
  type?: "button" | "submit" | "reset";
  /** md 40px mặc định; sm 32px cho chỗ chật như góc ô ảnh 96px của CMP-12 (ĐX-DS-08). */
  kichThuoc?: "sm" | "md";
  /** React 19 nhận ref như prop thường; dùng để đặt tiêu điểm (ví dụ nút Đóng của ImageViewer). */
  ref?: Ref<HTMLButtonElement>;
};

export function IconButton({
  nhanTruyCap,
  bieuTuong,
  type = "button",
  kichThuoc = "md",
  className,
  ...conLai
}: IconButtonProps) {
  if (nhanTruyCap.trim() === "") {
    throw new Error("IconButton cần nhanTruyCap không rỗng (DS CMP-02)");
  }
  return (
    <button
      {...conLai}
      type={type}
      aria-label={nhanTruyCap}
      className={classNames(
        "inline-flex items-center justify-center rounded-md bg-transparent text-text-secondary transition-colors cursor-pointer",
        "hover:bg-action-secondary-bg-hover active:bg-action-secondary-bg-pressed",
        "disabled:cursor-not-allowed disabled:text-text-disabled disabled:bg-transparent",
        kichThuoc === "sm" ? "size-control-sm" : "size-control-md",
        className,
      )}
    >
      {bieuTuong}
    </button>
  );
}
