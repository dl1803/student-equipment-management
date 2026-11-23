package com.nhom13.quanlymuontra.ui.tracuu;

import androidx.annotation.Nullable;

/**
 * Ba lựa chọn của bộ lọc Tình trạng hàng (UI-Screens 5.2): Tất cả, Còn hàng, Hết hàng, ánh xạ sang tham số
 * {@code conHang} (bỏ trống, {@code true}, {@code false}). Client không tự tính còn hay hết hàng.
 */
public enum TinhTrangHang {
    TAT_CA(null),
    CON_HANG(Boolean.TRUE),
    HET_HANG(Boolean.FALSE);

    @Nullable private final Boolean giaTriConHang;

    TinhTrangHang(@Nullable Boolean giaTriConHang) {
        this.giaTriConHang = giaTriConHang;
    }

    /** Giá trị gửi lên làm {@code conHang}; {@code null} nghĩa là không gửi. */
    @Nullable
    public Boolean getGiaTriConHang() {
        return giaTriConHang;
    }
}
