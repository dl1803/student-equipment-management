package com.nhom13.quanlymuontra.common.nhan;

import androidx.annotation.NonNull;
import androidx.annotation.StringRes;
import com.nhom13.quanlymuontra.common.HangSo;

/**
 * Cách hiển thị một trạng thái: nhãn, tông và mức nhấn (DS 7.1). Bất biến, chỉ tạo bằng phương thức
 * tĩnh; không giữ {@code Context} nên kiểm thử được trên JVM.
 */
public final class ThongTinTrangThai {

    @StringRes private final int nhanResId;
    @NonNull private final String maGoc;
    @NonNull private final Tong tong;
    private final boolean nhanDam;

    private ThongTinTrangThai(
            @StringRes int nhanResId, @NonNull String maGoc, @NonNull Tong tong, boolean nhanDam) {
        this.nhanResId = nhanResId;
        this.maGoc = maGoc;
        this.tong = tong;
        this.nhanDam = nhanDam;
    }

    /** Mã đã biết: nhãn nằm trong {@code strings.xml}. */
    @NonNull
    static ThongTinTrangThai daBiet(@StringRes int nhanResId, @NonNull Tong tong, boolean nhanDam) {
        return new ThongTinTrangThai(nhanResId, "", tong, nhanDam);
    }

    /**
     * Mã chưa biết: hiển thị nguyên mã, tông trung tính, mức nhạt, không biểu tượng và không báo lỗi
     * (DS 7.1, UI-Screens 3.3). {@code null} hiển thị giá trị thiếu.
     */
    @NonNull
    static ThongTinTrangThai maLa(String ma) {
        return new ThongTinTrangThai(0, ma == null ? HangSo.GIA_TRI_THIEU : ma, Tong.NEUTRAL, false);
    }

    /** Mã nhãn trong {@code strings.xml}; {@code 0} khi {@link #isMaLa()}. */
    @StringRes
    public int getNhanResId() {
        return nhanResId;
    }

    /** Mã nguyên văn để hiển thị khi {@link #isMaLa()}; chuỗi rỗng khi mã đã biết. */
    @NonNull
    public String getMaGoc() {
        return maGoc;
    }

    @NonNull
    public Tong getTong() {
        return tong;
    }

    /** {@code true} là nhấn đậm: nền đặc và chữ đảo màu (DS 7.1). */
    public boolean isNhanDam() {
        return nhanDam;
    }

    /** {@code true} khi mã không có trong bảng ánh xạ. */
    public boolean isMaLa() {
        return nhanResId == 0;
    }
}
