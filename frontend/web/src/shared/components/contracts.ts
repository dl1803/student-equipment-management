// Hợp đồng props ĐÃ CÔNG BỐ, CHƯA HIỆN THỰC (chốt ở D04-T, cập nhật D05-T). Sở hữu: Thịnh.
// Đổi chữ ký phải báo người dùng theo hợp đồng (BAN_GIAO_CORE.md).
// CHỈ KHAI BÁO KIỂU. Không import các kiểu này vào màn hình như thể thành phần đã có.
// Khi hiện thực: chuyển kiểu sang shared/components/{Ten}.tsx và xóa khỏi đây.
// Đã hiện thực ở D05-T: Select (chọn một), KeyValueList, Pagination, DataTable (mức cơ bản).
// Đã hiện thực ở D08-T: SelectNhieu (CMP-06 chọn nhiều) ở ./SelectNhieu.
import type { ReactNode } from "react";

// ---------- CMP-07 Ô tích và nút chọn (Checkbox, Radio) ----------

export type CheckboxProps = {
  id: string;
  nhan: ReactNode;
  checked: boolean;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
  "aria-invalid"?: boolean;
  "aria-describedby"?: string;
};

export type RadioGroupProps<T extends string = string> = {
  /** Tên nhóm cho trình đọc màn hình (DS CMP-07). */
  tenNhom: string;
  name: string;
  dsLuaChon: ReadonlyArray<{ giaTri: T; nhan: string }>;
  giaTri: T | null;
  onChange: (giaTri: T) => void;
  disabled?: boolean;
  "aria-invalid"?: boolean;
  "aria-describedby"?: string;
};

// ---------- CMP-18 Bảng dữ liệu: phần mở rộng sẽ thêm vào DataTableProps ----------

export type DataTableMoRongProps<TDong> = {
  /** Dòng bấm được để mở màn hình khác: nhận tiêu điểm, Enter mở. */
  onChonDong?: (dong: TDong) => void;
  /** Dòng mở rộng (aria-expanded). */
  moRong?: {
    noiDung: (dong: TDong) => ReactNode;
    nhanTruyCap: (dong: TDong) => string;
  };
  /** Cột chọn bằng ô tích. */
  chon?: {
    dsKhoaDaChon: ReadonlySet<string>;
    onDoiChon: (khoa: string, chon: boolean) => void;
    nhanTruyCap: (dong: TDong) => string;
  };
  /** Làm nổi bật dòng (DS mục 6): THONG_TIN hoặc CANH_BAO (luôn kèm nhãn chữ). */
  lamNoiBat?: (dong: TDong) => "THONG_TIN" | "CANH_BAO" | null;
};

// CMP-20 Menu đã hiện thực ở ./Menu (D06-T); DataTable tự gom từ 3 hành động.
