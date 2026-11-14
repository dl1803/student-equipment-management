// Chỉ báo quay — phần con của CMP-25 (StateView) và trạng thái đang xử lý của CMP-01 (Button).
// Không phải CMP riêng; tính năng không dùng trực tiếp mà dùng StateView hoặc Button (DS 8.5, mục 6).
// Cung quay color.progress.indicator trên vạch nền color.progress.track.
import { classNames } from "./classNames";

export type ChiBaoQuayProps = {
  kichThuoc?: "md" | "lg";
  className?: string;
};

export function ChiBaoQuay({ kichThuoc = "lg", className }: ChiBaoQuayProps) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden="true"
      focusable="false"
      className={classNames(
        "shrink-0 animate-quay",
        kichThuoc === "lg" ? "size-icon-lg" : "size-icon-md",
        className,
      )}
    >
      <circle
        cx="12"
        cy="12"
        r="9"
        strokeWidth="3"
        className="stroke-progress-track"
      />
      <path
        d="M12 3a9 9 0 0 1 9 9"
        strokeWidth="3"
        strokeLinecap="round"
        className="stroke-progress-indicator"
      />
    </svg>
  );
}
