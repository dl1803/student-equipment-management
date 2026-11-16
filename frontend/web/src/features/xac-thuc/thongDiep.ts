// Thông điệp riêng của SCR-CM-02 (UI-Screens 4.5; FA 3.5; FC 5.5 [ĐX-FC-11]). Sở hữu: Thịnh.
import type { ApiError, LoaiLoi } from "@/shared/api/apiError";
import type { LyDoKetThucPhien } from "@/shared/api/session/trangThaiPhien";
import type { TongBanner } from "@/shared/components/Banner";
import { THONG_DIEP_LOI_CHUNG } from "@/shared/labels/thongDiepLoi";

export const THONG_DIEP_DANG_NHAP = {
  DANG_NHAP_SAI: "Thông tin đăng nhập không đúng",
  TAI_KHOAN_BI_KHOA:
    "Tài khoản không đăng nhập được. Nhân viên liên hệ quản trị viên để mở lại; quản trị viên liên hệ bên vận hành hệ thống.",
  QUA_NHIEU_YEU_CAU:
    "Bạn đã thử quá nhiều lần. Vui lòng chờ một lát rồi thử lại.",
  PHIEN_KET_THUC_DO_MANG:
    "Không kết nối được tới hệ thống nên phiên đã kết thúc. Kiểm tra kết nối rồi đăng nhập lại.",
  PHIEN_KET_THUC_DO_TAN_SUAT:
    "Hệ thống đang nhận quá nhiều yêu cầu nên phiên đã kết thúc. Vui lòng chờ một lát rồi đăng nhập lại.",
  PHIEN_KET_THUC_DO_HE_THONG:
    "Đã xảy ra lỗi hệ thống nên phiên đã kết thúc. Vui lòng đăng nhập lại.",
} as const;

export type ThongBao = { tong: TongBanner; noiDung: string; traceId?: string };

function kemTraceId(thongBao: ThongBao, loi?: ApiError): ThongBao {
  return loi?.traceId !== undefined
    ? { ...thongBao, traceId: loi.traceId }
    : thongBao;
}

function laLoiMang(loai: LoaiLoi): boolean {
  return loai === "MANG" || loai === "QUA_THOI_GIAN" || loai === "HUY";
}

/**
 * Lý do về màn hình đăng nhập (FA 3.5, ĐX-FA-23): 401 báo hết phiên; 429 hướng dẫn chờ; mạng và
 * quá thời gian báo đúng loại; lỗi hệ thống kèm traceId; bị khóa thì báo tài khoản bị khóa.
 */
export function thongBaoKetThucPhien(
  lyDo: LyDoKetThucPhien | undefined,
  loi: ApiError | undefined,
): ThongBao | null {
  if (lyDo === "BI_KHOA") {
    return { tong: "danger", noiDung: THONG_DIEP_DANG_NHAP.TAI_KHOAN_BI_KHOA };
  }
  if (lyDo === undefined) {
    // Đăng xuất chủ động không báo gì; vai trò không dùng web thì báo không đủ quyền.
    return loi === undefined
      ? null
      : { tong: "danger", noiDung: THONG_DIEP_LOI_CHUNG.KHONG_DU_QUYEN };
  }
  if (loi === undefined || loi.maLoi === "CHUA_XAC_THUC") {
    return { tong: "info", noiDung: THONG_DIEP_LOI_CHUNG.PHIEN_HET_HAN };
  }
  if (laLoiMang(loi.loai)) {
    return {
      tong: "danger",
      noiDung: THONG_DIEP_DANG_NHAP.PHIEN_KET_THUC_DO_MANG,
    };
  }
  if (loi.status === 429) {
    return kemTraceId(
      {
        tong: "warning",
        noiDung: THONG_DIEP_DANG_NHAP.PHIEN_KET_THUC_DO_TAN_SUAT,
      },
      loi,
    );
  }
  if (loi.maLoi === undefined) {
    return kemTraceId(
      {
        tong: "danger",
        noiDung: THONG_DIEP_DANG_NHAP.PHIEN_KET_THUC_DO_HE_THONG,
      },
      loi,
    );
  }
  return { tong: "info", noiDung: THONG_DIEP_LOI_CHUNG.PHIEN_HET_HAN };
}

/** Kết quả lỗi của lần đăng nhập (UI-Screens 4.5; DS 9.4). */
export function thongBaoLoiDangNhap(loi: ApiError): ThongBao {
  if (laLoiMang(loi.loai)) {
    return { tong: "danger", noiDung: THONG_DIEP_LOI_CHUNG.MANG };
  }
  switch (loi.maLoi) {
    case "DANG_NHAP_SAI":
      return { tong: "danger", noiDung: THONG_DIEP_DANG_NHAP.DANG_NHAP_SAI };
    case "TAI_KHOAN_BI_KHOA":
      return {
        tong: "danger",
        noiDung: THONG_DIEP_DANG_NHAP.TAI_KHOAN_BI_KHOA,
      };
    case "QUA_NHIEU_YEU_CAU":
      return {
        tong: "warning",
        noiDung: THONG_DIEP_DANG_NHAP.QUA_NHIEU_YEU_CAU,
      };
    case "KHONG_DU_QUYEN":
      // Tài khoản đăng nhập được nhưng không thuộc vai trò dùng web (NFR-01).
      return { tong: "danger", noiDung: THONG_DIEP_LOI_CHUNG.KHONG_DU_QUYEN };
    default:
      return kemTraceId(
        { tong: "danger", noiDung: THONG_DIEP_LOI_CHUNG.HE_THONG },
        loi,
      );
  }
}
