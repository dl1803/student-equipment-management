// DataTable — CMP-18 Bảng dữ liệu (DS 8.4; FC 5.2). Sở hữu: Thịnh (shared core).
// Mức D05-T: bảng thật (caption, th scope), hàng tiêu đề cố định khi cuộn, cuộn ngang trong khung,
// trạng thái thân bằng StateView, tối đa 2 hành động ở dòng (nút văn bản sm, ĐX-DS-14).
// Phần còn lại của hợp đồng (dòng mở rộng, cột chọn, chọn dòng, làm nổi bật, Menu cho từ 3 hành
// động) nằm ở contracts.ts, bổ sung ở ngày dùng đến.
import type { ReactNode } from "react";
import { Button } from "./Button";
import { classNames } from "./classNames";

export type HanhDongDong = {
  nhan: string;
  onClick: () => void;
  /** Hành động hủy bỏ (Xóa, Khóa): khi gom vào Menu thì chữ tông danger, đặt cuối (DS CMP-20). */
  huyBo?: boolean;
  /** Không áp dụng cho trạng thái hiện tại thì ẩn, không vô hiệu (DS CMP-18). */
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

/** Tối đa số hành động hiện trực tiếp ở dòng; từ số này trở lên phải gom vào Menu (ĐX-DS-14). */
export const SO_HANH_DONG_TRUC_TIEP_TOI_DA = 2;

export type DataTableProps<TDong> = {
  /** Tên bảng cho trình đọc màn hình (caption ẩn). */
  tieuDe: string;
  dsCot: ReadonlyArray<CotBang<TDong>>;
  dsDong: ReadonlyArray<TDong>;
  /** Khóa ổn định của dòng (maPhong, maLoai), không dùng chỉ số (FC 5.2). */
  khoaDong: (dong: TDong) => string;
  /** Đang tải lần đầu/rỗng/lỗi: StateView thay phần thân bảng, giữ hàng tiêu đề. */
  trangThaiThan?: ReactNode;
  hanhDongDong?: (dong: TDong) => ReadonlyArray<HanhDongDong>;
  /** Đang tải lại khi đã có dữ liệu: giữ dữ liệu cũ, đánh dấu aria-busy (FC 5.2). */
  dangTaiLai?: boolean;
};

export function DataTable<TDong>({
  tieuDe,
  dsCot,
  dsDong,
  khoaDong,
  trangThaiThan,
  hanhDongDong,
  dangTaiLai = false,
}: DataTableProps<TDong>) {
  const coHanhDong = hanhDongDong !== undefined;
  const soCot = dsCot.length + (coHanhDong ? 1 : 0);

  return (
    <div className="overflow-x-auto rounded-md border border-border-subtle bg-surface-default">
      <table
        className="w-full border-collapse"
        aria-busy={dangTaiLai || undefined}
      >
        <caption className="sr-only">{tieuDe}</caption>
        <thead className="sticky top-0 z-[var(--ds-z-sticky)] bg-surface-muted">
          <tr>
            {dsCot.map((cot) => (
              <th
                key={cot.khoa}
                scope="col"
                className={classNames(
                  "px-3 py-2 type-label text-text-secondary",
                  cot.canLe === "right" ? "text-right" : "text-left",
                )}
              >
                {cot.tieuDe}
              </th>
            ))}
            {coHanhDong ? (
              <th
                scope="col"
                className="px-3 py-2 text-right type-label text-text-secondary"
              >
                <span className="sr-only">Thao tác</span>
              </th>
            ) : null}
          </tr>
        </thead>
        <tbody>
          {trangThaiThan !== undefined ? (
            <tr>
              <td colSpan={soCot}>{trangThaiThan}</td>
            </tr>
          ) : (
            dsDong.map((dong) => {
              const dsHanhDong = (hanhDongDong?.(dong) ?? []).filter(
                (hd) => hd.an !== true,
              );
              if (dsHanhDong.length > SO_HANH_DONG_TRUC_TIEP_TOI_DA) {
                // Lỗi lập trình: báo ngay ở dev/test; production chỉ ghi log và vẫn hiện đủ nút
                // (không làm sập màn hình, không âm thầm bỏ hành động).
                const thongDiep =
                  "DataTable: từ 3 hành động trở lên phải gom vào Menu (CMP-20, ĐX-DS-14) — chưa hiện thực";
                if (process.env.NODE_ENV !== "production") {
                  throw new Error(thongDiep);
                }
                console.error(thongDiep);
              }
              return (
                <tr
                  key={khoaDong(dong)}
                  className="h-table-row border-t border-border-subtle hover:bg-surface-subtle"
                >
                  {dsCot.map((cot) => (
                    <td
                      key={cot.khoa}
                      className={classNames(
                        "px-3 py-2 type-body text-text-primary",
                        cot.canLe === "right" && "text-right tabular-nums",
                        cot.khongXuongDong && "whitespace-nowrap",
                      )}
                    >
                      {cot.hienThi(dong)}
                    </td>
                  ))}
                  {coHanhDong ? (
                    <td className="px-3 py-2 text-right whitespace-nowrap">
                      <div className="inline-flex gap-2">
                        {dsHanhDong.map((hd) => (
                          <Button
                            key={hd.nhan}
                            bienThe="tertiary"
                            kichThuoc="sm"
                            onClick={hd.onClick}
                          >
                            {hd.nhan}
                          </Button>
                        ))}
                      </div>
                    </td>
                  ) : null}
                </tr>
              );
            })
          )}
        </tbody>
      </table>
    </div>
  );
}
