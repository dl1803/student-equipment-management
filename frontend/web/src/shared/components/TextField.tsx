// TextField — CMP-04 Ô nhập (DS 8.3). Sở hữu: Thịnh (shared core).
// Chỉ là ô nhập; nhãn, dấu bắt buộc, chú thích và lỗi do FormField đảm nhiệm:
//   <FormField id="tenDangNhap" nhan="Tên đăng nhập" batBuoc loi={loi}>
//     {(tt) => <TextField {...tt} {...register("tenDangNhap", { required: true })} />}
//   </FormField>
// Trạng thái lỗi lấy từ aria-invalid do FormField truyền xuống, không có prop màu riêng.
// Chiều cao và chỗ chừa cho biểu tượng đi qua prop (kichThuoc, chuaChoBieuTuong), không truyền đè
// class h-/px- qua className: classNames không gộp class trùng thuộc tính, class nào thắng phụ thuộc
// thứ tự trong CSS build chứ không theo thứ tự viết (review PR #79).
import { forwardRef, type InputHTMLAttributes } from "react";
import { classNames } from "./classNames";

export type TextFieldProps = Omit<
  InputHTMLAttributes<HTMLInputElement>,
  "size"
> & {
  /** Đơn vị hậu tố bên phải (giờ, phút, ngày, VNĐ, %), type.body màu chữ phụ (DS CMP-04). */
  hauTo?: string;
  /** md: size.control.md (mặc định); lg: size.control.lg (ô đăng nhập, DS 11.2). */
  kichThuoc?: "md" | "lg";
  /** Chừa chỗ space.12 cho biểu tượng đặt chồng ở đầu và/hoặc cuối ô. */
  chuaChoBieuTuong?: "DAU" | "CUOI" | "HAI_DAU";
};

export const TextField = forwardRef<HTMLInputElement, TextFieldProps>(
  function TextField(
    {
      hauTo,
      kichThuoc = "md",
      chuaChoBieuTuong,
      className,
      type = "text",
      ...conLai
    },
    ref,
  ) {
    const chuaDau =
      chuaChoBieuTuong === "DAU" || chuaChoBieuTuong === "HAI_DAU";
    const chuaCuoi =
      chuaChoBieuTuong === "CUOI" || chuaChoBieuTuong === "HAI_DAU";
    const o = (
      <input
        {...conLai}
        ref={ref}
        type={type}
        className={classNames(
          "w-full rounded-md border border-border-control bg-surface-default type-body text-text-primary transition-colors",
          kichThuoc === "lg" ? "h-control-lg" : "h-control-md",
          chuaDau ? "pl-12" : "pl-3",
          chuaCuoi ? "pr-12" : "pr-3",
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
