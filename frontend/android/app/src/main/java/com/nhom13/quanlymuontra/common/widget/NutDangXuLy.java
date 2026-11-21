package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import androidx.annotation.NonNull;
import androidx.annotation.StringRes;
import androidx.core.content.ContextCompat;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.progressindicator.CircularProgressIndicatorSpec;
import com.google.android.material.progressindicator.IndeterminateDrawable;
import com.nhom13.quanlymuontra.R;

/**
 * Trạng thái "đang xử lý" của nút (DS CMP-01, CMP-26, DS 9.8): chữ đổi thành "Đang xử lý" kèm chỉ báo
 * quay, nút vô hiệu và giữ nguyên chiều rộng. Dùng cho nút gửi ở màn hình và nút xác nhận của hộp xác nhận.
 * Bên gọi khôi phục bằng {@link #ketThuc}, truyền lại nhãn gốc.
 */
public final class NutDangXuLy {

    private NutDangXuLy() {}

    /** Chuyển nút sang "Đang xử lý" và vô hiệu nút. Gọi lại khi đang xử lý không có thêm tác dụng. */
    public static void batDau(@NonNull MaterialButton nut) {
        Context nguCanh = nut.getContext();
        if (nut.getWidth() > 0) {
            // Giữ chiều rộng để nút không co lại khi chữ đổi (DS CMP-01).
            nut.setMinWidth(nut.getWidth());
        }
        CircularProgressIndicatorSpec dacTa =
                new CircularProgressIndicatorSpec(
                        nguCanh,
                        null,
                        0,
                        com.google.android.material.R.style
                                .Widget_MaterialComponents_CircularProgressIndicator_ExtraSmall);
        dacTa.indicatorColors =
                new int[] {ContextCompat.getColor(nguCanh, R.color.ds_color_action_disabled_text)};
        nut.setIcon(IndeterminateDrawable.createCircularDrawable(nguCanh, dacTa));
        nut.setIconGravity(MaterialButton.ICON_GRAVITY_TEXT_START);
        nut.setText(R.string.hanh_dong_dang_xu_ly);
        nut.setEnabled(false);
    }

    /**
     * Khôi phục nhãn gốc và bỏ chỉ báo. Không bật lại nút: bên gọi quyết định nút bật hay tắt theo trạng
     * thái biểu mẫu của nó.
     */
    public static void ketThuc(@NonNull MaterialButton nut, @StringRes int nhanGoc) {
        nut.setIcon(null);
        nut.setText(nhanGoc);
    }

    /** Như {@link #ketThuc(MaterialButton, int)} cho nhãn gốc là chuỗi động (nút xác nhận của hộp thoại). */
    public static void ketThuc(@NonNull MaterialButton nut, @NonNull CharSequence nhanGoc) {
        nut.setIcon(null);
        nut.setText(nhanGoc);
    }
}
