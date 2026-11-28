// Biểu tượng `qr_code_scanner` — dáng Material Symbols Outlined 400 (DS Phụ lục A): bốn góc khung quét
// và mã QR. Đầu ô nhập mã quầy (CMP-11).
import { SvgIcon, type IconProps } from "./SvgIcon";

export function IconQrCodeScanner(props: IconProps) {
  return (
    <SvgIcon {...props}>
      <path d="M80-680v-200h200v80H160v120H80Zm600-200h200v200h-80v-120H680v-80ZM80-80v-200h80v120h120v80H80Zm600 0v-80h120v-120h80v200H680ZM240-720h200v200H240v-200Zm60 60v80h80v-80h-80Zm220-60h200v200H520v-200Zm60 60v80h80v-80h-80ZM240-440h200v200H240v-200Zm60 60v80h80v-80h-80Zm220-60h80v80h-80v-80Zm120 0h80v80h-80v-80Zm-60 120h80v80h-80v-80Z" />
    </SvgIcon>
  );
}
