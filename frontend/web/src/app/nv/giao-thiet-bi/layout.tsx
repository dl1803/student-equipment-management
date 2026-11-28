// Layout /nv/giao-thiet-bi. Đặt ngữ cảnh LuongGiaoThietBi (FA 4.7) cho bốn bước SCR-NV-04..07.
// Sở hữu: Thịnh.
import type { ReactNode } from "react";
import { LuongGiaoThietBiProvider } from "@/features/quay/giao-thiet-bi/LuongGiaoThietBi";

export default function Layout({ children }: { children: ReactNode }) {
  return <LuongGiaoThietBiProvider>{children}</LuongGiaoThietBiProvider>;
}
