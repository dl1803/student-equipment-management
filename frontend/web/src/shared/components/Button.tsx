// Button — CMP-01 Nút (DS 8.2, mục 6). Sở hữu: Thịnh (shared core).
import type { ButtonHTMLAttributes, ReactNode } from "react";
import { ChiBaoQuay } from "./ChiBaoQuay";
import { classNames } from "./classNames";

/** primary: tối đa một nút mỗi vùng; destructive: xóa, ngừng, khóa, hủy, từ chối (DS 9.2). */
export type BienTheButton =
  "primary" | "secondary" | "tertiary" | "destructive";
/** sm: dòng bảng, bộ lọc dày; md: mặc định; lg: thao tác chính ở quầy (ĐX-DS-08). */
export type KichThuocButton = "sm" | "md" | "lg";

export type ButtonProps = Omit<
  ButtonHTMLAttributes<HTMLButtonElement>,
  "type" | "children"
> & {
  /** Nhãn là động từ theo DS 9.8; không dùng "OK", "Đồng ý". */
  children: ReactNode;
  bienThe?: BienTheButton;
  kichThuoc?: KichThuocButton;
  /** Biểu tượng bên trái, size.icon.md (ví dụ IconRefresh cho Làm mới). */
  bieuTuong?: ReactNode;
  /** Đang chờ phản hồi: vô hiệu, chữ đổi thành "Đang xử lý" kèm chỉ báo quay (DS mục 6). */
  dangXuLy?: boolean;
  type?: "button" | "submit" | "reset";
};

const LOP_BIEN_THE: Record<BienTheButton, string> = {
  primary:
    "bg-action-primary-bg text-action-primary-text hover:bg-action-primary-bg-hover active:bg-action-primary-bg-pressed",
  destructive:
    "bg-action-destructive-bg text-action-destructive-text hover:bg-action-destructive-bg-hover active:bg-action-destructive-bg-pressed",
  secondary:
    "bg-action-secondary-bg text-action-secondary-text border-action-secondary-border hover:bg-action-secondary-bg-hover active:bg-action-secondary-bg-pressed",
  tertiary:
    "bg-transparent text-action-tertiary-text hover:bg-action-tertiary-bg-hover active:bg-action-tertiary-bg-pressed",
};

const LOP_KICH_THUOC: Record<KichThuocButton, string> = {
  sm: "h-control-sm",
  md: "h-control-md",
  lg: "h-control-lg",
};

export function Button({
  children,
  bienThe = "secondary",
  kichThuoc = "md",
  bieuTuong,
  dangXuLy = false,
  disabled = false,
  type = "button",
  className,
  ...conLai
}: ButtonProps) {
  return (
    <button
      {...conLai}
      type={type}
      disabled={disabled || dangXuLy}
      aria-busy={dangXuLy || undefined}
      data-bien-the={bienThe}
      className={classNames(
        "inline-flex items-center justify-center gap-2 rounded-md border border-transparent px-4 type-label whitespace-nowrap transition-colors cursor-pointer",
        LOP_BIEN_THE[bienThe],
        LOP_KICH_THUOC[kichThuoc],
        "disabled:cursor-not-allowed disabled:bg-action-disabled-bg disabled:text-action-disabled-text disabled:border-action-disabled-border",
        className,
      )}
    >
      {dangXuLy ? <ChiBaoQuay kichThuoc="md" /> : bieuTuong}
      <span>{dangXuLy ? "Đang xử lý" : children}</span>
    </button>
  );
}
