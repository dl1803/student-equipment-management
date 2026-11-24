import { describe, expect, it } from "vitest";
import type { NhomLoi } from "./apiError";
import { nhomCuaLoi } from "./nhomLoi";

// Bảng FA 3.4, chép độc lập với mã để phát hiện lệch.
const FA_3_4: Record<string, NhomLoi> = {
  CHUA_XAC_THUC: "PHIEN",
  DANG_NHAP_SAI: "DANG_NHAP_SAI",
  TAI_KHOAN_BI_KHOA: "KHOA_TAI_KHOAN",
  KHONG_DU_QUYEN: "QUYEN",
  PHONG_CHUA_CAP_QUYEN: "QUYEN",
  DU_LIEU_KHONG_HOP_LE: "DU_LIEU_NHAP",
  KHONG_TIM_THAY: "KHONG_TON_TAI",
  XUNG_DOT_CAP_NHAT: "CAN_TAI_LAI",
  TRANG_THAI_KHONG_HOP_LE: "CAN_TAI_LAI",
  QUA_NHIEU_YEU_CAU: "TAN_SUAT",
  SV_THE_HET_HAN: "NGHIEP_VU_THEO_MAN_HINH",
  SV_KHONG_CON_THEO_HOC: "NGHIEP_VU_THEO_MAN_HINH",
  SV_BI_KHOA_QUYEN_MUON: "NGHIEP_VU_THEO_MAN_HINH",
  VUOT_GIOI_HAN_THIET_BI: "NGHIEP_VU_THEO_MAN_HINH",
  VUOT_THOI_GIAN_MUON_TOI_DA: "NGHIEP_VU_THEO_MAN_HINH",
  CON_HANG_KHONG_DANG_KY_CHO: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_KHONG_SAN_SANG: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_SAI_PHONG: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_SAI_LOAI: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_KHONG_THUOC_PHIEU: "NGHIEP_VU_THEO_MAN_HINH",
  THIEU_MO_TA_HOAC_KHUNG_PHAT: "NGHIEP_VU_THEO_MAN_HINH",
  GIA_TRI_CAU_HINH_KHONG_HOP_LE: "NGHIEP_VU_THEO_MAN_HINH",
  GIAO_SAU_GIO_DONG_CUA: "NGHIEP_VU_THEO_MAN_HINH",
  SO_LUONG_KHA_DUNG_THAY_DOI: "NGHIEP_VU_THEO_MAN_HINH",
  GIU_CHO_HET_HAN: "NGHIEP_VU_THEO_MAN_HINH",
  THIET_BI_DA_DUOC_GIAO: "NGHIEP_VU_THEO_MAN_HINH",
  MA_THIET_BI_TRUNG: "NGHIEP_VU_THEO_MAN_HINH",
  TEN_DANG_NHAP_TRUNG: "NGHIEP_VU_THEO_MAN_HINH",
  MA_NV_TRUNG: "NGHIEP_VU_THEO_MAN_HINH",
  KHUNG_PHAT_DA_DUNG_KHONG_XOA: "NGHIEP_VU_THEO_MAN_HINH",
  CHUA_CAU_HINH_GIO_QUAY: "NGHIEP_VU_THEO_MAN_HINH",
  DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG: "NGHIEP_VU_THEO_MAN_HINH",
};

describe("nhomCuaLoi", () => {
  it("FC 7.4: bảng đủ 32 mã của MaLoi (openapi.yaml v1.1.0)", () => {
    expect(Object.keys(FA_3_4)).toHaveLength(32);
  });

  it.each(Object.entries(FA_3_4))("FA 3.4: %s → %s", (maLoi, nhom) => {
    expect(nhomCuaLoi({ loai: "HTTP", status: 400, maLoi })).toBe(nhom);
  });

  it("FA 3.4: maLoi lạ → NGHIEP_VU_THEO_MAN_HINH, không ném", () => {
    expect(nhomCuaLoi({ loai: "HTTP", status: 422, maLoi: "MA_MOI" })).toBe(
      "NGHIEP_VU_THEO_MAN_HINH",
    );
    expect(nhomCuaLoi({ loai: "HTTP", status: 422, maLoi: "toString" })).toBe(
      "NGHIEP_VU_THEO_MAN_HINH",
    );
  });

  it("FA 3.3 quy tắc 1: không có maLoi (500, trang lỗi hạ tầng) → HE_THONG", () => {
    expect(nhomCuaLoi({ loai: "HTTP", status: 500, traceId: "t" })).toBe(
      "HE_THONG",
    );
    expect(nhomCuaLoi({ loai: "HTTP", status: 502 })).toBe("HE_THONG");
  });

  it("FA 3.4: mất kết nối, quá thời gian → MANG", () => {
    expect(nhomCuaLoi({ loai: "MANG" })).toBe("MANG");
    expect(nhomCuaLoi({ loai: "QUA_THOI_GIAN" })).toBe("MANG");
  });
});
