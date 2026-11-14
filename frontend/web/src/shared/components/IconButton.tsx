// IconButton — CMP-02 Nút biểu tượng (DS 8.2). Sở hữu: Thịnh (shared core).
// Chỉ cho hành động quen thuộc (Quay lại, Đóng, mở rộng dòng, Gỡ ảnh, xóa nội dung);
// hành động khác dùng Button có chữ.
import type { ButtonHTMLAttributes, ReactNode } from "react";
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
};

export function IconButton({
  nhanTruyCap,
  bieuTuong,
  type = "button",
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
        "inline-flex size-control-md items-center justify-center rounded-md bg-transparent text-text-secondary transition-colors cursor-pointer",
        "hover:bg-action-secondary-bg-hover active:bg-action-secondary-bg-pressed",
        "disabled:cursor-not-allowed disabled:text-text-disabled disabled:bg-transparent",
        className,
      )}
    >
      {bieuTuong}
    </button>
  );
}
