package com.nhom13.quanlymuontra.data.session;

/**
 * Trạng thái phiên do {@link SessionManager} phát cho giao diện (FA 5.1, FA 5.3 quy tắc 5). Giữ
 * nguyên mã theo FC 3.3.
 */
public enum TrangThaiPhien {
    /** Chưa đọc kho lưu ở lần mở ứng dụng này; giao diện chưa được điều hướng theo phiên. */
    CHUA_KHOI_PHUC,
    /** Đang có cặp token trong bộ nhớ. */
    CO_PHIEN,
    /** Không có phiên: chưa đăng nhập, đã đăng xuất hoặc dữ liệu lưu không đọc được. */
    KHONG_CO_PHIEN
}
