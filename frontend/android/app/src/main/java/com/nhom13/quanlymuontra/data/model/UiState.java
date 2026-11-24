package com.nhom13.quanlymuontra.data.model;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

/**
 * Trạng thái của một màn hình hoặc vùng dữ liệu (FA 5.1, FC 6.3).
 *
 * <p>Bất biến, không có hàm tạo công khai. Khi tải lại, {@link #dangTai(Object)} giữ dữ liệu cũ để
 * giao diện không nhấp nháy.
 *
 * @param <T> kiểu dữ liệu của màn hình
 */
public final class UiState<T> {

    /** Giá trị giữ nguyên mã theo FC 3.3. */
    public enum TrangThai {
        DANG_TAI,
        THANH_CONG,
        RONG,
        LOI
    }

    @NonNull private final TrangThai trangThai;
    @Nullable private final T duLieu;
    @Nullable private final ApiError loi;

    private UiState(
            @NonNull TrangThai trangThai, @Nullable T duLieu, @Nullable ApiError loi) {
        this.trangThai = trangThai;
        this.duLieu = duLieu;
        this.loi = loi;
    }

    /** Đang tải; {@code duLieuCu} là dữ liệu đang hiển thị (nếu có). */
    @NonNull
    public static <T> UiState<T> dangTai(@Nullable T duLieuCu) {
        return new UiState<>(TrangThai.DANG_TAI, duLieuCu, null);
    }

    /** Tải xong và có dữ liệu. */
    @NonNull
    public static <T> UiState<T> thanhCong(@NonNull T duLieu) {
        return new UiState<>(TrangThai.THANH_CONG, duLieu, null);
    }

    /** Tải xong nhưng không có phần tử nào; rỗng không phải lỗi (FA 3.10). */
    @NonNull
    public static <T> UiState<T> rong() {
        return new UiState<>(TrangThai.RONG, null, null);
    }

    /** Tải lỗi; {@code duLieuCu} là dữ liệu đang hiển thị (nếu có). */
    @NonNull
    public static <T> UiState<T> loi(@NonNull ApiError loi, @Nullable T duLieuCu) {
        return new UiState<>(TrangThai.LOI, duLieuCu, loi);
    }

    @NonNull
    public TrangThai getTrangThai() {
        return trangThai;
    }

    @Nullable
    public T getDuLieu() {
        return duLieu;
    }

    @Nullable
    public ApiError getLoi() {
        return loi;
    }
}
