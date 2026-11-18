package com.nhom13.quanlymuontra.common.nhan;

/**
 * Năm tông phản hồi của Design-System (DS 5.2, ĐX-DS-06). Tông chỉ truyền đạt ý nghĩa tổng quát; ý
 * nghĩa chính xác nằm ở chữ của nhãn.
 */
public enum Tong {
    /** Đang diễn ra bình thường; thông tin. */
    INFO,
    /** Hoàn tất tốt, hợp lệ, sẵn sàng. */
    SUCCESS,
    /** Chờ xử lý hoặc cần chú ý; chưa phải vi phạm nặng. */
    WARNING,
    /** Vi phạm, bị từ chối, bị chặn, hư hỏng, mất, lỗi. */
    DANGER,
    /** Đã kết thúc, không hoạt động, phân loại trung tính. */
    NEUTRAL
}
