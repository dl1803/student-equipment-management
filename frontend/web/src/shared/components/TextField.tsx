// TextField — CMP-04 Ô nhập (DS 8.3). Sở hữu: Thịnh (shared core).
// Chỉ là ô nhập; nhãn, dấu bắt buộc, chú thích và lỗi do FormField đảm nhiệm:
//   <FormField id="tenDangNhap" nhan="Tên đăng nhập" batBuoc loi={loi}>
//     {(tt) => <TextField {...tt} {...register("tenDangNhap", { required: true })} />}
//   </FormField>
// Trạng thái lỗi lấy từ aria-invalid do FormField truyền xuống, không có prop màu riêng.
import { forwardRef, type InputHTMLAttributes } from "react";
import { classNames } from "./classNames";

export type TextFieldProps = Omit<
  InputHTMLAttributes<HTMLInputElement>,
  "size"
> & {
  /** Đơn vị hậu tố bên phải (giờ, phút, ngày, VNĐ, %), type.body màu chữ phụ (DS CMP-04). */
  hauTo?: string;
};

export const TextField = forwardRef<HTMLInputElement, TextFieldProps>(
  function TextField({ hauTo, className, type = "text", ...conLai }, ref) {
    const o = (
      <input
        {...conLai}
        ref={ref}
        type={type}
        className={classNames(
          "h-control-md w-full rounded-md border border-border-control bg-surface-default px-3 type-body text-text-primary transition-colors",
          "placeholder:text-text-placeholder hover:border-border-control-hover",
          "focus-visible:outline-none focus:border-focus-ring focus:border-[length:var(--ds-border-width-strong)]",
          "aria-invalid:border-border-error",
          "disabled:cursor-not-allowed disabled:bg-action-disabled-bg disabled:text-action-disabled-text disabled:border-action-disabled-border",
          // Ô vô hiệu cũng khớp :read-only nên chỉ áp kiểu chỉ đọc cho ô còn bật (DS mục 6).
          "enabled:read-only:bg-surface-subtle enabled:read-only:border-border-subtle",
          className,
        )}
      />
    );
    if (hauTo === undefined) return o;
    return (
      <div className="flex items-center gap-2">
        {o}
        <span className="type-body text-text-secondary whitespace-nowrap">
          {hauTo}
        </span>
      </div>
    );
  },
);
