// Sidebar — CMP-29 Thanh menu bên (DS 8.7, UI-Screens 4.2–4.3). Sở hữu: Thịnh (shared core).
// Danh sách mục do cấu hình menu theo vai trò truyền vào; không chứa mục ngoài vai trò (NFR-01).
// Bố cục (DS 11.2, ĐX-DS-17): cao hết cửa sổ, nền color.surface.subtle, tên hệ thống ở đầu (chữ, không
// logo — ĐX-DS-01). Mục cao size.control.md, bo radius.md. Mục đang chọn: nền color.nav.selected.bg,
// chữ color.nav.selected.text và chữ đậm (dấu hiệu ngoài màu, DS mục 6). Mỗi mục có biểu tượng
// size.icon.md ở trước chữ (Material Symbols, DS Phụ lục A); biểu tượng chỉ bổ trợ, ẩn với trình đọc
// màn hình vì tên mục đã đủ nghĩa.
import Link from "next/link";
import type { ComponentType } from "react";
import { classNames } from "./classNames";
import type { IconProps } from "./icons/SvgIcon";

export type MucMenu = {
  nhan: string;
  href: string;
  /** Biểu tượng trước chữ (DS CMP-29, Phụ lục A). */
  bieuTuong?: ComponentType<IconProps>;
  /**
   * Tiền tố nhóm tuyến của luồng nhiều bước: mục được đánh dấu ở mọi tuyến dưới tiền tố này,
   * ví dụ "Giao thiết bị" trỏ /nv/giao-thiet-bi/the nhưng sáng ở cả bốn bước /nv/giao-thiet-bi/*.
   */
  tienToNhom?: string;
};

export type NhomMenu = {
  /** QT có nhóm "Danh mục", "Cấu hình"; mục không có nhóm thì bỏ tiêu đề. */
  tieuDe?: string;
  dsMuc: ReadonlyArray<MucMenu>;
};

export type SidebarProps = {
  /** Tên hệ thống ở đầu thanh menu (DS CMP-29). */
  tenHeThong: string;
  /** Tuyến trang chủ của vai trò: chỉ khớp chính xác, không khớp mọi tuyến con. */
  tuyenTrangChu: string;
  dsNhom: ReadonlyArray<NhomMenu>;
  /** Tuyến hiện tại, để đánh dấu mục đang chọn bằng aria-current="page". */
  tuyenHienTai: string;
};

/**
 * Mục đang chọn: trùng tuyến; hoặc tuyến hiện tại nằm dưới tiền tố nhóm (nếu có) hay dưới tuyến
 * của mục. Trang chủ chỉ khớp chính xác để không sáng ở mọi tuyến con.
 */
export function laMucDangChon(
  muc: MucMenu,
  tuyenHienTai: string,
  tuyenTrangChu: string,
): boolean {
  if (muc.href === tuyenHienTai) return true;
  if (muc.href === tuyenTrangChu) return false;
  const goc = muc.tienToNhom ?? muc.href;
  return tuyenHienTai === goc || tuyenHienTai.startsWith(`${goc}/`);
}

export function Sidebar({
  tenHeThong,
  tuyenTrangChu,
  dsNhom,
  tuyenHienTai,
}: SidebarProps) {
  return (
    <aside className="flex w-web-sidebar-width shrink-0 flex-col bg-surface-subtle">
      <Link
        href={tuyenTrangChu}
        className="flex h-web-header-height shrink-0 items-center px-6 type-title-card font-bold text-text-primary"
      >
        {tenHeThong}
      </Link>
      <nav
        aria-label="Menu chính"
        className="flex min-h-0 flex-1 flex-col gap-2 overflow-y-auto px-4 pb-6"
      >
        {dsNhom.map((nhom, chiSo) => (
          <div
            key={nhom.tieuDe ?? `nhom-${chiSo}`}
            className="flex flex-col gap-1"
          >
            {nhom.tieuDe !== undefined ? (
              <p className="px-4 pt-3 pb-1 type-caption text-text-secondary">
                {nhom.tieuDe}
              </p>
            ) : null}
            <ul className="flex flex-col gap-1">
              {nhom.dsMuc.map((muc) => {
                const dangChon = laMucDangChon(
                  muc,
                  tuyenHienTai,
                  tuyenTrangChu,
                );
                const BieuTuong = muc.bieuTuong;
                return (
                  <li key={muc.href}>
                    <Link
                      href={muc.href}
                      aria-current={dangChon ? "page" : undefined}
                      className={classNames(
                        "flex min-h-control-md items-center gap-3 rounded-md px-4 py-2 type-label transition-colors",
                        dangChon
                          ? "bg-nav-selected-bg font-bold text-nav-selected-text"
                          : "text-text-secondary hover:bg-surface-muted hover:text-text-primary",
                      )}
                    >
                      {BieuTuong !== undefined ? (
                        <BieuTuong kichThuoc="md" />
                      ) : null}
                      <span>{muc.nhan}</span>
                    </Link>
                  </li>
                );
              })}
            </ul>
          </div>
        ))}
      </nav>
    </aside>
  );
}
