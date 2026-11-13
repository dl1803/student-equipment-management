// Nối lớp CSS có điều kiện (FC 5.7); không dùng clsx/classnames. Sở hữu: Thịnh (shared core).
export function classNames(
  ...dsLop: Array<string | false | null | undefined>
): string {
  return dsLop.filter(Boolean).join(" ");
}
