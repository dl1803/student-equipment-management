"use client";

// Điều kiện vào các bước 2–4 của Giao thiết bị (FA 4.3, 4.7; FC "Ngữ cảnh luồng"). Sở hữu: Thịnh.
// - Thiếu dữ liệu của bước trước (tải lại trang, mở thẳng đường dẫn) → router.replace về bước 1.
// - SV không hợp lệ không có đường tới bước 2 (UI-Screens 6.4 E1): hopLe do máy chủ trả.
// - Đã lập phiếu → mở lại bước 2, 3 chuyển về bước 4 (FA 4.7) cho tới khi NV đặt lại luồng.
// Bước 2 trở đi bỏ dấu "quay lại" để lần vào bước 1 kế tiếp không qua thanh tiến trình được đặt lại.
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { TUYEN_BUOC, type SoBuoc } from "./components/KhungBuocGiao";
import { useLuongGiaoThietBi } from "./LuongGiaoThietBi";

/** Trả true khi bước được phép hiển thị; false thì đang chuyển hướng, màn hình không vẽ gì. */
export function useBaoVeBuoc(buoc: Exclude<SoBuoc, 1>): boolean {
  const router = useRouter();
  const { sinhVien, nguon, phieuDaLap, boDauQuayLai } = useLuongGiaoThietBi();

  let dich: string | null = null;
  if (buoc === 4) {
    if (phieuDaLap === null) dich = TUYEN_BUOC[0];
  } else if (phieuDaLap !== null) {
    dich = TUYEN_BUOC[3];
  } else if (sinhVien === null || sinhVien.hopLe !== true) {
    dich = TUYEN_BUOC[0];
  } else if (buoc === 3 && nguon === null) {
    dich = TUYEN_BUOC[0];
  }

  useEffect(() => {
    boDauQuayLai();
  }, [boDauQuayLai]);

  useEffect(() => {
    if (dich !== null) router.replace(dich);
  }, [dich, router]);

  return dich === null;
}
