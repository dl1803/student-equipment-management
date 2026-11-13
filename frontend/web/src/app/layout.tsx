// Layout gốc: Server Component chỉ khai báo metadata, phông và <html lang="vi"> (FC 4.1, 5.8, 8.2).
// Providers (TanStack Query, AuthProvider) và AuthGate được thêm ở task phiên/khung ứng dụng.
import type { Metadata } from "next";
import { Roboto, Roboto_Mono } from "next/font/google";
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

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="vi" className={`${roboto.variable} ${robotoMono.variable}`}>
      <body>{children}</body>
    </html>
  );
}
