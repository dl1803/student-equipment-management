// TextLink — CMP-03 Liên kết (DS 8.2). Đặt tên TextLink để không trùng next/link (FC 3.4).
// Sở hữu: Thịnh (shared core). Chỉ để chuyển màn hình; hành động thay đổi dữ liệu dùng Button.
import Link, { type LinkProps } from "next/link";
import type { ReactNode } from "react";
import { classNames } from "./classNames";

export type TextLinkProps = {
  href: LinkProps["href"];
  children: ReactNode;
  /** Mã phiếu, mã biên bản làm liên kết dùng type.code (DS CMP-03). */
  laMa?: boolean;
  className?: string;
};

export function TextLink({
  href,
  children,
  laMa = false,
  className,
}: TextLinkProps) {
  return (
    <Link
      href={href}
      className={classNames(
        "text-text-link underline hover:text-text-link-hover visited:text-text-link transition-colors",
        laMa && "type-code whitespace-nowrap",
        className,
      )}
    >
      {children}
    </Link>
  );
}
