// Layout /nv: khung nhân viên, chỉ cho NHAN_VIEN (FA 4.4; UI-Screens 4.2). Sở hữu: Thịnh.
import type { ReactNode } from "react";
import { KhungVaiTro } from "@/features/xac-thuc/KhungVaiTro";

export default function Layout({ children }: { children: ReactNode }) {
  return <KhungVaiTro vaiTro="NHAN_VIEN">{children}</KhungVaiTro>;
}
