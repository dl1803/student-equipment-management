"use client";

// Hook của xác thực web (FA 4.5, 4.8; FC 5.4). Sở hữu: Thịnh.
import { useMutation } from "@tanstack/react-query";
import type { ApiError } from "@/shared/api/apiError";
import { dangXuat } from "@/shared/api/session/lamMoi";
import { ghiCapToken, xoaPhien } from "@/shared/api/session/phien";
import type { DangNhapNhanVienRequest, ThongTinToi } from "@/shared/api/types";
import { laVaiTroWeb } from "@/shared/tuyen/menuTheoVaiTro";
import { authDangNhapNhanVien, authToi } from "./api";

/**
 * Lỗi phía client khi tài khoản đăng nhập được nhưng không thuộc vai trò dùng web (NFR-01):
 * web chỉ dành cho NHAN_VIEN và QUAN_TRI. Không thêm mã mới vào hợp đồng API.
 */
export const LOI_SAI_VAI_TRO: ApiError = {
  loai: "HTTP",
  status: 403,
  maLoi: "KHONG_DU_QUYEN",
};

/**
 * Đăng nhập NV/QT (SCR-CM-02, FA 4.5): POST đăng nhập → ghi cặp token → GET /auth/toi.
 * gcTime 0 để mutation chứa mật khẩu được thu gom ngay khi không còn dùng; màn hình vẫn phải
 * reset() mutation sau khi lấy kết quả (FA 4.8, FC 5.4). Không tự retry.
 * - GET /auth/toi lỗi sau khi đã có token: xóa phiên vừa tạo để không còn phiên nửa vời.
 * - Vai trò không dùng web: thu hồi phiên vừa tạo rồi báo lỗi KHONG_DU_QUYEN qua onError, để màn
 *   hình hiển thị thông báo như mọi lỗi đăng nhập khác.
 */
export function useDangNhapNhanVienMutation() {
  return useMutation<ThongTinToi, ApiError, DangNhapNhanVienRequest>({
    mutationFn: async (yeuCau) => {
      ghiCapToken(await authDangNhapNhanVien(yeuCau));
      let nguoiDung: ThongTinToi;
      try {
        nguoiDung = await authToi();
      } catch (e) {
        xoaPhien();
        throw e;
      }
      if (!laVaiTroWeb(nguoiDung.vaiTro)) {
        await dangXuat();
        throw LOI_SAI_VAI_TRO;
      }
      return nguoiDung;
    },
    gcTime: 0,
  });
}
