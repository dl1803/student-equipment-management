// Trạng thái phiên phía client (FA 4.5). Sở hữu: Thịnh (shared core).
// Đây là trạng thái client, không thêm trường hay mã lỗi vào hợp đồng API.
import type { ApiError } from "../apiError";
import type { ThongTinToi } from "../types";

/** Lý do về màn hình đăng nhập để SCR-CM-02 báo đúng ngữ cảnh. */
export type LyDoKetThucPhien = "HET_PHIEN" | "BI_KHOA";

export type TrangThaiPhien =
  | { loai: "KHOI_DONG" }
  | { loai: "CHUA_DANG_NHAP"; lyDo?: LyDoKetThucPhien; loi?: ApiError }
  | { loai: "DA_DANG_NHAP"; nguoiDung: ThongTinToi };
