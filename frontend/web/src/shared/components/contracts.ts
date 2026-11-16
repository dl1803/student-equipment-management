// Hợp đồng props ĐÃ CÔNG BỐ cho nhóm thành phần Ngày 5–6 (chốt ở D04-T). Sở hữu: Thịnh (shared core).
// Đổi chữ ký sau ngày 4 phải báo người dùng theo hợp đồng (BAN_GIAO_CORE.md).
// CHỈ KHAI BÁO KIỂU — CHƯA HIỆN THỰC. Không import các kiểu này vào màn hình như thể thành phần đã có.
// Khi hiện thực: tạo shared/components/{Ten}.tsx, chuyển kiểu Props sang tệp đó, xóa khỏi đây.
// Mọi giá trị theo Design-System.md mục 8; chữ chuẩn theo DS 9.8.
import type { ReactNode } from "react";
import type { HoTrangThai, NhanPhatSinh } from "@/shared/labels/trangThai";

// ---------- CMP-06 Ô chọn (Select) ----------

export type LuaChon<T extends string = string> = {
  giaTri: T;
  nhan: string;
  /** Dòng phụ type.caption, ví dụ cơ sở của phòng. */
  moTa?: string;
  /** Loại/phòng đã ngưng sử dụng hiển thị kèm StatusBadge "Đã ngưng sử dụng" (SCR-QT-04). */
  daNgungSuDung?: boolean;
};

type SelectChung<T extends string> = {
  id: string;
  dsLuaChon: ReadonlyArray<LuaChon<T>>;
  disabled?: boolean;
  required?: boolean;
  "aria-invalid"?: boolean;
  "aria-describedby"?: string;
  /** Danh sách lựa chọn không tải được: ô ở trạng thái lỗi kèm "Thử lại" (DS CMP-06). */
  loiTaiLuaChon?: { onThuLai: () => void };
};

export type SelectProps<T extends string = string> = SelectChung<T> &
  (
    | {
        cheDo: "MOT";
        giaTri: T | null;
        onChange: (giaTri: T | null) => void;
        /** Bộ lọc có mục "Tất cả" đầu danh sách (giá trị null). */
        coMucTatCa?: boolean;
        goiY?: string;
      }
    | {
        cheDo: "NHIEU";
        giaTri: ReadonlyArray<T>;
        onChange: (giaTri: T[]) => void;
        /** Tóm tắt hiển thị trong ô, ví dụ (n) => `${n} phòng đã chọn`. */
        tomTat: (soDaChon: number) => string;
      }
  );

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

// ---------- CMP-14 Danh sách thông tin (KeyValueList) ----------

export type DongKeyValue = {
  ten: string;
  /** null/undefined hiển thị "—" hoặc giaTriThieu (ĐX-DS-12). */
  giaTri: ReactNode | null | undefined;
  /** Mã phiếu, mã thiết bị, mã SV dùng type.code. */
  laMa?: boolean;
  giaTriThieu?: string;
};

export type KeyValueListProps = {
  dsDong: ReadonlyArray<DongKeyValue>;
};

// ---------- CMP-18 Bảng dữ liệu (DataTable) ----------

export type HanhDongDong = {
  nhan: string;
  onClick: () => void;
  /** Hành động hủy bỏ: chữ danger, đặt cuối Menu (DS CMP-20). */
  huyBo?: boolean;
  /** Không áp dụng cho trạng thái hiện tại thì ẩn, không vô hiệu (DS CMP-18, CMP-20). */
  an?: boolean;
};

export type CotBang<TDong> = {
  khoa: string;
  tieuDe: string;
  /** right cho số lượng, tiền (DS CMP-18). */
  canLe?: "left" | "right";
  /** Mã, ngày giờ, tiền không xuống dòng. */
  khongXuongDong?: boolean;
  hienThi: (dong: TDong) => ReactNode;
};

export type DataTableProps<TDong> = {
  /** Tên bảng cho trình đọc màn hình. */
  tieuDe: string;
  dsCot: ReadonlyArray<CotBang<TDong>>;
  dsDong: ReadonlyArray<TDong>;
  /** Khóa ổn định của dòng (maPhieu, maThietBi), không dùng chỉ số (FC 5.2). */
  khoaDong: (dong: TDong) => string;
  /** Đang tải/rỗng/lỗi: StateView thay phần thân bảng, giữ hàng tiêu đề. */
  trangThaiThan?: ReactNode;
  /** Tối đa 2 hành động hiện trực tiếp; từ 3 gom vào Menu (ĐX-DS-14). */
  hanhDongDong?: (dong: TDong) => ReadonlyArray<HanhDongDong>;
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

// ---------- CMP-19 Phân trang (Pagination) ----------

export type PaginationProps = {
  /** Bắt đầu từ 1, theo tham số `trang` của API. */
  trang: number;
  kichThuoc: number;
  tongSo: number;
  onDoiTrang: (trang: number) => void;
  /** Vô hiệu cả hai nút khi đang tải trang mới. */
  dangTai?: boolean;
};

// ---------- CMP-20 Menu thao tác (Menu) ----------

export type MenuProps = {
  /** Nút kích hoạt là Button tertiary sm chữ "Thao tác" (DS 9.8). */
  nhanNutKichHoat?: string;
  dsMuc: ReadonlyArray<HanhDongDong>;
};

// ---------- Nhắc lại cho StatusBadge trong bảng ----------

/** Ô trạng thái trong DataTable dùng StatusBadge với đúng một trong hai dạng sau. */
export type GiaTriTrangThai =
  { ho: HoTrangThai; ma: string } | { nhanPhatSinh: NhanPhatSinh };
