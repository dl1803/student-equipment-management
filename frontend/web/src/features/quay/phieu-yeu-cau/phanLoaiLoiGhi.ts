// Phân loại lỗi của Duyệt, Từ chối phiếu yêu cầu (UI-Screens 6.3 "Kết quả và lỗi"; UF-NV-01 E1–E3;
// FA 3.11). Sở hữu: Thịnh. Hàm thuần: phân nhánh theo maLoi/loai, không đọc detail (FC 5.5).
// - ROI_MAN_HINH: phiếu không còn Chờ duyệt (404, TRANG_THAI_KHONG_HOP_LE) hoặc phòng chưa được cấp
//   quyền (403) → báo ở danh sách và quay về SCR-NV-02.
// - TAI_LAI_KIEM_TRA: điều kiện/số lượng đổi (E3) hoặc dữ liệu vừa bị giao dịch khác đổi → tải lại
//   kết quả kiểm tra, hiển thị theo kết quả mới; KHÔNG tự gửi lại.
// - CHUA_BIET_KET_QUA: mất phản hồi sau khi đã gửi → khóa gửi, kiểm tra danh sách trước khi lặp.
// - TRONG_HOP: lỗi còn lại, hiển thị trong hộp đang mở.
import { laChuaBietKetQua, type ApiError } from "@/shared/api/apiError";
import {
  THONG_DIEP_LOI_CHUNG,
  thongBaoTheoLoi,
  type ThongBao,
} from "@/shared/labels/thongDiepLoi";
import { THONG_DIEP_PHIEU_YEU_CAU } from "./thongDiep";

export type KetQuaPhanLoai =
  | { loai: "ROI_MAN_HINH"; thongBao: ThongBao }
  | { loai: "TAI_LAI_KIEM_TRA"; thongBao: ThongBao }
  | { loai: "CHUA_BIET_KET_QUA" }
  | { loai: "TRONG_HOP"; thongBao: ThongBao };

/** Các loại lỗi hộp thoại chuyển lên màn hình xử lý. */
export type LoiChoManHinh = Exclude<KetQuaPhanLoai, { loai: "TRONG_HOP" }>;

/** Mã lỗi của Duyệt mà UI-Screens 6.3 xử lý như "không được duyệt" (E3). */
const MA_DIEU_KIEN_DA_DOI = new Set<string>([
  "SO_LUONG_KHA_DUNG_THAY_DOI",
  "SV_THE_HET_HAN",
  "SV_KHONG_CON_THEO_HOC",
  "SV_BI_KHOA_QUYEN_MUON",
  "VUOT_GIOI_HAN_THIET_BI",
  "VUOT_THOI_GIAN_MUON_TOI_DA",
]);

export function phanLoaiLoiGhi(
  loi: ApiError,
  hanhDong: "DUYET" | "TU_CHOI",
): KetQuaPhanLoai {
  if (laChuaBietKetQua(loi)) return { loai: "CHUA_BIET_KET_QUA" };
  const ma = loi.maLoi;
  if (ma === "PHONG_CHUA_CAP_QUYEN") {
    return {
      loai: "ROI_MAN_HINH",
      thongBao: {
        tong: "warning",
        noiDung: THONG_DIEP_LOI_CHUNG.PHONG_CHUA_CAP_QUYEN,
      },
    };
  }
  if (ma === "KHONG_TIM_THAY" || ma === "TRANG_THAI_KHONG_HOP_LE") {
    return {
      loai: "ROI_MAN_HINH",
      thongBao: {
        tong: "warning",
        noiDung: THONG_DIEP_PHIEU_YEU_CAU.KHONG_CON_CHO_DUYET,
      },
    };
  }
  if (ma === "XUNG_DOT_CAP_NHAT") {
    return {
      loai: "TAI_LAI_KIEM_TRA",
      thongBao: { tong: "warning", noiDung: THONG_DIEP_PHIEU_YEU_CAU.XUNG_DOT },
    };
  }
  if (hanhDong === "DUYET" && ma !== undefined && MA_DIEU_KIEN_DA_DOI.has(ma)) {
    return {
      loai: "TAI_LAI_KIEM_TRA",
      thongBao: {
        tong: "warning",
        noiDung: THONG_DIEP_PHIEU_YEU_CAU.DIEU_KIEN_DA_DOI,
      },
    };
  }
  return { loai: "TRONG_HOP", thongBao: thongBaoTheoLoi(loi) };
}
