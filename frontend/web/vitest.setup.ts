import { afterEach } from "vitest";

// Testing Library chỉ tự dọn DOM khi test runner bật globals; Vitest ở đây không bật nên dọn tay.
afterEach(async () => {
  if (typeof document === "undefined") return;
  const { cleanup } = await import("@testing-library/react");
  cleanup();
});
