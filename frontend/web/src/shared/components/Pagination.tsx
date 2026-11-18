// Pagination — CMP-19 Phân trang (DS 8.4, 9.8; FA 3.10). Sở hữu: Thịnh (shared core).
// Dưới bảng, căn phải: "Hiển thị a–b trên tổng n", "Trang x / y", nút Trước/Sau.
// Ẩn khi tongSo không vượt một trang. Không có bộ chọn kích thước trang.
import { soTrang } from "@/shared/api/phanTrang";
import { Button } from "./Button";
import { IconChevronLeft } from "./icons/IconChevronLeft";
import { IconChevronRight } from "./icons/IconChevronRight";

export type PaginationProps = {
  /** Bắt đầu từ 1, theo tham số `trang` của API. */
  trang: number;
  kichThuoc: number;
  tongSo: number;
  onDoiTrang: (trang: number) => void;
  /** Vô hiệu cả hai nút khi đang tải trang mới. */
  dangTai?: boolean;
};

export function Pagination({
  trang,
  kichThuoc,
  tongSo,
  onDoiTrang,
  dangTai = false,
}: PaginationProps) {
  // Vẫn hiện khi đang ở trang > 1 dù tổng đã giảm còn một trang, để có nút Trước quay lại.
  if (tongSo <= kichThuoc && trang <= 1) return null;
  const tongSoTrang = soTrang(tongSo, kichThuoc);
  const tu = Math.min((trang - 1) * kichThuoc + 1, tongSo);
  const den = Math.min(trang * kichThuoc, tongSo);

  return (
    <nav
      aria-label="Phân trang"
      className="flex items-center justify-end gap-4"
    >
      <span className="type-caption text-text-secondary tabular-nums">
        Hiển thị {tu}–{den} trên tổng {tongSo}
      </span>
      <span className="type-body text-text-primary tabular-nums">
        Trang {trang} / {tongSoTrang}
      </span>
      <div className="flex gap-actions">
        <Button
          kichThuoc="sm"
          bieuTuong={<IconChevronLeft kichThuoc="md" />}
          disabled={dangTai || trang <= 1}
          onClick={() => onDoiTrang(trang - 1)}
        >
          Trước
        </Button>
        <Button
          kichThuoc="sm"
          bieuTuong={<IconChevronRight kichThuoc="md" />}
          disabled={dangTai || trang >= tongSoTrang}
          onClick={() => onDoiTrang(trang + 1)}
        >
          Sau
        </Button>
      </div>
    </nav>
  );
}
