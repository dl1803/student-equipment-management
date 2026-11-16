// Sidebar — CMP-29 Thanh menu bên (DS 8.7, UI-Screens 4.2–4.3). Sở hữu: Thịnh (shared core).
// Danh sách mục do cấu hình menu theo vai trò truyền vào; không chứa mục ngoài vai trò (NFR-01).
import Link from "next/link";
import { classNames } from "./classNames";

export type MucMenu = {
  nhan: string;
  href: string;
};

export type NhomMenu = {
  /** QT có nhóm "Danh mục", "Cấu hình"; mục không có nhóm thì bỏ tiêu đề. */
  tieuDe?: string;
  dsMuc: ReadonlyArray<MucMenu>;
};

export type SidebarProps = {
  dsNhom: ReadonlyArray<NhomMenu>;
  /** Tuyến hiện tại, để đánh dấu mục đang chọn bằng aria-current="page". */
  tuyenHienTai: string;
  /** Tuyến trang chủ của vai trò: chỉ khớp chính xác, không khớp mọi tuyến con. */
  tuyenTrangChu: string;
};

/** Mục đang chọn: trùng tuyến, hoặc tuyến hiện tại là tuyến con của mục (trừ trang chủ). */
export function laMucDangChon(
  href: string,
  tuyenHienTai: string,
  tuyenTrangChu: string,
): boolean {
  if (href === tuyenHienTai) return true;
  return href !== tuyenTrangChu && tuyenHienTai.startsWith(`${href}/`);
}

export function Sidebar({ dsNhom, tuyenHienTai, tuyenTrangChu }: SidebarProps) {
  return (
    <nav
      aria-label="Menu chính"
      className="w-web-sidebar-width shrink-0 overflow-y-auto border-r border-border-subtle bg-surface-default py-2"
    >
      {dsNhom.map((nhom, chiSo) => (
        <div
          key={nhom.tieuDe ?? `nhom-${chiSo}`}
          className="flex flex-col py-1"
        >
          {nhom.tieuDe !== undefined ? (
            <p className="px-4 py-1 type-caption text-text-secondary">
              {nhom.tieuDe}
            </p>
          ) : null}
          <ul className="flex flex-col">
            {nhom.dsMuc.map((muc) => {
              const dangChon = laMucDangChon(
                muc.href,
                tuyenHienTai,
                tuyenTrangChu,
              );
              return (
                <li key={muc.href}>
                  <Link
                    href={muc.href}
                    aria-current={dangChon ? "page" : undefined}
                    className={classNames(
                      "flex h-control-md items-center border-l-[length:var(--ds-border-width-accent)] px-4 type-label transition-colors",
                      dangChon
                        ? "border-border-selected bg-surface-selected text-text-link"
                        : "border-transparent text-text-primary hover:bg-surface-subtle",
                    )}
                  >
                    {muc.nhan}
                  </Link>
                </li>
              );
            })}
          </ul>
        </div>
      ))}
    </nav>
  );
}
