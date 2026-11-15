"use client";

// Gửi thao tác ghi nghiệp vụ an toàn (FA 3.11, 4.8; FC 5.4). Sở hữu: Thịnh (shared core).
// 1. Chống gửi trùng: cờ useRef đặt true ĐỒNG BỘ trong hàm xử lý bấm, trước mọi await, nên bấm
//    đúp nhanh hơn một lần vẽ lại vẫn chỉ gửi một yêu cầu.
// 2. Chưa biết kết quả: lỗi MANG/QUA_THOI_GIAN/HUY sau khi đã phát thao tác ghi không phải thành
//    công cũng không phải thất bại. Giữ khóa gửi cho tới khi màn hình kiểm tra trạng thái hiện tại
//    qua API đọc hiện hữu rồi gọi xacNhanDaKiemTra(); không coi là rollback, không gửi lại ngay.
import type { UseMutationResult } from "@tanstack/react-query";
import { useCallback, useRef, useState } from "react";
import { laChuaBietKetQua, type ApiError } from "@/shared/api/apiError";

export type ThaoTacGhi<TBien> = {
  /** Gửi; trả false khi bị chặn (đang gửi hoặc đang chờ kiểm tra kết quả chưa biết). */
  gui: (bien: TBien) => boolean;
  /** Vô hiệu nút gửi theo cờ này (FA 3.11). */
  biKhoa: boolean;
  dangGui: boolean;
  /** Hiển thị "Chưa xác định được kết quả" kèm hành động kiểm tra trạng thái hiện tại. */
  chuaBietKetQua: boolean;
  /** Gọi sau khi đã tải lại danh sách/chi tiết để kiểm tra; mở khóa gửi. */
  xacNhanDaKiemTra: () => void;
};

export function useThaoTacGhi<TData, TBien>(
  mutation: UseMutationResult<TData, ApiError, TBien>,
): ThaoTacGhi<TBien> {
  const khoaRef = useRef(false);
  const [chuaBietKetQua, setChuaBietKetQua] = useState(false);
  const { mutate, isPending } = mutation;

  const gui = useCallback(
    (bien: TBien) => {
      if (khoaRef.current) return false;
      khoaRef.current = true;
      mutate(bien, {
        onSuccess: () => {
          khoaRef.current = false;
        },
        onError: (loi) => {
          if (laChuaBietKetQua(loi)) {
            setChuaBietKetQua(true); // Giữ khóa: chỉ mở sau xacNhanDaKiemTra().
          } else {
            khoaRef.current = false;
          }
        },
      });
      return true;
    },
    [mutate],
  );

  const xacNhanDaKiemTra = useCallback(() => {
    khoaRef.current = false;
    setChuaBietKetQua(false);
  }, []);

  return {
    gui,
    biKhoa: isPending || chuaBietKetQua,
    dangGui: isPending,
    chuaBietKetQua,
    xacNhanDaKiemTra,
  };
}
