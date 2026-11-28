"use client";

// Khung chung của bốn bước Giao thiết bị (Wireframes SCR-NV-04..07, P-08): tiêu đề "Giao thiết bị",
// thanh tiến trình (CMP-31) ngay dưới tiêu đề, rồi nội dung bước. Sở hữu: Thịnh.
import type { ReactNode } from "react";
import { Stepper } from "@/shared/components/Stepper";
import { BAN_DO_TUYEN } from "@/shared/tuyen/banDoTuyen";
import { useLuongGiaoThietBi } from "../LuongGiaoThietBi";
import { NHAN_BUOC } from "../thongDiep";

export const TUYEN_BUOC = [
  BAN_DO_TUYEN["SCR-NV-04"].tuyen,
  BAN_DO_TUYEN["SCR-NV-05"].tuyen,
  BAN_DO_TUYEN["SCR-NV-06"].tuyen,
  BAN_DO_TUYEN["SCR-NV-07"].tuyen,
] as const;

const DS_BUOC = NHAN_BUOC.map((nhan, i) => ({ nhan, href: TUYEN_BUOC[i] }));

export type SoBuoc = 1 | 2 | 3 | 4;

export function KhungBuocGiao({
  buoc,
  children,
}: {
  buoc: SoBuoc;
  children: ReactNode;
}) {
  const { danhDauQuayLai } = useLuongGiaoThietBi();
  return (
    <main className="flex flex-col gap-section">
      <h1 className="type-title-screen text-text-primary">Giao thiết bị</h1>
      <Stepper
        tenThanh="Các bước giao thiết bị"
        dsBuoc={DS_BUOC}
        buocHienTai={buoc}
        onQuayLai={danhDauQuayLai}
      />
      {children}
    </main>
  );
}
