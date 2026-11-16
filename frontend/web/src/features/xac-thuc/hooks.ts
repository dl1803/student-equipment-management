"use client";

// Hook của xác thực web (FA 4.5, 4.8; FC 5.4). Sở hữu: Thịnh.
import { useMutation } from "@tanstack/react-query";
import type { ApiError } from "@/shared/api/apiError";
import { ghiCapToken, xoaPhien } from "@/shared/api/session/phien";
import type { DangNhapNhanVienRequest, ThongTinToi } from "@/shared/api/types";
import { authDangNhapNhanVien, authToi } from "./api";

/**
 * Đăng nhập NV/QT (SCR-CM-02, FA 4.5): POST đăng nhập → ghi cặp token → GET /auth/toi.
 * gcTime 0 để mutation chứa mật khẩu được thu gom ngay khi không còn dùng; màn hình vẫn phải
 * reset() mutation sau khi lấy kết quả (FA 4.8, FC 5.4). Không tự retry.
 * GET /auth/toi lỗi sau khi đã có token thì xóa phiên vừa tạo để không còn phiên nửa vời.
 */
export function useDangNhapNhanVienMutation() {
  return useMutation<ThongTinToi, ApiError, DangNhapNhanVienRequest>({
    mutationFn: async (yeuCau) => {
      ghiCapToken(await authDangNhapNhanVien(yeuCau));
      try {
        return await authToi();
      } catch (e) {
        xoaPhien();
        throw e;
      }
    },
    gcTime: 0,
  });
}
