// Khung chung cho biểu tượng SVG nhúng (FC 8.3 [ĐX-FC-05], DS Phụ lục A). Sở hữu: Thịnh.
// Nguồn đường vẽ: Material Symbols Outlined, độ đậm nét 400 (Apache License 2.0), chép thành SVG;
// không dùng thư viện hay phông biểu tượng. Chỉ thêm biểu tượng có trong DS Phụ lục A.
import type { ReactNode } from "react";
import { classNames } from "../classNames";

export type KichThuocIcon = "sm" | "md" | "lg";

export type IconProps = {
  /** size.icon.sm 16 / md 20 / lg 24. */
  kichThuoc?: KichThuocIcon;
  className?: string;
};

const LOP_KICH_THUOC: Record<KichThuocIcon, string> = {
  sm: "size-icon-sm",
  md: "size-icon-md",
  lg: "size-icon-lg",
};

export function SvgIcon({
  kichThuoc = "md",
  className,
  children,
}: IconProps & { children: ReactNode }) {
  return (
    <svg
      viewBox="0 -960 960 960"
      fill="currentColor"
      aria-hidden="true"
      focusable="false"
      className={classNames("shrink-0", LOP_KICH_THUOC[kichThuoc], className)}
    >
      {children}
    </svg>
  );
}
