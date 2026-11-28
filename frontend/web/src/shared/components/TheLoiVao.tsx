// CMP-15 Thẻ, biến thể Lối vào có biểu tượng và Số liệu (DS 8.4) cho trang chủ web. Sở hữu: Thịnh
// (shared core). Lối vào: biểu tượng trong ô nền color.surface.selected, tên chức năng, chevron; cả
// thẻ là liên kết.
import Link from "next/link";
import type { ComponentType } from "react";
import { IconChevronRight } from "./icons/IconChevronRight";
import type { IconProps } from "./icons/SvgIcon";

export function TheLoiVao({
  nhan,
  href,
  bieuTuong: BieuTuong,
}: {
  nhan: string;
  href: string;
  bieuTuong?: ComponentType<IconProps>;
}) {
  return (
    <Link
      href={href}
      className="group flex items-center gap-4 rounded-lg border border-border-subtle bg-surface-default p-4 transition-colors hover:border-border-control-hover hover:bg-surface-subtle"
    >
      {BieuTuong !== undefined ? (
        <span className="grid size-control-md shrink-0 place-items-center rounded-md bg-surface-selected text-text-link">
          <BieuTuong kichThuoc="lg" />
        </span>
      ) : null}
      <span className="flex-1 type-label text-text-primary">{nhan}</span>
      <IconChevronRight
        kichThuoc="md"
        className="text-text-secondary transition-transform group-hover:translate-x-1"
      />
    </Link>
  );
}

/** Thẻ số liệu: con số do máy chủ trả (tongSo), không tính ở client. */
export function TheSoLieu({
  nhan,
  giaTri,
  href,
  bieuTuong: BieuTuong,
  nhanManh = false,
}: {
  nhan: string;
  /** null: đang tải hoặc lỗi → hiển thị "—". */
  giaTri: number | null;
  href: string;
  bieuTuong: ComponentType<IconProps>;
  /** Dùng nền chanh cho thẻ cần chú ý nhất. */
  nhanManh?: boolean;
}) {
  return (
    <Link
      href={href}
      // Tên truy cập có ranh giới giữa nhãn và số; "—" đọc thành "chưa có số liệu" (review PR #79).
      aria-label={`${nhan}: ${giaTri === null ? "chưa có số liệu" : giaTri.toLocaleString("vi-VN")}`}
      className={
        nhanManh
          ? "flex flex-col gap-4 rounded-lg bg-nav-selected-bg p-6 text-nav-selected-text transition-colors hover:bg-action-primary-bg-hover"
          : "flex flex-col gap-4 rounded-lg bg-surface-subtle p-6 text-text-primary transition-colors hover:bg-surface-muted"
      }
    >
      <span className="flex items-center justify-between gap-2">
        <span className="type-label">{nhan}</span>
        <span
          className={
            nhanManh
              ? "grid size-control-sm place-items-center rounded-full bg-surface-default text-text-link"
              : "grid size-control-sm place-items-center rounded-full bg-surface-default text-text-secondary"
          }
        >
          <BieuTuong kichThuoc="md" />
        </span>
      </span>
      <span className="type-title-screen font-bold tabular-nums">
        {giaTri === null ? "—" : giaTri.toLocaleString("vi-VN")}
      </span>
    </Link>
  );
}
