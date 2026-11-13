"use client";

// Sau 8 giây chờ chưa có phản hồi thì báo "Hệ thống đang khởi động" (ĐX-FA-04, FC 5.3, 7.1).
// Sở hữu: Thịnh (shared core).
import { useEffect, useState } from "react";

export const NGUONG_THONG_BAO_KHOI_DONG_MS = 8_000;
export const THONG_DIEP_KHOI_DONG = "Hệ thống đang khởi động, vui lòng chờ";

/** Trả true khi đã chờ quá ngưỡng trong lúc `dangCho` vẫn true; về false ngay khi thôi chờ. */
export function useThongBaoKhoiDong(dangCho: boolean): boolean {
  const [quaNguong, setQuaNguong] = useState(false);

  useEffect(() => {
    if (!dangCho) return;
    const hen = setTimeout(
      () => setQuaNguong(true),
      NGUONG_THONG_BAO_KHOI_DONG_MS,
    );
    return () => {
      clearTimeout(hen);
      setQuaNguong(false);
    };
  }, [dangCho]);

  return dangCho && quaNguong;
}
