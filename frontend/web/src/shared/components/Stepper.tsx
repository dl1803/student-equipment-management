// Stepper — CMP-31 Thanh tiến trình các bước (DS 8.6; Wireframes P-08). Sở hữu: Thịnh (shared core).
// Các bước nối nhau bằng đường kẻ color.border.subtle; mỗi bước là hình tròn size.icon.lg
// (radius.full) và chữ type.label.
// - Đã qua: nền color.action.primary.bg, dấu check, chữ color.text.link; là liên kết để quay lại.
// - Hiện tại: nền color.action.primary.bg, số bước, chữ đậm; aria-current="step".
// - Chưa tới: viền color.border.control, số bước, chữ phụ; không bấm được, không nhận tiêu điểm.
// Dấu check và số phân biệt đã qua và hiện tại ngoài màu. Chỉ cho quay lại các bước đã qua.
import Link from "next/link";
import { classNames } from "./classNames";
import { IconCheck } from "./icons/IconCheck";

export type BuocStepper = { nhan: string; href: string };

export type StepperProps = {
  /** Tên truy cập của thanh, ví dụ "Các bước giao thiết bị". */
  tenThanh: string;
  dsBuoc: ReadonlyArray<BuocStepper>;
  /** Số thứ tự bước hiện tại, bắt đầu từ 1. */
  buocHienTai: number;
  /** Gọi trước khi chuyển tới một bước đã qua (màn hình đánh dấu "quay lại", không phải vào mới). */
  onQuayLai?: (so: number) => void;
};

const HINH_TRON =
  "grid size-icon-lg shrink-0 place-items-center rounded-full type-label-sm";

export function Stepper({
  tenThanh,
  dsBuoc,
  buocHienTai,
  onQuayLai,
}: StepperProps) {
  return (
    <nav aria-label={tenThanh}>
      <ol className="flex items-center gap-3">
        {dsBuoc.map((buoc, i) => {
          const so = i + 1;
          const daQua = so < buocHienTai;
          const hienTai = so === buocHienTai;
          const noiDung = (
            <>
              <span
                className={classNames(
                  HINH_TRON,
                  daQua || hienTai
                    ? "bg-action-primary-bg text-action-primary-text"
                    : "border border-border-control text-text-secondary",
                )}
              >
                {daQua ? <IconCheck kichThuoc="sm" /> : so}
              </span>
              <span
                className={classNames(
                  "type-label whitespace-nowrap",
                  daQua && "text-text-link",
                  hienTai && "font-bold text-text-primary",
                  !daQua && !hienTai && "text-text-secondary",
                )}
              >
                {buoc.nhan}
              </span>
            </>
          );
          return (
            <li
              key={buoc.href}
              className={classNames(
                "flex items-center gap-3",
                i > 0 && "flex-1",
              )}
            >
              {i > 0 ? (
                <span
                  aria-hidden="true"
                  className="h-px min-w-6 flex-1 bg-border-subtle"
                />
              ) : null}
              {daQua ? (
                <Link
                  href={buoc.href}
                  onClick={() => onQuayLai?.(so)}
                  className="flex items-center gap-2 rounded-md hover:underline"
                >
                  {noiDung}
                  <span className="sr-only"> (đã qua)</span>
                </Link>
              ) : (
                <span
                  aria-current={hienTai ? "step" : undefined}
                  className="flex items-center gap-2"
                >
                  {noiDung}
                </span>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
