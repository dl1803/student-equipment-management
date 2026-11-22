// Nội dung demo các màn hình Android sinh viên (UI-Screens mục 5; Wireframes mục 2.1 và 5).
// Mỗi màn hình: id, mã, tên, trạng thái minh họa, tab gốc (null nếu là màn chi tiết), nội dung.
// Phần tử có data-di="<id>" chuyển sang màn hình đó trong app.html. Dữ liệu là ví dụ.

const P = {
  home: "M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z",
  search: "M15.5 14h-.79l-.28-.27A6.47 6.47 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z",
  bell: "M12 22c1.1 0 2-.9 2-2h-4c0 1.1.89 2 2 2zm6-6v-5c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z",
  person: "M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z",
  back: "M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z",
  chevron: "M10 6 8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z",
  expand: "M16.59 8.59 12 13.17 7.41 8.59 6 10l6 6 6-6z",
  check: "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z",
  info: "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z",
  warning: "M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z",
  error: "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z",
  dot: "M12 8a4 4 0 1 0 0 8 4 4 0 0 0 0-8z",
  add: "M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z",
  remove: "M19 13H5v-2h14v2z",
  image: "M21 19V5c0-1.1-.9-2-2-2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2zM8.5 13.5l2.5 3.01L14.5 12l4.5 6H5l3.5-4.5z",
  event: "M16.53 11.06 15.47 10l-4.88 4.88-2.12-2.12-1.06 1.06L10.59 17l5.94-5.94zM19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19a2 2 0 0 0 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11z",
  receipt: "M19.5 3.5 18 2l-1.5 1.5L15 2l-1.5 1.5L12 2l-1.5 1.5L9 2 7.5 3.5 6 2 4.5 3.5 3 2v20l1.5-1.5L6 22l1.5-1.5L9 22l1.5-1.5L12 22l1.5-1.5L15 22l1.5-1.5L18 22l1.5-1.5L21 22V2l-1.5 1.5zM19 19.09H5V4.91h14v14.18zM6 15h12v2H6zm0-4h12v2H6zm0-4h12v2H6z",
  hourglass: "M6 2v6h.01L6 8.01 10 12l-4 4 .01.01H6V22h12v-5.99h-.01L18 16l-4-4 4-3.99-.01-.01H18V2H6zm10 14.5V20H8v-3.5l4-4 4 4zm-4-5-4-4V4h8v3.5l-4 4z",
  history: "M13 3a9 9 0 0 0-9 9H1l3.89 3.89.07.14L9 12H6c0-3.87 3.13-7 7-7s7 3.13 7 7-3.13 7-7 7c-1.93 0-3.68-.79-4.94-2.06l-1.42 1.42A8.95 8.95 0 0 0 13 21a9 9 0 0 0 0-18zm-1 5v5l4.28 2.54.72-1.21-3.5-2.08V8H12z",
  gavel: "M1 21h12v2H1zM5.25 8.07l2.83-2.83 14.14 14.14-2.83 2.83zM12.32 1l5.66 5.66-2.83 2.83-5.66-5.66zM3.83 9.49l5.66 5.66-2.83 2.83-5.66-5.66z",
  flag: "M14.4 6 14 4H5v17h2v-7h5.6l.4 2h7V6z",
  inbox: "M19 3H4.99C3.88 3 3 3.9 3 5v14c0 1.1.88 2 1.99 2H19c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 12h-4c0 1.66-1.35 3-3 3s-3-1.34-3-3H4.99V5H19v10z",
  searchOff: "M15.5 14h-.79l-.28-.27A6.47 6.47 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14zm-2.5-6.29L8.29 7 9.5 8.21 10.71 7 12 8.29 10.79 9.5 12 10.71 10.71 12 9.5 10.79 8.29 12 7 10.71 8.21 9.5z",
};

const ic = (ten, lop = "") =>
  `<svg class="icon ${lop}" viewBox="0 0 24 24" aria-hidden="true"><path d="${P[ten]}"/></svg>`;

const ICON_TONG = { info: "info", success: "check", warning: "warning", danger: "error", neutral: "dot" };
/** Nhãn trạng thái CMP-13: nền, viền, chữ theo tông và biểu tượng (dấu hiệu ngoài màu). */
const nhan = (chu, tong, dam = false) =>
  `<span class="badge tone-${tong}${dam ? " strong" : ""}">${ic(ICON_TONG[tong], "sm")}${chu}</span>`;

const banner = (tong, tieuDe, noiDung = "", ds = []) =>
  `<div class="banner tone-${tong}">${ic(ICON_TONG[tong])}<div>${tieuDe ? `<p class="t-card" style="margin:0">${tieuDe}</p>` : ""}${noiDung}${
    ds.length ? `<ul>${ds.map((d) => `<li>${d}</li>`).join("")}</ul>` : ""
  }</div></div>`;

const kv = (dong) =>
  `<dl class="kv">${dong.map(([k, v]) => `<dt>${k}</dt><dd>${v}</dd>`).join("")}</dl>`;

const rong = (icon, thongDiep, them = "") =>
  `<div class="state">${ic(icon)}<p class="t-body" style="margin:0">${thongDiep}</p>${them}</div>`;

