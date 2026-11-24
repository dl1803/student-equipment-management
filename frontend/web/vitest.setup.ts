import { afterEach } from "vitest";

// jsdom chưa hiện thực showModal()/close() của <dialog>; bổ sung bản tối thiểu (đặt thuộc tính
// open, phát sự kiện close) để kiểm thử Dialog/ConfirmDialog. Trình duyệt thật dùng bản gốc.
if (
  typeof HTMLDialogElement !== "undefined" &&
  typeof HTMLDialogElement.prototype.showModal !== "function"
) {
  HTMLDialogElement.prototype.showModal = function showModal(
    this: HTMLDialogElement,
  ) {
    this.setAttribute("open", "");
  };
  HTMLDialogElement.prototype.close = function close(this: HTMLDialogElement) {
    this.removeAttribute("open");
    this.dispatchEvent(new Event("close"));
  };
}

// Testing Library chỉ tự dọn DOM khi test runner bật globals; Vitest ở đây không bật nên dọn tay.
afterEach(async () => {
  if (typeof document === "undefined") return;
  const { cleanup } = await import("@testing-library/react");
  cleanup();
});
