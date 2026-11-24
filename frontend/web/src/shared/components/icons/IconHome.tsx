// Biểu tượng `home` — Material Symbols Outlined 400 (DS Phụ lục A).
import { SvgIcon, type IconProps } from "./SvgIcon";

export function IconHome(props: IconProps) {
  return (
    <SvgIcon {...props}>
      <path d="M220-180h150v-250h220v250h150v-390L480-765 220-570v390Zm-60 60v-480l320-240 320 240v480H530v-250H430v250H160Zm320-353Z" />
    </SvgIcon>
  );
}
