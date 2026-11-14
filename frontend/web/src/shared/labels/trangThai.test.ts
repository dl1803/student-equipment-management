import { describe, expect, it } from "vitest";
import {
  nhanTrangThai,
  thongTinNhanPhatSinh,
  thongTinTrangThai,
  type HoTrangThai,
} from "./trangThai";

// Bảng DS 7.2, chép độc lập với bảng trong mã để phát hiện lệch.
const DS_7_2: Array<[HoTrangThai, string, string, string, boolean]> = [
  ["TINH_TRANG_THIET_BI", "SAN_SANG", "Sẵn sàng cho mượn", "success", false],
  ["TINH_TRANG_THIET_BI", "DANG_DUOC_MUON", "Đang được mượn", "info", false],
  [
    "TINH_TRANG_THIET_BI",
    "DANG_BAO_TRI_SUA_CHUA",
    "Đang bảo trì/sửa chữa",
    "warning",
    false,
  ],
  ["TINH_TRANG_THIET_BI", "DA_THANH_LY", "Đã thanh lý", "neutral", false],
  ["TINH_TRANG_THIET_BI", "DA_BAO_MAT", "Đã báo mất", "danger", false],
  ["PHIEU_YEU_CAU", "CHO_DUYET", "Chờ duyệt", "warning", false],
  ["PHIEU_YEU_CAU", "DA_DUYET", "Đã duyệt", "success", false],
  ["PHIEU_YEU_CAU", "TU_CHOI", "Từ chối", "danger", false],
  ["PHIEU_YEU_CAU", "DA_HUY", "Đã hủy", "neutral", false],
  ["PHIEU_YEU_CAU", "HET_HAN", "Hết hạn", "neutral", false],
  ["PHIEU_YEU_CAU", "DA_NHAN", "Đã nhận", "success", false],
  ["PHIEU_MUON", "DANG_MUON", "Đang mượn", "info", false],
  ["PHIEU_MUON", "QUA_HAN", "Quá hạn", "danger", true],
  ["PHIEU_MUON", "DA_TRA", "Đã trả", "success", false],
  ["DANG_KY_CHO", "DANG_CHO", "Đang chờ", "info", false],
  ["DANG_KY_CHO", "DA_THONG_BAO", "Đã thông báo", "success", false],
  ["DANG_KY_CHO", "DA_NHAN", "Đã nhận", "success", false],
  ["DANG_KY_CHO", "DA_HUY", "Đã hủy", "neutral", false],
  ["DANG_KY_CHO", "HET_HAN", "Hết hạn", "neutral", false],
  ["KET_QUA_KIEM_TRA", "NGUYEN_VEN", "Nguyên vẹn", "success", false],
  ["KET_QUA_KIEM_TRA", "HU_HONG", "Hư hỏng", "danger", false],
  ["KET_QUA_KIEM_TRA", "THIEU_PHU_KIEN", "Thiếu phụ kiện", "warning", false],
  ["KET_QUA_KIEM_TRA", "MAT", "Mất", "danger", true],
  ["BIEN_BAN", "CHUA_NOP", "Chưa nộp", "warning", false],
  ["BIEN_BAN", "DA_NOP", "Đã nộp", "success", false],
  ["BIEN_BAN", "DA_HUY", "Đã hủy", "neutral", false],
  ["LOAI_VI_PHAM", "TRA_TRE", "Trả trễ", "warning", false],
  ["LOAI_VI_PHAM", "HU_HONG", "Hư hỏng", "danger", false],
  ["LOAI_VI_PHAM", "MAT", "Mất", "danger", false],
  ["LOAI_VI_PHAM", "KHONG_DEN_NHAN", "Không đến nhận", "warning", false],
  ["TRANG_THAI_HOC", "DANG_HOC", "Đang học", "success", false],
  ["TRANG_THAI_HOC", "TOT_NGHIEP", "Tốt nghiệp", "neutral", false],
  ["TRANG_THAI_HOC", "THOI_HOC", "Thôi học", "danger", false],
  ["TRANG_THAI_HOC", "DINH_CHI", "Đình chỉ", "danger", false],
  ["QUYEN_MUON", "BINH_THUONG", "Bình thường", "success", false],
  ["QUYEN_MUON", "TAM_KHOA", "Tạm khóa", "warning", true],
  ["QUYEN_MUON", "CAM_MUON_DAI_HAN", "Cấm mượn dài hạn", "danger", true],
  ["TAI_KHOAN", "HOAT_DONG", "Hoạt động", "success", false],
  ["TAI_KHOAN", "TAM_KHOA_DANG_NHAP", "Tạm khóa đăng nhập", "warning", false],
  ["TAI_KHOAN", "BI_KHOA", "Bị khóa", "danger", false],
  ["LOAI_BAO_TRI", "BAO_TRI_DINH_KY", "Bảo trì định kỳ", "neutral", false],
  ["LOAI_BAO_TRI", "SUA_CHUA", "Sửa chữa", "neutral", false],
  ["KET_QUA_BAO_TRI", "DUA_VE_SAN_SANG", "Đưa về Sẵn sàng", "success", false],
  ["KET_QUA_BAO_TRI", "DE_XUAT_THANH_LY", "Đề xuất thanh lý", "warning", false],
  ["NGUON_GIAO", "PHIEU_YEU_CAU", "Phiếu yêu cầu", "neutral", false],
  ["NGUON_GIAO", "DANG_KY_CHO", "Danh sách chờ", "neutral", false],
  ["NGUON_GIAO", "TRUC_TIEP", "Mượn trực tiếp", "neutral", false],
  ["LOAI_THONG_BAO", "NHAC_HAN_TRA", "Nhắc hạn trả", "warning", false],
  ["LOAI_THONG_BAO", "QUA_HAN", "Quá hạn", "danger", false],
  [
    "LOAI_THONG_BAO",
    "THIET_BI_CHO_SAN_SANG",
    "Thiết bị chờ sẵn sàng",
    "success",
    false,
  ],
  ["LOAI_THONG_BAO", "DUYET_HOAC_TU_CHOI", "Duyệt hoặc từ chối", "info", false],
  ["LOAI_THONG_BAO", "VI_PHAM_PHI_PHAT", "Vi phạm, phí phạt", "danger", false],
  ["LOAI_THONG_BAO", "NHAC_NHO_NV", "Nhắc nhở của NV", "info", false],
  ["LOAI_THONG_BAO", "NHAC_BAO_TRI", "Nhắc bảo trì", "neutral", false],
];

