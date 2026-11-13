import { fileURLToPath } from "node:url";
import { defineConfig } from "vitest/config";

export default defineConfig({
  resolve: {
    alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) },
  },
  test: {
    include: ["src/**/*.test.{ts,tsx}"],
    // Mặc định node; tệp kiểm thử component khai báo `// @vitest-environment jsdom` ở dòng đầu.
    environment: "node",
    setupFiles: ["./vitest.setup.ts"],
  },
});
