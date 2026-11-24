"use client";

// Hợp đồng đọc phiên cho mọi tính năng (FC 4.1, 5.10). Sở hữu: Thịnh (shared core).
// AuthProvider ở features/xac-thuc/ cung cấp giá trị; tính năng khác chỉ đọc qua useAuth,
// không import AuthProvider và không tự chạm token. Hợp đồng ổn định từ D04-T.
import { createContext, useContext } from "react";
import type { ApiError } from "@/shared/api/apiError";
import type { TrangThaiPhien } from "@/shared/api/session/trangThaiPhien";
import type { ThongTinToi } from "@/shared/api/types";

export type GiaTriAuth = {
  trangThai: TrangThaiPhien;
  /**
   * Lỗi của GET /auth/toi khi đã có token (khôi phục phiên hoặc tải lại). SCR-CM-03 hiển thị kèm
   * Thử lại; không đăng xuất vì phiên vẫn hợp lệ (FA 4.5, sơ đồ khôi phục).
   */
  loiTaiThongTin: ApiError | null;
  /** Gọi POST /auth/dang-xuat, bỏ qua kết quả, xóa phiên và bộ nhớ đệm, về /dang-nhap (FA 3.5). */
  dangXuat: () => Promise<void>;
  /** Tải lại ThongTinToi, ví dụ khi gặp PHONG_CHUA_CAP_QUYEN hoặc sau lỗi tải (FA 3.4, 4.4). */
  taiLaiThongTinToi: () => Promise<void>;
  /** Dùng ở SCR-CM-02 sau khi đăng nhập và GET /auth/toi thành công: xóa cache, vào phiên. */
  hoanTatDangNhap: (nguoiDung: ThongTinToi) => void;
  /**
   * Tuyến đang phải hiện banner "Bạn không có quyền thực hiện thao tác này" sau khi bị chuyển
   * về trang chủ vì sai vai trò (FA 4.4); null khi không có.
   */
  tuyenBaoKhongDuQuyen: string | null;
  /** Đánh dấu banner không đủ quyền cho tuyến đích sắp chuyển tới; null để xóa dấu khi rời tuyến. */
  baoKhongDuQuyen: (tuyenDich: string | null) => void;
};

export const AuthContext = createContext<GiaTriAuth | null>(null);

export function useAuth(): GiaTriAuth {
  const giaTri = useContext(AuthContext);
  if (giaTri === null) {
    throw new Error("useAuth phải được dùng bên trong AuthProvider");
  }
  return giaTri;
}
