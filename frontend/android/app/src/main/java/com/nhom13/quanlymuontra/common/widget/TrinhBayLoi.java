package com.nhom13.quanlymuontra.common.widget;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.StringRes;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.data.model.ApiError;

/**
 * Quy tắc chọn nội dung của {@link ErrorView} từ một {@link ApiError} (DS 9.4, 9.8; FA 3.3, 3.4).
 * Hàm thuần, không giữ trạng thái: mỗi vùng tải của một màn hình tự gọi cho lỗi của riêng nó nên lỗi
 * một vùng không làm đổi nội dung của vùng khác (ĐX-UI-04).
 */
final class TrinhBayLoi {

    /** Hai kiểu nội dung lỗi dùng chung của Design-System. */
    enum Kieu {
        /** Mất kết nối hoặc quá thời gian chờ. */
        MANG,
        /** Mọi phản hồi lỗi khác không có xử lý riêng của màn hình. */
        HE_THONG
    }

    /** Nội dung trình bày; {@code traceId} chỉ có ở kiểu {@link Kieu#HE_THONG}. */
    static final class NoiDung {
        @NonNull final Kieu kieu;
        @StringRes final int thongDiepResId;
        @Nullable final String traceId;

        private NoiDung(@NonNull Kieu kieu, @StringRes int thongDiepResId, @Nullable String traceId) {
            this.kieu = kieu;
            this.thongDiepResId = thongDiepResId;
            this.traceId = traceId;
        }
    }

    private TrinhBayLoi() {}

    /**
     * @return nội dung cần hiển thị, hoặc {@code null} khi lỗi là {@link ApiError.Loai#HUY}: truy
     *     vấn bị hủy chủ động nên không có gì để báo (FA 3.3, quy tắc 4). Chỉ gọi cho lỗi của truy
     *     vấn; lỗi của thao tác ghi đã phát không đi qua hàm này (FA 3.3, quy tắc 3).
     */
    @Nullable
    static NoiDung choVung(@NonNull ApiError loi) {
        switch (loi.getLoai()) {
            case HUY:
                return null;
            case MANG:
            case QUA_THOI_GIAN:
                return new NoiDung(Kieu.MANG, R.string.loi_mang, null);
            case HTTP:
            default:
                // Gồm lỗi 500 và mọi lỗi HTTP không có xử lý riêng: thông báo chung kèm traceId,
                // không lộ chi tiết nội bộ (FA 3.4, dòng "Hệ thống" và đoạn cuối).
                String traceId = loi.getTraceId();
                boolean coTraceId = traceId != null && !traceId.isEmpty();
                return new NoiDung(Kieu.HE_THONG, R.string.loi_he_thong, coTraceId ? traceId : null);
        }
    }
}
