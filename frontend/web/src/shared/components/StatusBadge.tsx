// StatusBadge — CMP-13 Nhãn trạng thái (DS 7, 8.4; FC 7.3). Sở hữu: Thịnh (shared core).
// Thành phần DUY NHẤT hiển thị trạng thái nghiệp vụ. Chỉ nhận (họ, mã API) hoặc khóa nhãn phát sinh;
// không nhận màu hay chữ tùy ý. Không tương tác, không nhận tiêu điểm.
import type { ComponentType } from "react";
import { classNames } from "./classNames";
import { IconCheckCircle } from "./icons/IconCheckCircle";
import { IconError } from "./icons/IconError";
import { IconInfo } from "./icons/IconInfo";
import { IconWarning } from "./icons/IconWarning";
import type { IconProps } from "./icons/SvgIcon";
import {
  thongTinNhanPhatSinh,
  thongTinTrangThai,
  type HoTrangThai,
  type NhanPhatSinh,
  type Tong,
} from "@/shared/labels/trangThai";

export type StatusBadgeProps =
  | { ho: HoTrangThai; ma: string; nhanPhatSinh?: never }
  | { nhanPhatSinh: NhanPhatSinh; ho?: never; ma?: never };

// Lớp viết đầy đủ để Tailwind quét được.
const LOP_NHAT: Record<Tong, string> = {
  info: "bg-tone-info-bg border-tone-info-border text-tone-info-text",
  success:
    "bg-tone-success-bg border-tone-success-border text-tone-success-text",
  warning:
    "bg-tone-warning-bg border-tone-warning-border text-tone-warning-text",
  danger: "bg-tone-danger-bg border-tone-danger-border text-tone-danger-text",
  neutral:
    "bg-tone-neutral-bg border-tone-neutral-border text-tone-neutral-text",
};

const LOP_DAM: Record<Tong, string> = {
  info: "bg-tone-info-solid border-tone-info-solid text-tone-info-on-solid",
  success:
    "bg-tone-success-solid border-tone-success-solid text-tone-success-on-solid",
  warning:
    "bg-tone-warning-solid border-tone-warning-solid text-tone-warning-on-solid",
  danger:
    "bg-tone-danger-solid border-tone-danger-solid text-tone-danger-on-solid",
  neutral:
    "bg-tone-neutral-solid border-tone-neutral-solid text-tone-neutral-on-solid",
};

const LOP_ICON_NHAT: Record<Exclude<Tong, "neutral">, string> = {
  info: "text-tone-info-icon",
  success: "text-tone-success-icon",
  warning: "text-tone-warning-icon",
  danger: "text-tone-danger-icon",
};

/** Biểu tượng theo tông (DS Phụ lục A); neutral không có biểu tượng. */
const ICON_THEO_TONG: Record<
  Exclude<Tong, "neutral">,
  ComponentType<IconProps>
> = {
  info: IconInfo,
  success: IconCheckCircle,
  warning: IconWarning,
  danger: IconError,
};

export function StatusBadge(props: StatusBadgeProps) {
  const thongTin =
    props.nhanPhatSinh !== undefined
      ? thongTinNhanPhatSinh(props.nhanPhatSinh)
      : thongTinTrangThai(props.ho, props.ma);
  const { nhan, tong, nhanDam, laMaChuaBiet } = thongTin;
  const Icon = tong === "neutral" ? null : ICON_THEO_TONG[tong];

  return (
    <span
      data-tong={tong}
      data-nhan-dam={nhanDam}
      className={classNames(
        // border-solid luôn dày border.width.default; mức đậm dùng màu viền trùng nền (không thấy viền).
        "inline-flex min-h-badge-height items-center gap-1 rounded-full border px-2 type-label-sm",
        nhanDam ? LOP_DAM[tong] : LOP_NHAT[tong],
      )}
    >
      {Icon !== null && tong !== "neutral" ? (
        <Icon
          kichThuoc="sm"
          className={nhanDam ? undefined : LOP_ICON_NHAT[tong]}
        />
      ) : null}
      <span className={laMaChuaBiet ? "type-code" : undefined}>{nhan}</span>
    </span>
  );
}
