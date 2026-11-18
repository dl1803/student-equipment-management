// DataTable — CMP-18 Bảng dữ liệu (DS 8.4; FC 5.2). Sở hữu: Thịnh (shared core).
// Bảng thật (caption, th scope), hàng tiêu đề cố định khi cuộn, cuộn ngang trong khung, trạng thái
// thân bằng StateView. Hành động ở dòng: tối đa 2 nút văn bản sm, từ 3 gom vào Menu (ĐX-DS-14).
// Phần còn lại của hợp đồng (dòng mở rộng, cột chọn, chọn dòng, làm nổi bật) nằm ở contracts.ts,
// bổ sung ở ngày dùng đến.
import type { ReactNode } from "react";
import { Button } from "./Button";
import { classNames } from "./classNames";
import { Menu, type HanhDongDong } from "./Menu";

// Kiểu mục hành động định nghĩa ở Menu; xuất lại ở đây để màn hình chỉ cần import từ DataTable.
export type { HanhDongDong };

export type CotBang<TDong> = {
  khoa: string;
  tieuDe: string;
  /** right cho số lượng, tiền (DS CMP-18). */
  canLe?: "left" | "right";
  /** Mã, ngày giờ, tiền không xuống dòng. */
  khongXuongDong?: boolean;
  hienThi: (dong: TDong) => ReactNode;
};

/** Tối đa số hành động hiện trực tiếp ở dòng; vượt số này thì gom vào Menu (ĐX-DS-14). */
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
              // Ngưỡng tính sau khi bỏ mục ẩn: cùng màn hình, dòng có 2 mục hiện thì là nút trực tiếp,
              // dòng có từ 3 mục hiện thì gom vào Menu (ĐX-DS-14).
              const gomVaoMenu =
                dsHanhDong.length > SO_HANH_DONG_TRUC_TIEP_TOI_DA;
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
                      {gomVaoMenu ? (
                        <Menu dsMuc={dsHanhDong} />
                      ) : (
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
                      )}
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
