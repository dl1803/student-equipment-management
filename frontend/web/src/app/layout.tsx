// Layout gốc: Server Component chỉ khai báo metadata, phông và <html lang="vi"> (FC 4.1, 5.8, 8.2).
// Providers → AuthProvider → AuthGate bọc mọi tuyến (FA 4.4): AuthGate quyết định khởi động,
// đăng nhập hay vào nội dung, nên không tuyến nào dựng trước khi biết trạng thái phiên.
import type { Metadata } from "next";
import { Roboto, Roboto_Mono } from "next/font/google";
import type { ReactNode } from "react";
import { AuthGate } from "@/features/xac-thuc/AuthGate";
import { AuthProvider } from "@/features/xac-thuc/AuthProvider";
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
        <Providers>
          <AuthProvider>
            <AuthGate>{children}</AuthGate>
          </AuthProvider>
        </Providers>
      </body>
    </html>
  );
}
