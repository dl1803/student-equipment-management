// Hàm API của xác thực web (FA 4.6; FC 5.4). Tên hàm trùng operationId. Sở hữu: Thịnh.
import { apiClient } from "@/shared/api/client";
import type {
  DangNhapNhanVienRequest,
  ThongTinToi,
  TokenResponse,
} from "@/shared/api/types";

export async function authDangNhapNhanVien(
  yeuCau: DangNhapNhanVienRequest,
): Promise<TokenResponse> {
  const res = await apiClient.post<TokenResponse>(
    "/auth/nhan-vien/dang-nhap",
    yeuCau,
  );
  return res.data;
}

export async function authToi(tuyChon?: {
  signal?: AbortSignal;
}): Promise<ThongTinToi> {
  const res = await apiClient.get<ThongTinToi>("/auth/toi", {
    signal: tuyChon?.signal,
  });
  return res.data;
}
