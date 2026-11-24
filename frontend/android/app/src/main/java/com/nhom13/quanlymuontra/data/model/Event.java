package com.nhom13.quanlymuontra.data.model;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

/**
 * Sự kiện một lần (điều hướng, thông báo lỗi ghi) đi qua {@code LiveData<Event<T>>} để không phát
 * lại khi xoay màn hình (FA 5.1, FC 6.3). Chỉ dùng trên luồng chính, nơi {@code LiveData} gọi
 * người quan sát.
 *
 * @param <T> kiểu nội dung sự kiện
 */
public final class Event<T> {

    @NonNull private final T noiDung;
    private boolean daXuLy = false;

    public Event(@NonNull T noiDung) {
        this.noiDung = noiDung;
    }

    /** Trả nội dung ở lần gọi đầu tiên, các lần sau trả {@code null}. */
    @Nullable
    public T getContentIfNotHandled() {
        if (daXuLy) {
            return null;
        }
        daXuLy = true;
        return noiDung;
    }
}
