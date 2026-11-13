"use client";

// Hợp đồng đọc phiên cho mọi tính năng (FC 4.1, 5.10). Sở hữu: Thịnh (shared core).
// AuthProvider ở features/xac-thuc/ cung cấp giá trị; tính năng khác chỉ đọc qua useAuth,
// không import AuthProvider và không tự chạm token.
import { createContext, useContext } from "react";
import type { TrangThaiPhien } from "@/shared/api/session/trangThaiPhien";

export type GiaTriAuth = {
  trangThai: TrangThaiPhien;
  /** Gọi POST /auth/dang-xuat, bỏ qua kết quả, xóa phiên và bộ nhớ đệm, về /dang-nhap (FA 3.5). */
  dangXuat: () => Promise<void>;
  /** Tải lại ThongTinToi, ví dụ khi gặp PHONG_CHUA_CAP_QUYEN (FA 3.4, 4.4). */
  taiLaiThongTinToi: () => Promise<void>;
};

export const AuthContext = createContext<GiaTriAuth | null>(null);

export function useAuth(): GiaTriAuth {
  const giaTri = useContext(AuthContext);
  if (giaTri === null) {
    throw new Error("useAuth phải được dùng bên trong AuthProvider");
  }
  return giaTri;
}
