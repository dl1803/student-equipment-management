// Banner — CMP-24 Khối thông báo (DS 8.5, 9.4; FC 5.5, 5.8). Sở hữu: Thịnh (shared core).
// Đầu màn hình hoặc đầu một khu vực; không tự đóng, không có nút đóng, không dùng toast (NT-07).
// danger và warning là thông báo khẩn (role="alert"); info và success là thông báo thường.
import type { ComponentType, ReactNode } from "react";
import { Button } from "./Button";
import { classNames } from "./classNames";
import { IconCheckCircle } from "./icons/IconCheckCircle";
import { IconError } from "./icons/IconError";
import { IconInfo } from "./icons/IconInfo";
import { IconWarning } from "./icons/IconWarning";
import type { IconProps } from "./icons/SvgIcon";
import {
  thongTinNhanPhatSinh,
  type NhanPhatSinh,
  type Tong,
} from "@/shared/labels/trangThai";

/** neutral không dùng cho khối thông báo (DS CMP-24). */
export type TongBanner = Exclude<Tong, "neutral">;

export type BannerProps = {
  tong: TongBanner;
  tieuDe?: string;
  children?: ReactNode;
  /** Danh sách lý do, mỗi lý do một dòng (biến thể Kết luận dùng thongDiep của API). */
  dsLyDo?: ReadonlyArray<string>;
  /** Biến thể Kết luận: tiêu đề là nhãn phát sinh Hợp lệ/Không hợp lệ/Được duyệt/Không được duyệt (DS 7.3). */
  ketLuan?: Extract<
    NhanPhatSinh,
    "HOP_LE" | "KHONG_HOP_LE" | "DUOC_DUYET" | "KHONG_DUOC_DUYET"
  >;
  hanhDong?: { nhan: string; onClick: () => void };
  /** Mã theo dõi khi lỗi hệ thống lúc thao tác (DS 9.4). */
  traceId?: string;
};

const LOP_TONG: Record<TongBanner, string> = {
  info: "bg-tone-info-bg border-tone-info-border",
  success: "bg-tone-success-bg border-tone-success-border",
  warning: "bg-tone-warning-bg border-tone-warning-border",
  danger: "bg-tone-danger-bg border-tone-danger-border",
};

const LOP_CHU_TONG: Record<TongBanner, string> = {
  info: "text-tone-info-text",
  success: "text-tone-success-text",
  warning: "text-tone-warning-text",
  danger: "text-tone-danger-text",
};

const LOP_ICON: Record<TongBanner, string> = {
  info: "text-tone-info-icon",
  success: "text-tone-success-icon",
  warning: "text-tone-warning-icon",
  danger: "text-tone-danger-icon",
};

const ICON: Record<TongBanner, ComponentType<IconProps>> = {
  info: IconInfo,
  success: IconCheckCircle,
  warning: IconWarning,
  danger: IconError,
};

export function Banner({
  tong,
  tieuDe,
  children,
  dsLyDo,
  ketLuan,
  hanhDong,
  traceId,
}: BannerProps) {
  const Icon = ICON[tong];
  const tieuDeHienThi =
    ketLuan !== undefined ? thongTinNhanPhatSinh(ketLuan).nhan : tieuDe;
  const khan = tong === "danger" || tong === "warning";

  return (
    <div
      role={khan ? "alert" : "status"}
      data-tong={tong}
      className={classNames(
        "flex items-start gap-3 rounded-lg border p-card",
        LOP_TONG[tong],
      )}
    >
      <Icon kichThuoc="md" className={LOP_ICON[tong]} />
      <div className="flex min-w-0 flex-1 flex-col gap-1">
        {tieuDeHienThi !== undefined ? (
          <p className={classNames("type-title-card", LOP_CHU_TONG[tong])}>
            {tieuDeHienThi}
          </p>
        ) : null}
        {children !== undefined ? (
          <div className="type-body text-text-primary">{children}</div>
        ) : null}
        {dsLyDo !== undefined && dsLyDo.length > 0 ? (
          <ul className="list-disc pl-6 type-body text-text-primary">
            {dsLyDo.map((lyDo, chiSo) => (
              <li key={`${chiSo}-${lyDo}`}>{lyDo}</li>
            ))}
          </ul>
        ) : null}
        {traceId !== undefined ? (
          <p className="type-body text-text-secondary">
            Mã theo dõi:{" "}
            <code className="type-code select-all text-text-primary">
              {traceId}
            </code>
          </p>
        ) : null}
        {hanhDong !== undefined ? (
          <div>
            <Button
              bienThe="tertiary"
              kichThuoc="sm"
              onClick={hanhDong.onClick}
            >
              {hanhDong.nhan}
            </Button>
          </div>
        ) : null}
      </div>
    </div>
  );
}
