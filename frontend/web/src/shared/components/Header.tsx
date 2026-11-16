// Header — CMP-30 Thanh đầu trang (DS 8.7, UI-Screens 4.2–4.3). Sở hữu: Thịnh (shared core).
// Bên phải: họ tên, vai trò (chữ thường, không StatusBadge — DS 7.1), nút Đăng xuất.
// Đăng xuất không có hộp xác nhận (UF-CM-03).
import { Button } from "./Button";
import { IconLogout } from "./icons/IconLogout";

export type HeaderProps = {
  /** Tên hệ thống ở bên trái (Wireframes 2.2 có cho NV; khung QT ở 2.3 không ghi). */
  tenHeThong?: string;
  hoTen: string;
  nhanVaiTro: string;
  onDangXuat: () => void;
  dangDangXuat?: boolean;
};

export function Header({
  tenHeThong,
  hoTen,
  nhanVaiTro,
  onDangXuat,
  dangDangXuat = false,
}: HeaderProps) {
  return (
    <header className="sticky top-0 z-[var(--ds-z-sticky)] flex h-web-header-height shrink-0 items-center justify-between border-b border-border-subtle bg-surface-default px-gutter">
      <p className="type-title-card text-text-primary">{tenHeThong}</p>
      <div className="flex items-center gap-4">
        <div className="flex flex-col items-end">
          <span className="type-body-strong text-text-primary">{hoTen}</span>
          <span className="type-caption text-text-secondary">{nhanVaiTro}</span>
        </div>
        <Button
          bienThe="tertiary"
          bieuTuong={<IconLogout kichThuoc="md" />}
          onClick={onDangXuat}
          dangXuLy={dangDangXuat}
        >
          Đăng xuất
        </Button>
      </div>
    </header>
  );
}
