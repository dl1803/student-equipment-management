// Hợp đồng props ĐÃ CÔNG BỐ, CHƯA HIỆN THỰC (chốt ở D04-T, cập nhật D05-T). Sở hữu: Thịnh.
// Đổi chữ ký phải báo người dùng theo hợp đồng (BAN_GIAO_CORE.md).
// CHỈ KHAI BÁO KIỂU. Không import các kiểu này vào màn hình như thể thành phần đã có.
// Khi hiện thực: chuyển kiểu sang shared/components/{Ten}.tsx và xóa khỏi đây.
// Đã hiện thực ở D05-T: Select (chọn một), KeyValueList, Pagination, DataTable (mức cơ bản).
import type { ReactNode } from "react";
import type { HanhDongDong } from "./DataTable";
import type { LuaChon } from "./Select";

// ---------- CMP-06 Ô chọn, chế độ chọn nhiều (SCR-QT-04 nhà cung cấp, SCR-QT-09 quyền phòng) ----------

export type SelectNhieuProps<T extends string = string> = {
  id: string;
  dsLuaChon: ReadonlyArray<LuaChon<T> & { moTa?: string }>;
  giaTri: ReadonlyArray<T>;
  onChange: (giaTri: T[]) => void;
  /** Tóm tắt hiển thị trong ô, ví dụ (n) => `${n} phòng đã chọn`. */
  tomTat: (soDaChon: number) => string;
  disabled?: boolean;
  required?: boolean;
  "aria-invalid"?: boolean;
  "aria-describedby"?: string;
  /** Danh sách lựa chọn không tải được: ô ở trạng thái lỗi kèm "Thử lại" (DS CMP-06). */
  loiTaiLuaChon?: { onThuLai: () => void };
};

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

// ---------- CMP-20 Menu thao tác (Menu) ----------

export type MenuProps = {
  /** Nút kích hoạt là Button tertiary sm chữ "Thao tác" (DS 9.8). */
  nhanNutKichHoat?: string;
  dsMuc: ReadonlyArray<HanhDongDong>;
};
