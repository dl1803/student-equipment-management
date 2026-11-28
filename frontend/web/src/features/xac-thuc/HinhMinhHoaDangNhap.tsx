// Hình minh họa trang trí của SCR-CM-02 (DS 11.2, Wireframes SCR-CM-02): thẻ phiếu yêu cầu, biểu đồ
// cột và dấu duyệt, vẽ bằng SVG với màu qua token (class fill-*/stroke-*), nhóm tự vẽ nên không
// vướng giấy phép ảnh. Ẩn với trình đọc màn hình vì không mang thông tin. Sở hữu: Thịnh.
export function HinhMinhHoaDangNhap() {
  return (
    <svg
      viewBox="0 0 440 300"
      aria-hidden="true"
      focusable="false"
      className="w-full max-w-dialog-md"
    >
      {/* Thẻ "phiếu yêu cầu" chính */}
      <rect
        x="60"
        y="50"
        width="280"
        height="200"
        rx="16"
        className="fill-surface-default"
      />
      <rect
        x="84"
        y="74"
        width="120"
        height="12"
        rx="6"
        className="fill-text-primary opacity-80"
      />
      <rect
        x="84"
        y="94"
        width="80"
        height="8"
        rx="4"
        className="fill-border-subtle"
      />
      <rect
        x="252"
        y="72"
        width="64"
        height="22"
        rx="11"
        className="fill-surface-selected"
      />
      <rect
        x="262"
        y="80"
        width="44"
        height="6"
        rx="3"
        className="fill-text-link"
      />
      {/* Ba dòng thiết bị */}
      {[124, 156, 188].map((y, i) => (
        <g key={y}>
          <rect
            x="84"
            y={y}
            width="24"
            height="24"
            rx="6"
            className="fill-surface-muted"
          />
          <rect
            x="118"
            y={y + 4}
            width={110 - i * 18}
            height="7"
            rx="3.5"
            className="fill-text-secondary opacity-60"
          />
          <rect
            x="118"
            y={y + 15}
            width="60"
            height="5"
            rx="2.5"
            className="fill-border-subtle"
          />
          <rect
            x="268"
            y={y + 6}
            width="48"
            height="12"
            rx="6"
            className={
              i === 2 ? "fill-tone-warning-bg" : "fill-surface-selected"
            }
          />
        </g>
      ))}
      {/* Biểu đồ cột nhỏ */}
      <rect
        x="290"
        y="168"
        width="110"
        height="110"
        rx="16"
        className="fill-surface-default"
      />
      <rect
        x="306"
        y="230"
        width="14"
        height="32"
        rx="4"
        className="fill-nav-selected-bg"
      />
      <rect
        x="326"
        y="212"
        width="14"
        height="50"
        rx="4"
        className="fill-nav-selected-bg"
      />
      <rect
        x="346"
        y="196"
        width="14"
        height="66"
        rx="4"
        className="fill-focus-ring"
      />
      <rect
        x="366"
        y="220"
        width="14"
        height="42"
        rx="4"
        className="fill-nav-selected-bg"
      />
      {/* Dấu duyệt */}
      <circle cx="60" cy="60" r="26" className="fill-nav-selected-bg" />
      <path
        d="M48 60l8 8 16-16"
        fill="none"
        strokeWidth="5"
        strokeLinecap="round"
        strokeLinejoin="round"
        className="stroke-text-primary"
      />
    </svg>
  );
}
