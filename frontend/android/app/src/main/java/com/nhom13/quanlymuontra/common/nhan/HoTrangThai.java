package com.nhom13.quanlymuontra.common.nhan;

/**
 * Họ trạng thái theo Phụ lục A của {@code User-Flows.md} (FC 3.5). Một mã có thể trùng giữa các họ
 * nhưng tông khác nhau (DS 7.1) nên nhãn luôn tra theo cặp (họ, mã). Vai trò (SINH_VIEN, NHAN_VIEN,
 * QUAN_TRI) không phải họ trạng thái và hiển thị bằng chữ thường (DS 7.1).
 */
public enum HoTrangThai {
    TINH_TRANG_THIET_BI,
    PHIEU_YEU_CAU,
    PHIEU_MUON,
    DANG_KY_CHO,
    KET_QUA_KIEM_TRA,
    BIEN_BAN,
    LOAI_VI_PHAM,
    TRANG_THAI_HOC,
    QUYEN_MUON,
    TAI_KHOAN,
    LOAI_BAO_TRI,
    KET_QUA_BAO_TRI,
    NGUON_GIAO,
    LOAI_THONG_BAO;
}
