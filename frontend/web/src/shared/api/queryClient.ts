// QueryClient mặc định của web (FA 3.6, 4.7; FC 5.4). Sở hữu: Thịnh (shared core).
import { QueryClient } from "@tanstack/react-query";
import { dangKyKetThucPhien } from "./session/phien";

/**
 * - staleTime 0: luôn tải lại từ máy chủ khi mở màn hình (ĐX-UF-04).
 * - retry 0 cho cả truy vấn và thao tác ghi: không tự gửi lại (FA-08); người dùng bấm Thử lại.
 * - refetchOnWindowFocus false: không tải lại bất ngờ khi đổi cửa sổ.
 * - mutation networkMode "always": mất mạng thì lần gọi HTTP thất bại ngay theo lỗi mạng, không
 *   bị giữ lại để tự phát khi có mạng (FA 3.6).
 * - query networkMode "always": mặc định "online" của TanStack tạm dừng im lặng khi offline, trái
 *   FA 3.4 (lỗi mạng phải hiển thị kèm Thử lại). Chỉ đổi cách báo lỗi, không thêm lần gọi nào.
 */
export function taoQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 0,
        retry: 0,
        refetchOnWindowFocus: false,
        networkMode: "always",
      },
      mutations: {
        retry: 0,
        networkMode: "always",
      },
    },
  });
}

/**
 * Xóa toàn bộ bộ nhớ đệm khi phiên kết thúc (đăng xuất, hết phiên, bị khóa) để người đăng nhập
 * sau không thấy dữ liệu của phiên trước (FA 3.5, 4.7). Trả hàm hủy đăng ký.
 */
export function noiXoaCacheKhiKetThucPhien(
  queryClient: QueryClient,
): () => void {
  return dangKyKetThucPhien(() => {
    queryClient.clear();
  });
}
