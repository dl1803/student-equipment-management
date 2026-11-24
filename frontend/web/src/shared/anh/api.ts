// Hàm API tải ảnh (FA 4.10 bước 5; FC 5.4). Tên hàm trùng operationId. Sở hữu: Thịnh (shared core).
// Ảnh dùng chung cho SCR-NV-06 (BAN_DAU), SCR-NV-09 (KHI_TRA), SCR-QT-04 (THIET_BI) nên đặt ở shared.
// multipart/form-data: không tự đặt Content-Type để trình duyệt gắn boundary. Không retry (ĐX-FA-22):
// thử lại do người dùng bấm ở từng ảnh.
import { apiClient } from "@/shared/api/client";
import type { GiaiDoanAnh, TaiAnhResponse } from "@/shared/api/types";

/** Tên tệp gửi kèm: nội dung luôn là JPEG sau nén; máy chủ kiểm chữ ký tệp, không tin tên. */
const TEN_TEP_GUI = "anh.jpg";

export async function anhTai(
  tep: Blob,
  giaiDoan: GiaiDoanAnh,
  tuyChon?: { signal?: AbortSignal },
): Promise<TaiAnhResponse> {
  const duLieu = new FormData();
  duLieu.append("file", tep, TEN_TEP_GUI);
  duLieu.append("giaiDoan", giaiDoan);
  const res = await apiClient.post<TaiAnhResponse>("/anh", duLieu, {
    signal: tuyChon?.signal,
  });
  return res.data;
}