const MAN_HINH = [
  // ---------------- Chung ----------------
  {
    id: "dang-nhap", ma: "SCR-CM-01", ten: "Đăng nhập sinh viên", trangThai: "Đã nhập đủ, nút chính bật",
    tab: null, khongAppbar: true,
    noiDung: `
      <div style="padding-top:40px;display:flex;flex-direction:column;gap:16px">
        <h1 class="t-screen">Hệ thống mượn trả thiết bị</h1>
        <p class="t-caption" style="margin:0">* Bắt buộc</p>
        <div class="field"><label>Mã số sinh viên *</label><div class="input">22520001</div></div>
        <div class="field"><label>Mật khẩu *</label><div class="input">••••••••</div></div>
      </div>`,
    ghim: `<button class="btn primary" data-di="trang-chu">Đăng nhập</button>`,
  },
  {
    id: "dang-nhap-loi", ma: "SCR-CM-01", ten: "Đăng nhập sinh viên", trangThai: "Sai thông tin đăng nhập",
    tab: null, khongAppbar: true,
    noiDung: `
      <div style="padding-top:40px;display:flex;flex-direction:column;gap:16px">
        <h1 class="t-screen">Hệ thống mượn trả thiết bị</h1>
        ${banner("danger", "", "Mã số sinh viên hoặc mật khẩu không đúng")}
        <p class="t-caption" style="margin:0">* Bắt buộc</p>
        <div class="field"><label>Mã số sinh viên *</label><div class="input">22520001</div></div>
        <div class="field"><label>Mật khẩu *</label><div class="input placeholder">Nhập mật khẩu</div></div>
      </div>`,
    ghim: `<button class="btn primary" disabled>Đăng nhập</button>`,
  },

  // ---------------- SCR-SV-01 ----------------
  {
    id: "trang-chu", ma: "SCR-SV-01", ten: "Trang chủ sinh viên", trangThai: "Quyền mượn Bình thường",
    tab: "trang-chu", tieuDe: "Trang chủ",
    noiDung: `
      <p class="t-section">Xin chào, Nguyễn Văn An</p>
      <div class="card">
        <div class="row"><span class="t-label muted">Quyền mượn</span>${nhan("Bình thường", "success")}</div>
        <div class="row" style="align-items:baseline"><span class="t-label muted">Đang mượn</span>
          <span><span class="t-num">1</span><span class="muted"> / 3 thiết bị</span></span></div>
      </div>
      <div class="grid2">
        <div class="tile" data-di="tra-cuu"><span class="ic">${ic("search")}</span>Tra cứu thiết bị</div>
        <div class="tile" data-di="soan-phieu"><span class="ic">${ic("event")}</span>Đặt trước thiết bị</div>
        <div class="tile" data-di="ds-phieu"><span class="ic">${ic("receipt")}</span>Phiếu yêu cầu của tôi</div>
        <div class="tile" data-di="ds-cho"><span class="ic">${ic("hourglass")}</span>Đăng ký chờ của tôi</div>
        <div class="tile" data-di="lich-su"><span class="ic">${ic("history")}</span>Lịch sử mượn/trả</div>
        <div class="tile" data-di="bien-ban"><span class="ic">${ic("gavel")}</span>Biên bản và phí phạt</div>
        <div class="tile" data-di="vi-pham"><span class="ic">${ic("flag")}</span>Vi phạm</div>
        <div class="tile" data-di="ho-so"><span class="ic">${ic("person")}</span>Hồ sơ mượn</div>
      </div>`,
  },
  {
    id: "trang-chu-khoa", ma: "SCR-SV-01", ten: "Trang chủ sinh viên", trangThai: "Quyền mượn Tạm khóa (thẻ cảnh báo)",
    tab: "trang-chu", tieuDe: "Trang chủ",
    noiDung: `
      <p class="t-section">Xin chào, Nguyễn Văn An</p>
      <div class="banner tone-warning">${ic("warning")}<div style="display:flex;flex-direction:column;gap:6px">
        <div>${nhan("Tạm khóa", "warning", true)}</div>
        <span>Bạn chưa thể đặt trước hoặc đăng ký chờ.</span>
        <button class="btn secondary sm" style="align-self:flex-start" data-di="ho-so-chan">Xem lý do</button></div></div>
      <div class="grid2">
        <div class="tile"><span class="ic">${ic("search")}</span>Tra cứu thiết bị</div>
        <div class="tile"><span class="ic">${ic("event")}</span>Đặt trước thiết bị</div>
        <div class="tile"><span class="ic">${ic("receipt")}</span>Phiếu yêu cầu của tôi</div>
        <div class="tile"><span class="ic">${ic("hourglass")}</span>Đăng ký chờ của tôi</div>
      </div>
      <p class="t-caption" style="margin:0;text-align:center">(tám lối vào như màn Bình thường, vẫn dùng được)</p>`,
  },

  // ---------------- SCR-SV-02 ----------------
  {
    id: "tra-cuu", ma: "SCR-SV-02", ten: "Tra cứu thiết bị", trangThai: "Có kết quả; bộ lọc cuộn cùng danh sách",
    tab: "tra-cuu", tieuDe: "Tra cứu thiết bị",
    noiDung: `
      <div class="input placeholder">${ic("search")}<span style="flex:1;margin-left:8px">Tìm theo tên loại thiết bị</span></div>
      <div class="chips">
        <span class="chip">Loại: Tất cả ${ic("expand", "sm")}</span>
        <span class="chip">Phòng: Tất cả ${ic("expand", "sm")}</span>
        <span class="chip">Tình trạng hàng: Tất cả ${ic("expand", "sm")}</span>
      </div>
      <div class="list-gap">
        <div class="card tap" data-di="chi-tiet-loai">
          <div class="row"><span class="t-card">Laptop Dell Latitude 5420 · Dell</span>${ic("chevron")}</div>
          <div class="row"><span class="t-caption">Phòng thiết bị A1 · Cơ sở Linh Trung</span><span class="t-label">Còn: 5</span></div>
        </div>
        <div class="card tap" data-di="chi-tiet-het">
          <div class="row"><span class="t-card">Máy chiếu Epson EB-X51 · Epson</span>${ic("chevron")}</div>
          <div class="row"><span class="t-caption">Phòng thiết bị B2 · Cơ sở Thủ Đức</span>${nhan("Hết hàng", "danger")}</div>
        </div>
        <div class="card tap">
          <div class="row"><span class="t-card">Loa di động JBL Charge 5 · JBL</span>${ic("chevron")}</div>
          <div class="row"><span class="t-caption">Phòng thiết bị A1 · Cơ sở Linh Trung</span><span class="t-label">Còn: 2</span></div>
        </div>
        <div style="display:flex;justify-content:center;padding:8px"><span class="spinner"></span></div>
      </div>`,
  },
  {
    id: "tra-cuu-rong", ma: "SCR-SV-02", ten: "Tra cứu thiết bị", trangThai: "Rỗng do bộ lọc",
    tab: "tra-cuu", tieuDe: "Tra cứu thiết bị",
    noiDung: `
      <div class="input">${ic("search")}<span style="flex:1;margin-left:8px">bảng vẽ điện tử</span></div>
      <div class="chips">
        <span class="chip">Loại: Tất cả ${ic("expand", "sm")}</span>
        <span class="chip">Phòng: B2 ${ic("expand", "sm")}</span>
        <span class="chip">Tình trạng hàng: Còn hàng ${ic("expand", "sm")}</span>
        <button class="btn tertiary">Xóa bộ lọc</button>
      </div>
      ${rong("searchOff", "Không tìm thấy thiết bị phù hợp", `<button class="btn secondary">Xóa bộ lọc</button>`)}`,
  },

  // ---------------- SCR-SV-03 ----------------
  {
    id: "chi-tiet-loai", ma: "SCR-SV-03", ten: "Chi tiết loại thiết bị tại phòng", trangThai: "Còn hàng → nút Đặt trước",
    tab: null, tieuDe: "Chi tiết thiết bị",
    noiDung: `
      <div class="photos"><div class="photo">${ic("image", "lg")}</div><div class="photo">${ic("image", "lg")}</div><div class="photo">${ic("image", "lg")}</div><div class="photo">${ic("image", "lg")}</div></div>
      <div><h2 class="t-section">Laptop Dell Latitude 5420</h2><p class="muted" style="margin:4px 0 0">Dell · Phòng thiết bị A1 · Cơ sở Linh Trung</p></div>
      <div><p class="t-label" style="margin:0 0 4px">Mô tả</p><p class="t-body" style="margin:0">Laptop dùng cho thực hành lập trình và thiết kế, kèm sạc và túi chống sốc.</p></div>
      <div><p class="t-label" style="margin:0 0 4px">Thông số kỹ thuật</p>
        <ul class="t-body" style="margin:0;padding-left:20px"><li>Core i5-1145G7, RAM 16 GB</li><li>SSD 512 GB, màn 14 inch</li></ul></div>
      <div class="card"><div class="row" style="align-items:center"><span class="t-label muted">Số lượng khả dụng</span><span class="t-num">5</span></div></div>`,
    ghim: `<button class="btn primary" data-di="soan-phieu">Đặt trước</button>`,
  },
  {
    id: "chi-tiet-het", ma: "SCR-SV-03", ten: "Chi tiết loại thiết bị tại phòng", trangThai: "Hết hàng → nút Đăng ký chờ",
    tab: null, tieuDe: "Chi tiết thiết bị",
    noiDung: `
      <div class="photo big">${ic("image", "lg")}</div>
      <div><h2 class="t-section">Máy chiếu Epson EB-X51</h2><p class="muted" style="margin:4px 0 0">Epson · Phòng thiết bị B2 · Cơ sở Thủ Đức</p></div>
      <div><p class="t-label" style="margin:0 0 4px">Mô tả</p><p class="t-body" style="margin:0">Độ sáng 3.800 lumen, kèm điều khiển và dây HDMI.</p></div>
      <div class="card">
        <div class="row" style="align-items:center"><span class="t-label muted">Số lượng khả dụng</span><span style="display:flex;gap:8px;align-items:center"><span class="t-num">0</span>${nhan("Hết hàng", "danger")}</span></div>
        <div class="row"><span class="t-label muted">Ngày dự kiến có sẵn</span><span class="t-label">24/11/2026 15:30</span></div>
      </div>`,
    ghim: `<button class="btn primary" data-di="dlg-dang-ky-cho">Đăng ký chờ</button>`,
  },

  // ---------------- DLG-SV-01 ----------------
  {
    id: "dlg-dang-ky-cho", ma: "DLG-SV-01", ten: "Hộp thoại Đã đăng ký chờ", trangThai: "Chồng lên SCR-SV-03",
    tab: null, tieuDe: "Chi tiết thiết bị",
    noiDung: `
      <div class="photo big">${ic("image", "lg")}</div>
      <div><h2 class="t-section">Máy chiếu Epson EB-X51</h2><p class="muted" style="margin:4px 0 0">Epson · Phòng thiết bị B2</p></div>`,
    ghim: `<button class="btn primary" disabled>Đăng ký chờ</button>`,
    lopPhu: `
      <div class="scrim"><div class="dialog" role="dialog">
        <h2 class="t-section">Đã đăng ký chờ</h2>
        <p class="t-body" style="margin:0"><span class="t-code">ĐK-000007</span> · Máy chiếu Epson EB-X51 · Phòng thiết bị B2</p>
        <div style="display:flex;gap:12px;align-items:center">${nhan("Đang chờ", "info")}<span class="t-label">Số thứ tự chờ: 3</span></div>
        <p class="t-body muted" style="margin:0;font-size:14px;line-height:20px">Mỗi đăng ký chờ dành cho một thiết bị. Khi có thiết bị cùng loại được trả nguyên vẹn tại phòng, người đứng đầu danh sách được thông báo và giữ chỗ (mặc định 30 phút).</p>
        <div class="actions" style="justify-content:space-between"><button class="btn tertiary" data-di="chi-tiet-het">Đóng</button><button class="btn primary" data-di="ds-cho">Xem danh sách chờ</button></div>
      </div></div>`,
  },

  // ---------------- SCR-SV-04 ----------------
  {
    id: "soan-phieu", ma: "SCR-SV-04", ten: "Soạn phiếu yêu cầu mượn", trangThai: "Đã chọn phòng và số lượng",
    tab: null, tieuDe: "Đặt trước thiết bị",
    noiDung: `
      <div class="field"><label>Phòng thiết bị *</label><div class="input">Phòng thiết bị A1 · Cơ sở Linh Trung ${ic("expand")}</div>
        <span class="t-caption">Muốn mượn ở phòng khác thì gửi phiếu khác.</span></div>
      <div class="list-gap">
        <div class="card"><span class="t-card">Laptop Dell Latitude 5420</span><span class="t-caption">Dell · Còn 5</span>
          <div class="row" style="align-items:center"><span class="t-label">Số lượng</span>
          <div class="stepper"><span class="sq">${ic("remove")}</span><span class="val">1</span><span class="sq">${ic("add")}</span></div></div></div>
        <div class="card"><span class="t-card">Loa di động JBL Charge 5</span><span class="t-caption">JBL · Còn 2</span>
          <div class="row" style="align-items:center"><span class="t-label">Số lượng</span>
          <div class="stepper"><span class="sq">${ic("remove")}</span><span class="val">0</span><span class="sq">${ic("add")}</span></div></div></div>
        <div class="card disabled"><div class="row"><span class="t-card">Máy chiếu Epson EB-X51</span>${nhan("Hết hàng", "danger")}</div>
          <span class="t-caption">Epson · Không chọn được số lượng</span>
          <span class="link" style="font-size:14px" data-di="dlg-dang-ky-cho">Đăng ký chờ</span></div>
      </div>
      <div class="field"><label>Thời gian mượn dự kiến (giờ) *</label><div class="input">2 <span class="muted">giờ</span></div></div>`,
    ghim: `<button class="btn primary" data-di="xem-lai">Xem lại</button>`,
  },
  {
    id: "soan-phieu-loi", ma: "SCR-SV-04", ten: "Soạn phiếu yêu cầu mượn", trangThai: "Thời gian vượt giới hạn; Xem lại vô hiệu",
    tab: null, tieuDe: "Đặt trước thiết bị",
    noiDung: `
      <div class="field"><label>Phòng thiết bị *</label><div class="input">Phòng thiết bị A1 · Cơ sở Linh Trung ${ic("expand")}</div></div>
      <div class="card"><span class="t-card">Laptop Dell Latitude 5420</span><span class="t-caption">Dell · Còn 5</span>
        <div class="row" style="align-items:center"><span class="t-label">Số lượng</span>
        <div class="stepper"><span class="sq">${ic("remove")}</span><span class="val">1</span><span class="sq">${ic("add")}</span></div></div></div>
      <div class="field"><label>Thời gian mượn dự kiến (giờ) *</label><div class="input error">12 <span class="muted">giờ</span></div>
        <span class="err">${ic("error", "sm")}Tối đa 8 giờ cho mỗi lần mượn</span></div>`,
    ghim: `<button class="btn primary" disabled>Xem lại</button>`,
  },

  // ---------------- SCR-SV-05 ----------------
  {
    id: "xem-lai", ma: "SCR-SV-05", ten: "Xem lại và gửi phiếu yêu cầu", trangThai: "Sẵn sàng gửi",
    tab: null, tieuDe: "Xem lại phiếu",
    noiDung: `
      <div class="card"><h2 class="t-card">Tóm tắt phiếu</h2>
        ${kv([["Phòng", "Phòng thiết bị A1 · Cơ sở Linh Trung"], ["Loại và số lượng", "Laptop Dell Latitude 5420 × 1"], ["Thời gian mượn dự kiến", "2 giờ"]])}
      </div>`,
    ghim: `<button class="btn secondary" data-di="soan-phieu">Sửa</button><button class="btn primary" data-di="chi-tiet-phieu-moi">Gửi</button>`,
  },
  {
    id: "xem-lai-loi", ma: "SCR-SV-05", ten: "Xem lại và gửi phiếu yêu cầu", trangThai: "Lỗi SV_THE_HET_HAN: không gửi được",
    tab: null, tieuDe: "Xem lại phiếu",
    noiDung: `
      ${banner("danger", "Không gửi được phiếu", "Thẻ sinh viên đã hết hạn. Liên hệ phòng công tác sinh viên để gia hạn thẻ.")}
      <div class="card"><h2 class="t-card">Tóm tắt phiếu</h2>
        ${kv([["Phòng", "Phòng thiết bị A1 · Cơ sở Linh Trung"], ["Loại và số lượng", "Laptop Dell Latitude 5420 × 1"], ["Thời gian mượn dự kiến", "2 giờ"]])}
      </div>`,
    ghim: `<button class="btn secondary" data-di="soan-phieu">Sửa</button><button class="btn primary" disabled>Gửi</button>`,
  },

  // ---------------- SCR-SV-06 ----------------
  {
    id: "ds-phieu", ma: "SCR-SV-06", ten: "Danh sách phiếu yêu cầu của tôi", trangThai: "Có dữ liệu (mới nhất trước)",
    tab: null, tieuDe: "Phiếu yêu cầu của tôi",
    noiDung: `
      <button class="btn primary full" data-di="soan-phieu">${ic("add")}Tạo phiếu mới</button>
      <div class="list-gap">
        <div class="card tap" data-di="chi-tiet-phieu-duyet"><div class="row"><span class="t-card"><span class="t-code">YC-000123</span> · Phòng A1</span>${nhan("Đã duyệt", "success")}</div><span class="t-caption">Gửi: 22/11/2026 08:15</span></div>
        <div class="card tap" data-di="chi-tiet-phieu-moi"><div class="row"><span class="t-card"><span class="t-code">YC-000121</span> · Phòng A1</span>${nhan("Chờ duyệt", "warning")}</div><span class="t-caption">Gửi: 21/11/2026 14:02</span></div>
        <div class="card tap"><div class="row"><span class="t-card"><span class="t-code">YC-000120</span> · Phòng B2</span>${nhan("Từ chối", "danger")}</div><span class="t-caption">Gửi: 20/11/2026 09:40</span></div>
        <div class="card tap"><div class="row"><span class="t-card"><span class="t-code">YC-000117</span> · Phòng B2</span>${nhan("Hết hạn", "neutral")}</div><span class="t-caption">Gửi: 18/11/2026 16:55</span></div>
      </div>`,
  },
  {
    id: "ds-phieu-rong", ma: "SCR-SV-06", ten: "Danh sách phiếu yêu cầu của tôi", trangThai: "Rỗng",
    tab: null, tieuDe: "Phiếu yêu cầu của tôi",
    noiDung: `<button class="btn primary full">${ic("add")}Tạo phiếu mới</button>${rong("inbox", "Bạn chưa gửi phiếu yêu cầu nào")}`,
  },

  // ---------------- SCR-SV-07 ----------------
  {
    id: "chi-tiet-phieu-moi", ma: "SCR-SV-07", ten: "Chi tiết phiếu yêu cầu mượn", trangThai: "Vừa gửi: khối lưu ý ở đầu",
    tab: null, tieuDe: "Phiếu yêu cầu",
    noiDung: `
      <div class="row" style="display:flex;justify-content:space-between;align-items:center"><span class="t-section t-code" style="font-size:20px">YC-000124</span>${nhan("Chờ duyệt", "warning")}</div>
      <div class="banner tone-info">${ic("info")}<div><p class="t-card" style="margin:0">Lưu ý</p><ol style="margin:4px 0 0;padding-left:18px">
        <li>Phiếu chưa giữ chỗ thiết bị.</li><li>Nhân viên sẽ duyệt trong ngày.</li><li>Được duyệt: bạn nhận thông báo và thời gian giữ chỗ (mặc định 30 phút).</li>
        <li>Chưa được duyệt trong ngày: phiếu tự hết hiệu lực, không tính vi phạm.</li><li>Hạn trả do hệ thống tính khi giao thiết bị.</li><li>Ứng dụng chỉ để đặt trước; nhận thiết bị tại quầy.</li></ol></div></div>
      ${kv([["Phòng", "Phòng thiết bị A1 · Cơ sở Linh Trung"], ["Loại", "Laptop Dell Latitude 5420 × 1"], ["Thời gian dự kiến", "2 giờ"], ["Thời gian gửi", "22/11/2026 09:30"]])}
      <div class="card" style="background:var(--surface-muted)"><p class="t-label" style="margin:0">Chờ nhân viên duyệt trong ngày; chưa giữ chỗ.</p></div>`,
  },
  {
    id: "chi-tiet-phieu-duyet", ma: "SCR-SV-07", ten: "Chi tiết phiếu yêu cầu mượn", trangThai: "Đã duyệt: đếm ngược hạn giữ chỗ",
    tab: null, tieuDe: "Phiếu yêu cầu",
    noiDung: `
      <div style="display:flex;justify-content:space-between;align-items:center"><span class="t-code" style="font-size:20px;font-weight:500">YC-000123</span>${nhan("Đã duyệt", "success")}</div>
      ${kv([["Phòng", "Phòng thiết bị A1 · Cơ sở Linh Trung"], ["Loại", "Laptop Dell Latitude 5420 × 1"], ["Thời gian dự kiến", "2 giờ"], ["Thời gian gửi", "22/11/2026 08:15"]])}
      <div class="banner tone-success">${ic("check")}<div style="display:flex;flex-direction:column;gap:6px;width:100%">
        <span>Đã giữ chỗ lúc 22/11/2026 08:40</span><span>Hạn giữ chỗ: <b>22/11/2026 09:10</b></span>
        <span style="display:flex;justify-content:space-between;align-items:center">Còn lại <span class="countdown">00:24:10</span></span>
        <span>Đến quầy nhận thiết bị trước hạn.</span></div></div>`,
  },
  {
    id: "chi-tiet-phieu-tu-choi", ma: "SCR-SV-07", ten: "Chi tiết phiếu yêu cầu mượn", trangThai: "Từ chối: lý do và Tạo phiếu mới",
    tab: null, tieuDe: "Phiếu yêu cầu",
    noiDung: `
      <div style="display:flex;justify-content:space-between;align-items:center"><span class="t-code" style="font-size:20px;font-weight:500">YC-000120</span>${nhan("Từ chối", "danger")}</div>
      ${kv([["Phòng", "Phòng thiết bị B2 · Cơ sở Thủ Đức"], ["Loại", "Máy chiếu Epson EB-X51 × 2"], ["Thời gian dự kiến", "4 giờ"], ["Thời gian gửi", "20/11/2026 09:40"]])}
      ${banner("danger", "Lý do từ chối", "Phòng B2 đang bảo trì máy chiếu trong tuần này.")}
      <button class="btn secondary full" data-di="soan-phieu">Tạo phiếu mới</button>`,
  },

  // ---------------- SCR-SV-08 ----------------
  {
    id: "ds-cho", ma: "SCR-SV-08", ten: "Danh sách đăng ký chờ của tôi", trangThai: "Đang chờ và Đã thông báo",
    tab: null, tieuDe: "Đăng ký chờ của tôi",
    noiDung: `
      <div class="list-gap">
        <div class="card"><div class="row"><span class="t-card">Máy chiếu Epson EB-X51 · B2</span>${nhan("Đang chờ", "info")}</div>
          <span class="t-caption">Đăng ký: 22/11/2026 09:45</span>
          <div class="row" style="align-items:center"><span class="t-label">Số thứ tự chờ: 3</span><button class="btn tertiary" data-di="dlg-roi">Rời danh sách</button></div></div>
        <div class="card"><div class="row"><span class="t-card">Laptop Dell Latitude 5420 · A1</span>${nhan("Đã thông báo", "success")}</div>
          <span class="t-label">Thiết bị đã sẵn sàng</span><span class="t-caption">Hạn giữ chỗ: 22/11/2026 10:20</span>
          <div class="row" style="align-items:center"><span class="t-caption">Còn lại</span><span class="countdown">00:20:05</span></div>
          <span class="t-caption">Đến quầy nhận trước hạn.</span>
          <div style="display:flex;justify-content:flex-end"><button class="btn tertiary">Từ chối nhận</button></div></div>
        <div class="card"><div class="row"><span class="t-card">Loa di động JBL Charge 5 · A1</span>${nhan("Hết hạn", "neutral")}</div>
          <span class="t-caption">Đăng ký: 15/11/2026 13:10</span></div>
      </div>`,
  },
  {
    id: "dlg-roi", ma: "SCR-SV-08", ten: "Danh sách đăng ký chờ của tôi", trangThai: "Hộp xác nhận Rời danh sách",
    tab: null, tieuDe: "Đăng ký chờ của tôi",
    noiDung: `<div class="card"><div class="row"><span class="t-card">Máy chiếu Epson EB-X51 · B2</span>${nhan("Đang chờ", "info")}</div><span class="t-label">Số thứ tự chờ: 3</span></div>`,
    lopPhu: `
      <div class="scrim"><div class="dialog" role="dialog">
        <h2 class="t-section">Rời danh sách chờ?</h2>
        <p class="t-body" style="margin:0">Máy chiếu Epson EB-X51 · Phòng thiết bị B2 · Số thứ tự 3</p>
        <p class="muted" style="margin:0;font-size:14px;line-height:20px">Bạn sẽ mất vị trí trong danh sách. Rời danh sách không tính vi phạm.</p>
        <div class="actions"><button class="btn tertiary" data-di="ds-cho">Hủy</button><button class="btn destructive" data-di="ds-cho">Rời danh sách</button></div>
      </div></div>`,
  },

  // ---------------- SCR-SV-09, 10 ----------------
  {
    id: "lich-su", ma: "SCR-SV-09", ten: "Lịch sử mượn/trả", trangThai: "Có dữ liệu; phiếu Quá hạn nhấn mạnh",
    tab: null, tieuDe: "Lịch sử mượn/trả",
    noiDung: `
      <div class="list-gap">
        <div class="card tap"><div class="row"><span class="t-card t-code">PM-000045</span>${nhan("Đang mượn", "info")}</div>
          <span class="t-caption">Mượn: 22/11/2026 08:50</span><span class="t-caption">Hẹn trả: 22/11/2026 10:50 · 2 thiết bị</span></div>
        <div class="card tap" data-di="chi-tiet-pm"><div class="row"><span class="t-card t-code">PM-000039</span>${nhan("Quá hạn", "danger", true)}</div>
          <span class="t-caption">Mượn: 17/11/2026 13:00</span><span class="t-caption">Hẹn trả: 17/11/2026 17:00 · 1 thiết bị</span></div>
        <div class="card tap"><div class="row"><span class="t-card t-code">PM-000031</span>${nhan("Đã trả", "success")}</div>
          <span class="t-caption">Mượn: 10/11/2026 09:00</span><span class="t-caption">Hẹn trả: 10/11/2026 11:00 · 1 thiết bị</span></div>
      </div>`,
  },
  {
    id: "chi-tiet-pm", ma: "SCR-SV-10", ten: "Chi tiết phiếu mượn", trangThai: "Quá hạn, có biên bản",
    tab: null, tieuDe: "Chi tiết phiếu mượn",
    noiDung: `
      <div style="display:flex;justify-content:space-between;align-items:center"><span class="t-code" style="font-size:20px;font-weight:500">PM-000039</span>${nhan("Quá hạn", "danger", true)}</div>
      ${kv([["Mượn", "17/11/2026 13:00"], ["Hẹn trả", "17/11/2026 17:00"], ["Số lần gia hạn", "0"]])}
      <div class="card"><h3 class="t-card">Laptop Dell Latitude 5420</h3><span class="t-caption"><span class="t-code">LT-001</span> · Laptop</span>
        <div class="divider"></div><span class="t-label">Lúc giao</span><span class="t-body" style="font-size:14px">Máy hoạt động tốt, vỏ có vết xước nhỏ ở góc.</span>
        <div class="photos"><div class="photo">${ic("image", "lg")}</div><div class="photo">${ic("image", "lg")}</div></div>
        <div class="divider"></div>
        <div class="row" style="align-items:center"><span class="t-label">Kết quả kiểm tra</span>${nhan("Hư hỏng", "danger")}</div>
        <span class="t-label">Lúc trả</span><span class="t-body" style="font-size:14px">Bản lề màn hình bị lỏng.</span>
        <div class="photos"><div class="photo">${ic("image", "lg")}</div></div>
        <span class="t-caption">Trả thực tế: 20/11/2026 09:15</span></div>
      <div class="card"><div class="row"><span class="t-card">Biên bản vi phạm</span>${nhan("Chưa nộp", "warning")}</div>
        <span class="t-caption"><span class="t-code">BB-000012</span> · 20/11/2026</span>
        <div class="row" style="align-items:baseline"><span class="t-label muted">Tổng tiền</span><span class="t-num" style="font-size:24px">240.000 VNĐ</span></div>
        <span class="t-caption">Nộp tiền trực tiếp cho nhân viên tại quầy.</span>
        <ul class="t-caption" style="margin:0;padding-left:18px"><li>Laptop · Trả trễ · 3 ngày · 60.000 VNĐ</li><li>Laptop · Hư hỏng · 180.000 VNĐ</li></ul></div>`,
  },

  // ---------------- SCR-SV-11, 12 ----------------
  {
    id: "bien-ban", ma: "SCR-SV-11", ten: "Biên bản và phí phạt", trangThai: "Có dữ liệu; một thẻ mở rộng",
    tab: null, tieuDe: "Biên bản và phí phạt",
    noiDung: `
      <div class="list-gap">
        <div class="card"><div class="row"><span class="t-card"><span class="t-code">BB-000012</span> · <span class="link t-code" data-di="chi-tiet-pm">PM-000039</span></span>${nhan("Chưa nộp", "warning")}</div>
          <span class="t-caption">Lập: 20/11/2026 · 240.000 VNĐ</span><span class="t-caption">Nộp tiền trực tiếp cho nhân viên tại quầy.</span>
          <button class="btn tertiary" style="align-self:flex-start">Ẩn các khoản ${ic("expand", "sm")}</button>
          <ul class="t-caption" style="margin:0;padding-left:18px"><li>Laptop Dell Latitude 5420 · Trả trễ 3 ngày · 60.000 VNĐ</li><li>Laptop Dell Latitude 5420 · Hư hỏng · 180.000 VNĐ</li></ul></div>
        <div class="card"><div class="row"><span class="t-card"><span class="t-code">BB-000010</span> · <span class="link t-code">PM-000031</span></span>${nhan("Đã nộp", "success")}</div>
          <span class="t-caption">Lập: 11/11/2026 · 80.000 VNĐ</span><span class="t-caption">Nộp: 11/11/2026 15:20</span>
          <button class="btn tertiary" style="align-self:flex-start">Xem các khoản ${ic("expand", "sm")}</button></div>
      </div>`,
  },
  {
    id: "vi-pham", ma: "SCR-SV-12", ten: "Danh sách vi phạm", trangThai: "Có dữ liệu",
    tab: null, tieuDe: "Vi phạm",
    noiDung: `
      <div class="list-gap">
        <div class="card"><div class="row">${nhan("Trả trễ", "warning")}<span class="t-caption">20/11/2026 09:15</span></div>
          <div class="row"><span class="t-caption">Học kỳ 1, 2026–2027</span><span class="link t-code" style="font-size:14px" data-di="bien-ban">BB-000012</span></div></div>
        <div class="card"><div class="row">${nhan("Không đến nhận", "warning")}<span class="t-caption">12/11/2026 17:00</span></div>
          <div class="row"><span class="t-caption">Học kỳ 1, 2026–2027</span><span class="t-caption">Không có biên bản</span></div>
          <span class="t-caption">Tham chiếu: <span class="t-code">YC-000109</span></span></div>
      </div>`,
  },

  // ---------------- SCR-SV-13 ----------------
  {
    id: "thong-bao", ma: "SCR-SV-13", ten: "Thông báo", trangThai: "Có dữ liệu (mới nhất trước)",
    tab: "thong-bao", tieuDe: "Thông báo",
    noiDung: `
      <div class="list-gap">
        <div class="card tap" data-di="chi-tiet-phieu-duyet"><div class="row">${nhan("Duyệt hoặc từ chối", "info")}<span class="t-caption">08:40</span></div>
          <span class="t-body" style="font-size:14px;line-height:20px">Phiếu YC-000123 đã được duyệt. Đến quầy phòng A1 nhận thiết bị trước 09:10.</span></div>
        <div class="card tap" data-di="lich-su"><div class="row">${nhan("Nhắc hạn trả", "warning")}<span class="t-caption">22/11/2026 07:00</span></div>
          <span class="t-body" style="font-size:14px;line-height:20px">Phiếu PM-000045 đến hạn trả lúc 10:50 hôm nay.</span></div>
        <div class="card tap" data-di="ds-cho"><div class="row">${nhan("Thiết bị chờ sẵn sàng", "success")}<span class="t-caption">21/11/2026 16:20</span></div>
          <span class="t-body" style="font-size:14px;line-height:20px">Laptop Dell Latitude 5420 tại phòng A1 đã sẵn sàng cho bạn.</span></div>
        <div class="card tap" data-di="bien-ban"><div class="row">${nhan("Vi phạm, phí phạt", "danger")}<span class="t-caption">20/11/2026 09:20</span></div>
          <span class="t-body" style="font-size:14px;line-height:20px">Biên bản BB-000012: 240.000 VNĐ chưa nộp.</span></div>
        <div class="card"><div class="row">${nhan("Nhắc nhở của NV", "info")}<span class="t-caption">19/11/2026 10:00</span></div>
          <span class="t-body" style="font-size:14px;line-height:20px">Vui lòng mang thẻ sinh viên khi đến nhận thiết bị.</span></div>
      </div>`,
  },
  {
    id: "thong-bao-rong", ma: "SCR-SV-13", ten: "Thông báo", trangThai: "Rỗng",
    tab: "thong-bao", tieuDe: "Thông báo",
    noiDung: rong("bell", "Bạn chưa có thông báo nào"),
  },

  // ---------------- SCR-SV-14 ----------------
  {
    id: "ho-so", ma: "SCR-SV-14", ten: "Hồ sơ mượn", trangThai: "Mở từ thanh điều hướng, quyền Bình thường",
    tab: "ho-so", tieuDe: "Hồ sơ mượn",
    noiDung: `
      <div class="card"><h2 class="t-card">Thông tin sinh viên</h2>
        ${kv([["Sinh viên", `<span class="t-code">22520001</span> · Nguyễn Văn An`], ["Lớp", "KTPM2022.1"], ["Khoa", "Công nghệ phần mềm"], ["Nhóm", "Sinh viên chính quy"], ["Hạn thẻ", `31/08/2027 ${nhan("Đang học", "success")}`]])}</div>
      <div class="card"><h2 class="t-card">Quyền mượn</h2>
        <div class="row"><span class="t-label muted">Quyền mượn</span>${nhan("Bình thường", "success")}</div>
        <div class="row"><span class="t-label muted">Đang mượn</span><span class="t-label">1 / 3</span></div></div>
      <div class="card"><h2 class="t-card">Hướng dẫn</h2><p class="t-body" style="margin:0;font-size:14px;line-height:20px">Bạn có thể đặt trước và đăng ký chờ thiết bị.</p></div>
      <button class="btn secondary full" data-di="dang-nhap">Đăng xuất</button>`,
  },
  {
    id: "ho-so-chan", ma: "SCR-SV-14", ten: "Hồ sơ mượn", trangThai: "Chế độ \"bị chặn\" (đến từ Đặt trước)",
    tab: "ho-so", tieuDe: "Hồ sơ mượn", quayLai: true,
    noiDung: `
      ${banner("warning", "Đặt trước không thực hiện được", "Quyền mượn của bạn đang Tạm khóa.")}
      <div class="card"><h2 class="t-card">Thông tin sinh viên</h2>
        ${kv([["Sinh viên", `<span class="t-code">22520001</span> · Nguyễn Văn An`], ["Lớp", "KTPM2022.1"], ["Hạn thẻ", `31/08/2027 ${nhan("Đang học", "success")}`]])}</div>
      <div class="card"><h2 class="t-card">Quyền mượn</h2>
        <div class="row"><span class="t-label muted">Quyền mượn</span>${nhan("Tạm khóa", "warning", true)}</div>
        <div class="row"><span class="t-label muted">Đang mượn</span><span class="t-label">1 / 3</span></div></div>
      <div class="card"><h2 class="t-card">Biên bản chưa nộp</h2>
        <span class="t-body" style="font-size:14px"><span class="t-code">BB-000012</span> · 240.000 VNĐ · 20/11/2026</span>
        <span class="link" style="font-size:14px" data-di="bien-ban">Xem biên bản</span></div>
      <div class="card"><h2 class="t-card">Lý do không được mượn</h2>
        <ul class="t-body" style="margin:0;padding-left:20px;font-size:14px;line-height:20px"><li>Còn biên bản vi phạm chưa nộp tiền</li><li>Đang có phiếu mượn quá hạn</li></ul></div>
      <div class="card"><h2 class="t-card">Hướng dẫn mở lại</h2><p class="t-body" style="margin:0;font-size:14px;line-height:20px">Trả thiết bị quá hạn và nộp tiền biên bản tại quầy; quyền mượn tự mở lại sau khi hoàn tất.</p></div>
      <button class="btn secondary full" data-di="dang-nhap">Đăng xuất</button>`,
  },
];

