// Layout gốc: Server Component chỉ khai báo metadata, phông và <html lang="vi"> (FC 4.1, 5.8, 8.2).
// Providers (Client Component) bọc toàn bộ ứng dụng; AuthProvider và AuthGate thêm ở task sau.
import type { Metadata } from "next";
import { Roboto, Roboto_Mono } from "next/font/google";
import type { ReactNode } from "react";
import { Providers } from "@/providers/Providers";
import "./globals.css";

const roboto = Roboto({
  subsets: ["vietnamese", "latin"],
  weight: ["400", "500", "700"],
  display: "swap",
  variable: "--font-roboto",
});

const robotoMono = Roboto_Mono({
  subsets: ["vietnamese", "latin"],
  weight: ["400"],
  display: "swap",
  variable: "--font-roboto-mono",
});

export const metadata: Metadata = {
  title: "Quản lý mượn trả thiết bị",
  description: "Web dành cho nhân viên và quản trị viên",
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="vi" className={`${roboto.variable} ${robotoMono.variable}`}>
      <body>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
