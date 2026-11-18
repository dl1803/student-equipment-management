package com.nhom13.quanlymuontra.common.widget;

import androidx.annotation.ColorRes;
import androidx.annotation.DrawableRes;
import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.nhan.Tong;

/**
 * Bảng tra token màu và biểu tượng theo {@link Tong} (DS 5.2, CMP-13, CMP-24). Chỉ trả tên tài
 * nguyên; thành phần tự đọc màu bằng {@code ContextCompat.getColor} để theo đúng token.
 */
final class GiaoDienTong {

    private GiaoDienTong() {}

    @ColorRes
    static int nen(@NonNull Tong tong) {
        switch (tong) {
            case INFO:
                return R.color.ds_color_tone_info_bg;
            case SUCCESS:
                return R.color.ds_color_tone_success_bg;
            case WARNING:
                return R.color.ds_color_tone_warning_bg;
            case DANGER:
                return R.color.ds_color_tone_danger_bg;
            default:
                return R.color.ds_color_tone_neutral_bg;
        }
    }

    @ColorRes
    static int vien(@NonNull Tong tong) {
        switch (tong) {
            case INFO:
                return R.color.ds_color_tone_info_border;
            case SUCCESS:
                return R.color.ds_color_tone_success_border;
            case WARNING:
                return R.color.ds_color_tone_warning_border;
            case DANGER:
                return R.color.ds_color_tone_danger_border;
            default:
                return R.color.ds_color_tone_neutral_border;
        }
    }

    @ColorRes
    static int chu(@NonNull Tong tong) {
        switch (tong) {
            case INFO:
                return R.color.ds_color_tone_info_text;
            case SUCCESS:
                return R.color.ds_color_tone_success_text;
            case WARNING:
                return R.color.ds_color_tone_warning_text;
            case DANGER:
                return R.color.ds_color_tone_danger_text;
            default:
                return R.color.ds_color_tone_neutral_text;
        }
    }

    @ColorRes
    static int bieuTuong(@NonNull Tong tong) {
        switch (tong) {
            case INFO:
                return R.color.ds_color_tone_info_icon;
            case SUCCESS:
                return R.color.ds_color_tone_success_icon;
            case WARNING:
                return R.color.ds_color_tone_warning_icon;
            case DANGER:
                return R.color.ds_color_tone_danger_icon;
            default:
                return R.color.ds_color_tone_neutral_icon;
        }
    }

    @ColorRes
    static int dac(@NonNull Tong tong) {
        switch (tong) {
            case INFO:
                return R.color.ds_color_tone_info_solid;
            case SUCCESS:
                return R.color.ds_color_tone_success_solid;
            case WARNING:
                return R.color.ds_color_tone_warning_solid;
            case DANGER:
                return R.color.ds_color_tone_danger_solid;
            default:
                return R.color.ds_color_tone_neutral_solid;
        }
    }

    @ColorRes
    static int chuTrenNenDac(@NonNull Tong tong) {
        switch (tong) {
            case INFO:
                return R.color.ds_color_tone_info_on_solid;
            case SUCCESS:
                return R.color.ds_color_tone_success_on_solid;
            case WARNING:
                return R.color.ds_color_tone_warning_on_solid;
            case DANGER:
                return R.color.ds_color_tone_danger_on_solid;
            default:
                return R.color.ds_color_tone_neutral_on_solid;
        }
    }

    /** Biểu tượng của tông (DS Phụ lục B); {@code 0} cho tông trung tính vì không có biểu tượng. */
    @DrawableRes
    static int hinhBieuTuong(@NonNull Tong tong) {
        switch (tong) {
            case INFO:
                return R.drawable.ic_info;
            case SUCCESS:
                return R.drawable.ic_check_circle;
            case WARNING:
                return R.drawable.ic_warning;
            case DANGER:
                return R.drawable.ic_error;
            default:
                return 0;
        }
    }
}