describe("thongTinTrangThai", () => {
  it.each(DS_7_2)(
    "TC-03: %s/%s → %s, tông %s, đậm %s",
    (ho, ma, nhan, tong, nhanDam) => {
      expect(thongTinTrangThai(ho, ma)).toEqual({
        nhan,
        tong,
        nhanDam,
        laMaChuaBiet: false,
      });
    },
  );

  it("DS 7.1: cùng mã khác họ tra theo họ (QUA_HAN phiếu mượn đậm, loại thông báo nhạt)", () => {
    expect(thongTinTrangThai("PHIEU_MUON", "QUA_HAN").nhanDam).toBe(true);
    expect(thongTinTrangThai("LOAI_THONG_BAO", "QUA_HAN").nhanDam).toBe(false);
  });

  it("DS 7.1: mã chưa biết → nguyên mã, tông neutral, không đậm, không ném lỗi", () => {
    expect(thongTinTrangThai("PHIEU_MUON", "MA_MOI_TU_BACKEND")).toEqual({
      nhan: "MA_MOI_TU_BACKEND",
      tong: "neutral",
      nhanDam: false,
      laMaChuaBiet: true,
    });
  });

  it("DS 7.1: tên thuộc tính của Object không bị coi là mã đã biết", () => {
    expect(thongTinTrangThai("BIEN_BAN", "toString").laMaChuaBiet).toBe(true);
    expect(thongTinTrangThai("BIEN_BAN", "__proto__").laMaChuaBiet).toBe(true);
  });

  it("nhanTrangThai trả nhãn chữ", () => {
    expect(nhanTrangThai("PHIEU_YEU_CAU", "CHO_DUYET")).toBe("Chờ duyệt");
    expect(nhanTrangThai("PHIEU_YEU_CAU", "XYZ")).toBe("XYZ");
  });
});

describe("thongTinNhanPhatSinh", () => {
  it("DS 7.3: nhãn phát sinh có nhãn, tông, mức nhấn", () => {
    expect(thongTinNhanPhatSinh("HET_HANG")).toMatchObject({
      nhan: "Hết hàng",
      tong: "danger",
      nhanDam: false,
    });
    expect(thongTinNhanPhatSinh("HOP_LE")).toMatchObject({
      nhan: "Hợp lệ",
      tong: "success",
      nhanDam: true,
    });
    expect(thongTinNhanPhatSinh("TAM_TINH")).toMatchObject({
      nhan: "Tạm tính",
      tong: "warning",
    });
  });
});
