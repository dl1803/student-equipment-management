// Select — CMP-06 Ô chọn, chế độ chọn một (DS 8.3). Sở hữu: Thịnh (shared core).
// Dùng <select> gốc với token (D05-T): có sẵn bàn phím (mũi tên, Enter/Space), không viết thư viện
// chọn tổng quát. Chế độ chọn nhiều (SCR-QT-04, QT-09) hiện thực sau theo contracts.ts.
// Dùng trong FormField như TextField: {(tt) => <Select {...tt} ... />}.
import { classNames } from "./classNames";

export type LuaChon<T extends string = string> = {
  giaTri: T;
  nhan: string;
};

/** Giá trị đại diện mục "Tất cả" trong <select> (không gửi lên API). */
const GIA_TRI_TAT_CA = "";

export type SelectProps<T extends string = string> = {
  id: string;
  dsLuaChon: ReadonlyArray<LuaChon<T>>;
  /** null là "Tất cả" (khi coMucTatCa) hoặc chưa chọn. */
  giaTri: T | null;
  onChange: (giaTri: T | null) => void;
  /** Bộ lọc có mục "Tất cả" đầu danh sách (DS CMP-06). */
  coMucTatCa?: boolean;
  disabled?: boolean;
  required?: boolean;
  "aria-invalid"?: boolean;
  "aria-describedby"?: string;
  className?: string;
};

export function Select<T extends string = string>({
  id,
  dsLuaChon,
  giaTri,
  onChange,
  coMucTatCa = false,
  disabled,
  required,
  className,
  ...aria
}: SelectProps<T>) {
  return (
    <select
      id={id}
      value={giaTri ?? GIA_TRI_TAT_CA}
      disabled={disabled}
      required={required}
      aria-invalid={aria["aria-invalid"]}
      aria-describedby={aria["aria-describedby"]}
      onChange={(suKien) => {
        const chon = dsLuaChon.find((lc) => lc.giaTri === suKien.target.value);
        onChange(chon === undefined ? null : chon.giaTri);
      }}
      className={classNames(
        "h-control-md w-full rounded-md border border-border-control bg-surface-default px-3 type-body text-text-primary transition-colors cursor-pointer",
        "hover:border-border-control-hover aria-invalid:border-border-error",
        "disabled:cursor-not-allowed disabled:bg-action-disabled-bg disabled:text-action-disabled-text disabled:border-action-disabled-border",
        className,
      )}
    >
      {coMucTatCa ? <option value={GIA_TRI_TAT_CA}>Tất cả</option> : null}
      {dsLuaChon.map((lc) => (
        <option key={lc.giaTri} value={lc.giaTri}>
          {lc.nhan}
        </option>
      ))}
    </select>
  );
}
