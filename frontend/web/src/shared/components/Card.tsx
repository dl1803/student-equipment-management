// Card — CMP-15 Thẻ (DS 8.4). Sở hữu: Thịnh (shared core).
// Bốn biến thể: Tĩnh, Tương tác (cả thẻ là liên kết; lối vào chức năng ở trang chủ),
// Chọn được (một trong nhiều; nhóm nút chọn), Tông (thẻ quyền mượn). Không đổ bóng.
// Kiểu (ĐX-DS-17): nền color.surface.subtle, bo radius.lg, đệm space.6; thẻ tĩnh và tương tác không
// có viền nhìn thấy (viền trong suốt giữ kích thước khi chuyển sang trạng thái được chọn).
import Link from "next/link";
import type { ComponentType, ReactNode } from "react";
import { classNames } from "./classNames";
import { IconCheckCircle } from "./icons/IconCheckCircle";
import { IconChevronRight } from "./icons/IconChevronRight";
import { IconError } from "./icons/IconError";
import { IconInfo } from "./icons/IconInfo";
import { IconWarning } from "./icons/IconWarning";
import type { IconProps } from "./icons/SvgIcon";
import type { Tong } from "@/shared/labels/trangThai";

type TongThe = Exclude<Tong, "neutral">;

export type CardProps =
  | { bienThe: "TINH"; tieuDe?: string; children: ReactNode }
  | {
      /** Cả thẻ bấm được, có chevron_right khi chuyển màn hình. */
      bienThe: "TUONG_TAC";
      tieuDe: string;
      href: string;
      children?: ReactNode;
    }
  | {
      /** Chọn một trong nhiều; đặt trong phần tử role="radiogroup" có tên nhóm. */
      bienThe: "CHON_DUOC";
      tieuDe: string;
      duocChon: boolean;
      onChon: () => void;
      children?: ReactNode;
    }
  | { bienThe: "TONG"; tong: TongThe; tieuDe?: string; children: ReactNode };

const KHUNG = "flex flex-col gap-2 rounded-lg border p-6";

const LOP_TONG: Record<TongThe, string> = {
  info: "bg-tone-info-bg border-tone-info-border",
  success: "bg-tone-success-bg border-tone-success-border",
  warning: "bg-tone-warning-bg border-tone-warning-border",
  danger: "bg-tone-danger-bg border-tone-danger-border",
};

const ICON_TONG: Record<TongThe, ComponentType<IconProps>> = {
  info: IconInfo,
  success: IconCheckCircle,
  warning: IconWarning,
  danger: IconError,
};

const LOP_ICON_TONG: Record<TongThe, string> = {
  info: "text-tone-info-icon",
  success: "text-tone-success-icon",
  warning: "text-tone-warning-icon",
  danger: "text-tone-danger-icon",
};

function TieuDe({ children }: { children: ReactNode }) {
  return <span className="type-title-card text-text-primary">{children}</span>;
}

export function Card(props: CardProps) {
  switch (props.bienThe) {
    case "TINH":
      return (
        <div
          className={classNames(KHUNG, "border-transparent bg-surface-subtle")}
        >
          {props.tieuDe !== undefined ? <TieuDe>{props.tieuDe}</TieuDe> : null}
          {props.children}
        </div>
      );
    case "TUONG_TAC":
      return (
        <Link
          href={props.href}
          className={classNames(
            KHUNG,
            "border-transparent bg-surface-subtle transition-colors hover:border-border-control-hover active:bg-surface-muted",
          )}
        >
          <span className="flex items-center justify-between gap-2">
            <TieuDe>{props.tieuDe}</TieuDe>
            <IconChevronRight kichThuoc="md" className="text-text-secondary" />
          </span>
          {props.children}
        </Link>
      );
    case "CHON_DUOC":
      return (
        <button
          type="button"
          role="radio"
          aria-checked={props.duocChon}
          onClick={props.onChon}
          className={classNames(
            KHUNG,
            // Cả hai trạng thái cùng viền border.width.strong để chọn/bỏ chọn không xê dịch nội dung.
            "w-full cursor-pointer border-[length:var(--ds-border-width-strong)] text-left transition-colors",
            props.duocChon
              ? "border-border-selected bg-surface-selected"
              : "border-transparent bg-surface-subtle hover:border-border-control-hover",
          )}
        >
          <span className="flex items-center justify-between gap-2">
            <TieuDe>{props.tieuDe}</TieuDe>
            {props.duocChon ? (
              <IconCheckCircle
                kichThuoc="md"
                className="text-border-selected"
              />
            ) : null}
          </span>
          {props.children}
        </button>
      );
    case "TONG": {
      const Icon = ICON_TONG[props.tong];
      return (
        <div className={classNames(KHUNG, LOP_TONG[props.tong])}>
          <span className="flex items-center gap-2">
            <Icon kichThuoc="md" className={LOP_ICON_TONG[props.tong]} />
            {props.tieuDe !== undefined ? (
              <TieuDe>{props.tieuDe}</TieuDe>
            ) : null}
          </span>
          {props.children}
        </div>
      );
    }
    default: {
      const chuaXuLy: never = props;
      return chuaXuLy;
    }
  }
}
