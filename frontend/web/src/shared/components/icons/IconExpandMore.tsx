// Biểu tượng `expand_more` — Material Symbols Outlined 400 (DS Phụ lục A).
// Gói @material-symbols/svg-400 0.48.2 không còn tệp expand_more; dùng hình của
// keyboard_arrow_down (cùng hình mũi tên xuống).
import { SvgIcon, type IconProps } from "./SvgIcon";

export function IconExpandMore(props: IconProps) {
  return (
    <SvgIcon {...props}>
      <path d="M480-344 240-584l43-43 197 197 197-197 43 43-240 240Z" />
    </SvgIcon>
  );
}