const TAB = [
  { id: "trang-chu", nhan: "Trang chủ", icon: "home" },
  { id: "tra-cuu", nhan: "Tra cứu", icon: "search" },
  { id: "thong-bao", nhan: "Thông báo", icon: "bell" },
  { id: "ho-so", nhan: "Hồ sơ", icon: "person" },
];

/** Dựng HTML khung điện thoại cho một màn hình. */
function dungManHinh(m) {
  const appbar = m.khongAppbar
    ? ""
    : `<header class="appbar">${m.tab === null || m.quayLai ? `<span class="back" data-quay-lai>${ic("back", "lg")}</span>` : ""}<h1>${m.tieuDe}</h1></header>`;
  const nav =
    m.tab === null
      ? ""
      : `<nav class="bottomnav">${TAB.map(
          (t) => `<a class="${t.id === m.tab ? "on" : ""}" data-di="${t.id}"><span class="pill">${ic(t.icon, "lg")}</span>${t.nhan}</a>`,
        ).join("")}</nav>`;
  return `<div class="phone" data-man="${m.id}">
    <div class="statusbar"><span>9:41</span><span>●●● ▮</span></div>
    ${appbar}
    <main class="content">${m.noiDung}</main>
    ${m.ghim ? `<div class="pinned">${m.ghim}</div>` : ""}
    ${nav}
    ${m.lopPhu ?? ""}
  </div>`;
}
