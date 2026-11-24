"use client";

// Provider gốc (FA 4.2, FC 4.1). Sở hữu: Thịnh (shared core).
// Mỗi trình duyệt một QueryClient; bộ nhớ đệm bị xóa khi phiên kết thúc.
// AuthProvider và AuthGate (features/xac-thuc) được thêm ở task phiên giao diện.
import { QueryClientProvider, type QueryClient } from "@tanstack/react-query";
import { useEffect, type ReactNode } from "react";
import {
  noiXoaCacheKhiKetThucPhien,
  taoQueryClient,
} from "@/shared/api/queryClient";

let queryClientTrinhDuyet: QueryClient | undefined;

/**
 * Theo hướng dẫn TanStack Query của Next.js đi kèm dự án: máy chủ tạo client mới cho mỗi lần dựng,
 * trình duyệt dùng một client duy nhất. Không dùng useState vì React có thể bỏ trạng thái của lần
 * dựng đầu khi có thành phần bị treo (Suspense), làm mất bộ nhớ đệm.
 */
function layQueryClient(): QueryClient {
  if (typeof window === "undefined") return taoQueryClient();
  queryClientTrinhDuyet ??= taoQueryClient();
  return queryClientTrinhDuyet;
}

export function Providers({ children }: { children: ReactNode }) {
  const queryClient = layQueryClient();

  useEffect(() => noiXoaCacheKhiKetThucPhien(queryClient), [queryClient]);

  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}
