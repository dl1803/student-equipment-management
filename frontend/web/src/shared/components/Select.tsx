// Select — CMP-06 Ô chọn, chế độ chọn một (DS 8.3). Sở hữu: Thịnh (shared core).
// Dùng <select> gốc với token (D05-T): có sẵn bàn phím (mũi tên, Enter/Space), không viết thư viện
// chọn tổng quát. Chế độ chọn nhiều là SelectNhieu (D08-T).
// Dùng trong FormField như TextField: {(tt) => <Select {...tt} ... />}.
// Nguồn lựa chọn tải từ máy chủ: dangTaiLuaChon vô hiệu ô và hiện "Đang tải"; loiTaiLuaChon đặt ô
// ở trạng thái lỗi kèm nút "Thử lại", các điều khiển khác vẫn dùng được (DS CMP-06).
import { Button } from "./Button";
import { classNames } from "./classNames";

/** Chữ của trạng thái nguồn lựa chọn, dùng chung cho Select và SelectNhieu. */
export const CHU_NGUON_LUA_CHON = {
  DANG_TAI: "Đang tải",
  LOI: "Không tải được danh sách lựa chọn",
} as const;

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
  /** Chữ của mục "Tất cả" khi cần cụ thể hơn, ví dụ "Tất cả phòng được cấp quyền" (SCR-NV-02). */
  nhanTatCa?: string;
  /**
   * Chữ gợi ý khi chưa chọn (không có mục Tất cả và giaTri = null). Mục gợi ý không chọn lại
   * được, để ô không tự hiện lựa chọn đầu tiên trong khi giá trị thật vẫn là null.
   */
  goiY?: string;
  disabled?: boolean;
  required?: boolean;
  "aria-invalid"?: boolean;
  "aria-describedby"?: string;
  className?: string;
  /** Nguồn lựa chọn đang tải lần đầu. */
  dangTaiLuaChon?: boolean;
  /** Nguồn lựa chọn không tải được (DS CMP-06 "Lỗi tải lựa chọn"). */
  loiTaiLuaChon?: { onThuLai: () => void };
};

export function Select<T extends string = string>({
  id,
  dsLuaChon,
  giaTri,
  onChange,
  coMucTatCa = false,
  nhanTatCa = "Tất cả",
  goiY = "Chọn",
  disabled,
  required,
  className,
  dangTaiLuaChon = false,
  loiTaiLuaChon,
  ...aria
}: SelectProps<T>) {
  const o = (
    <select
      id={id}
      value={giaTri ?? GIA_TRI_TAT_CA}
      disabled={disabled || dangTaiLuaChon || loiTaiLuaChon !== undefined}
      required={required}
      aria-busy={dangTaiLuaChon || undefined}
      aria-invalid={aria["aria-invalid"] || loiTaiLuaChon !== undefined}
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
      {coMucTatCa ? (
        <option value={GIA_TRI_TAT_CA}>
          {dangTaiLuaChon ? CHU_NGUON_LUA_CHON.DANG_TAI : nhanTatCa}
        </option>
      ) : giaTri === null ? (
        <option value={GIA_TRI_TAT_CA} disabled>
          {dangTaiLuaChon ? CHU_NGUON_LUA_CHON.DANG_TAI : goiY}
        </option>
      ) : null}
      {dsLuaChon.map((lc) => (
        <option key={lc.giaTri} value={lc.giaTri}>
          {lc.nhan}
        </option>
      ))}
    </select>
  );
  if (loiTaiLuaChon === undefined) return o;
  return (
    <div className="flex flex-col gap-1">
      {o}
      <LoiNguonLuaChon onThuLai={loiTaiLuaChon.onThuLai} />
    </div>
  );
}

/** Dòng lỗi tải nguồn lựa chọn kèm "Thử lại" (DS CMP-06), dùng chung cho Select và SelectNhieu. */
export function LoiNguonLuaChon({ onThuLai }: { onThuLai: () => void }) {
  return (
    <div className="flex items-center gap-2">
      <span className="type-caption text-tone-danger-text">
        {CHU_NGUON_LUA_CHON.LOI}
      </span>
      <Button bienThe="tertiary" kichThuoc="sm" onClick={onThuLai}>
        Thử lại
      </Button>
    </div>
  );
}
