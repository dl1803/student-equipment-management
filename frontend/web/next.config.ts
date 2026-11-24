import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  cacheComponents: true,
  partialPrefetching: true,
  experimental: {
    // Mọi tuyến bị AuthGate chặn tới khi khôi phục xong phiên trong trình duyệt (ĐX-FA-01, FA 4.4)
    // nên không đoạn nào dựng được trước khi biết phiên. Chỉ kiểm tra "instant navigation" ở đoạn
    // khai báo `instant` rõ ràng, thay vì mọi page (cảnh báo khi dev, không ảnh hưởng build).
    instantInsights: { validationLevel: "manual-warning" },
  },
  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
  // ĐX-FA-21, FC 5.12: ba tiêu đề bảo mật cơ bản, không CSP chặt. Không rewrites/redirects tới API.
  async headers() {
    return [
      {
        source: "/:path*",
        headers: [
          { key: "X-Content-Type-Options", value: "nosniff" },
          { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
          { key: "X-Frame-Options", value: "DENY" },
        ],
      },
    ];
  },
};

export default nextConfig;
