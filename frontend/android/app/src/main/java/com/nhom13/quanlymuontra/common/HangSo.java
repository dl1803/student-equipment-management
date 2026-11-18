package com.nhom13.quanlymuontra.common;

import java.time.ZoneId;

/**
 * Hằng số dùng chung của app Android. Mỗi hằng số có đúng một nơi định nghĩa và giá trị lấy từ
 * tài liệu nguồn; muốn đổi giá trị phải đổi tài liệu nguồn trước (FC 7.1, ĐX-FC-03).
 */
public final class HangSo {

    /** Tổng thời gian chờ tối đa của một yêu cầu HTTP, tính bằng giây (ĐX-FA-04). */
    public static final int THOI_GIAN_CHO_TOI_DA_GIAY = 90;

    /**
     * Số giây chờ trước khi vùng đang tải hiện dòng "Hệ thống đang khởi động, vui lòng chờ"
     * (ĐX-FA-04, UI-Screens 3.1).
     */
    public static final int NGUONG_THONG_BAO_KHOI_DONG_GIAY = 8;

    /** Múi giờ hiển thị của mọi thời điểm; API trả giờ UTC (UI-Screens 3.4, ĐX-UI-12). */
    public static final ZoneId MUI_GIO_VN = ZoneId.of("Asia/Ho_Chi_Minh");

    /** Chữ hiển thị khi giá trị không có dữ liệu: dấu gạch dài U+2014 (ĐX-DS-12, FC 7.2). */
    public static final String GIA_TRI_THIEU = "\u2014";

    private HangSo() {}
}
