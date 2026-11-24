package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import androidx.annotation.NonNull;
import androidx.core.content.ContextCompat;
import com.google.android.material.card.MaterialCardView;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.nhan.Tong;

/**
 * CMP-15 Thẻ, biến thể Tông (DS 8): nền và viền của thẻ theo tông phản hồi (DS 5.2), dùng cho thẻ quyền
 * mượn khi khác Bình thường (SCR-SV-01). Dùng chung bảng token với CMP-13 và CMP-24 qua {@link
 * GiaoDienTong}; lớp này chỉ tồn tại vì {@code GiaoDienTong} không công khai. Thẻ tông không thay khối thông
 * báo (CMP-24) khi nội dung là lỗi hoặc cảnh báo.
 */
public final class TheTong {

    private TheTong() {}

    /** Đổi nền và viền của thẻ sang tông {@code tong}. */
    public static void apDung(@NonNull MaterialCardView the, @NonNull Tong tong) {
        Context nguCanh = the.getContext();
        the.setCardBackgroundColor(ContextCompat.getColor(nguCanh, GiaoDienTong.nen(tong)));
        the.setStrokeColor(ContextCompat.getColor(nguCanh, GiaoDienTong.vien(tong)));
    }

    /** Trả thẻ về hình thức mặc định của {@code Widget.Ds.Card}: nền mặt phẳng, viền nhạt. */
    public static void datMacDinh(@NonNull MaterialCardView the) {
        Context nguCanh = the.getContext();
        the.setCardBackgroundColor(ContextCompat.getColor(nguCanh, R.color.ds_color_surface_default));
        the.setStrokeColor(ContextCompat.getColor(nguCanh, R.color.ds_color_border_subtle));
    }
}
