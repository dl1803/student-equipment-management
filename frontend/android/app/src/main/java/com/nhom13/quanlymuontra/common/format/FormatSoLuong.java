package com.nhom13.quanlymuontra.common.format;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.common.HangSo;

/**
 * Định dạng số lượng để hiển thị (DS 9.7, FC 7.2). Chỉ ghép hai số máy chủ trả về, không tính toán
 * (FA-07). Giá trị thiếu hiển thị {@link HangSo#GIA_TRI_THIEU}.
 */
public final class FormatSoLuong {

    private FormatSoLuong() {}

    /** Số thiết bị đang mượn trên số tối đa của nhóm, dạng {@code n/m} (DS 9.7), ví dụ {@code 1/3}. */
    @NonNull
    public static String tren(@Nullable Integer soDangCo, @Nullable Integer toiDa) {
        return giaTri(soDangCo) + "/" + giaTri(toiDa);
    }

    @NonNull
    private static String giaTri(@Nullable Integer so) {
        return so == null ? HangSo.GIA_TRI_THIEU : String.valueOf(so.intValue());
    }
}
