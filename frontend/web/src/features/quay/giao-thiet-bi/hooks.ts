"use client";

// Truy vấn của luồng Giao thiết bị (FA 4.7; FC 5.4). Sở hữu: Thịnh.
// Mặc định QueryClient: staleTime 0, retry 0. Quét thẻ là GET không tác dụng phụ nhưng do NV chủ động
// gọi (Enter), nên dùng fetchQuery thay vì truy vấn tự chạy theo state.
import { useQueryClient } from "@tanstack/react-query";
import { useCallback, useRef } from "react";
import type { SinhVienTheInfo } from "@/shared/api/types";
import { khoaGiaoThietBi, quayLayThongTinSinhVien } from "./api";

/** Tra cứu SV theo mã thẻ; mỗi lần gọi là một lần tra cứu mới (không dùng kết quả cũ). */
export function useTraCuuSinhVien(): (
  maThe: string,
) => Promise<SinhVienTheInfo> {
  const queryClient = useQueryClient();
  const lan = useRef(0);
  return useCallback(
    (maThe: string) => {
      lan.current += 1;
      return queryClient.fetchQuery({
        queryKey: khoaGiaoThietBi.sinhVienTheoThe(maThe, lan.current),
        queryFn: ({ signal }) => quayLayThongTinSinhVien(maThe, { signal }),
        gcTime: 0,
      });
    },
    [queryClient],
  );
}
