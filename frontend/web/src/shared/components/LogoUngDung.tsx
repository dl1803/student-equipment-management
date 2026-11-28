// Logo ứng dụng (DS ĐX-DS-18; bản vẽ nhóm tạo trên Figma): hộp thiết bị và hai mũi tên mượn/trả.
// Sở hữu: Thịnh (shared core). Màu theo token: nền color.nav.selected.bg (chanh), nét color.text.link (xanh lá đậm).
// Trang trí, ẩn với trình đọc màn hình (tên hệ thống luôn đi kèm bằng chữ).
const NET = {
  fill: "none",
  strokeWidth: 22,
  strokeLinecap: "round",
  strokeLinejoin: "round",
  className: "stroke-text-link",
} as const;

export function LogoUngDung({ className }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 512 512"
      aria-hidden="true"
      focusable="false"
      className={className}
    >
      <rect
        width="512"
        height="512"
        rx="104"
        className="fill-nav-selected-bg"
      />
      <path {...NET} d="M175 224L256 188L337 224L256 263L175 224Z" />
      <path {...NET} d="M337 224V293L256 333L175 293V224M256 333V263" />
      <path
        {...NET}
        d="M112 242C100 143 204 100 289 119C333 129 364 153 385 190"
      />
      <path {...NET} d="M350 181L385 190L394 155" />
      <path
        {...NET}
        d="M400 270C412 369 308 412 223 393C179 383 148 359 127 322"
      />
      <path {...NET} d="M162 331L127 322L118 357" />
    </svg>
  );
}
