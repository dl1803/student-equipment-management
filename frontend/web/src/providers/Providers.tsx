"use client";

// Provider gốc (FA 4.2, FC 4.1). Sở hữu: Thịnh (shared core).
// Mỗi trình duyệt một QueryClient; bộ nhớ đệm bị xóa khi phiên kết thúc.
// AuthProvider và AuthGate (features/xac-thuc) được thêm ở task phiên giao diện.
import { QueryClientProvider } from "@tanstack/react-query";
import { useEffect, useState, type ReactNode } from "react";
import {
  noiXoaCacheKhiKetThucPhien,
  taoQueryClient,
} from "@/shared/api/queryClient";

export function Providers({ children }: { children: ReactNode }) {
  const [queryClient] = useState(taoQueryClient);

  useEffect(() => noiXoaCacheKhiKetThucPhien(queryClient), [queryClient]);

  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}
