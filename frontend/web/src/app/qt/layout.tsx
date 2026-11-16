// Layout /qt: khung quản trị viên, chỉ cho QUAN_TRI (FA 4.4; UI-Screens 4.3). Sở hữu: Thịnh.
import type { ReactNode } from "react";
import { KhungVaiTro } from "@/features/xac-thuc/KhungVaiTro";

export default function Layout({ children }: { children: ReactNode }) {
  return <KhungVaiTro vaiTro="QUAN_TRI">{children}</KhungVaiTro>;
}
