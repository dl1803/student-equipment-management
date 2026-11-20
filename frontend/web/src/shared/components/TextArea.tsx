// TextArea — CMP-04 Ô nhập, biến thể Nhiều dòng (DS 8.3). Sở hữu: Thịnh (shared core).
// Cùng hình thức và cách dùng với TextField (nhãn, lỗi do FormField đảm nhiệm), tối thiểu 3 dòng
// (DS CMP-04 "ô nhiều dòng tối thiểu 3 dòng"), người dùng kéo giãn theo chiều dọc. Văn bản dài
// tự xuống dòng, không cắt.
import { forwardRef, type TextareaHTMLAttributes } from "react";
import { classNames } from "./classNames";

export type TextAreaProps = TextareaHTMLAttributes<HTMLTextAreaElement>;

/** Số dòng tối thiểu của ô nhiều dòng (DS CMP-04). */
const SO_DONG_TOI_THIEU = 3;

export const TextArea = forwardRef<HTMLTextAreaElement, TextAreaProps>(
  function TextArea({ className, rows = SO_DONG_TOI_THIEU, ...conLai }, ref) {
    return (
      <textarea
        {...conLai}
        ref={ref}
        rows={Math.max(rows, SO_DONG_TOI_THIEU)}
        className={classNames(
          "w-full resize-y rounded-md border border-border-control bg-surface-default px-3 py-2 type-body text-text-primary transition-colors",
          "placeholder:text-text-placeholder hover:border-border-control-hover",
          "focus-visible:outline-none focus:border-focus-ring focus:border-[length:var(--ds-border-width-strong)]",
          "aria-invalid:border-border-error",
          "disabled:cursor-not-allowed disabled:bg-action-disabled-bg disabled:text-action-disabled-text disabled:border-action-disabled-border",
          "enabled:read-only:bg-surface-subtle enabled:read-only:border-border-subtle",
          className,
        )}
      />
    );
  },
);
