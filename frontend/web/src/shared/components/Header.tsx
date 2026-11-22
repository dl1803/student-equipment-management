// Header — CMP-30 Thanh đầu trang (DS 8.7, UI-Screens 4.2–4.3). Sở hữu: Thịnh (shared core).
// Dải đầu của cột nội dung (DS 11.2, ĐX-DS-17): không chạy suốt chiều ngang, không đường kẻ dưới.
// Bên phải: khối người dùng (họ tên, vai trò bằng chữ thường — không StatusBadge, DS 7.1) và nút
// Đăng xuất. Tên hệ thống nằm ở đầu thanh menu bên (CMP-29). Đăng xuất không có hộp xác nhận (UF-CM-03).
import { Button } from "./Button";
import { IconLogout } from "./icons/IconLogout";

export type HeaderProps = {
  hoTen: string;
  nhanVaiTro: string;
  onDangXuat: () => void;
  dangDangXuat?: boolean;
};

export function Header({
  hoTen,
  nhanVaiTro,
  onDangXuat,
  dangDangXuat = false,
}: HeaderProps) {
  return (
    <header className="sticky top-0 z-[var(--ds-z-sticky)] flex h-web-header-height shrink-0 items-center justify-end gap-3 bg-surface-default px-gutter">
      <div className="flex flex-col items-end rounded-full bg-surface-subtle px-4 py-1">
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
    </header>
  );
}
