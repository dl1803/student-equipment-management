package com.nhom13.quanlymuontra.data.repository;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.data.model.ApiError;

/**
 * Callback mà {@code Repository} trả kết quả cho {@code ViewModel} (FA 5.1, FC 6.4). Cả hai phương
 * thức chạy trên luồng chính vì Retrofit gọi lại ở đó.
 *
 * @param <T> kiểu kết quả; {@link Void} với thao tác không có thân phản hồi (ví dụ đăng xuất)
 */
public interface ApiCallback<T> {

    /** Thành công; {@code ketQua} là {@code null} khi phản hồi không có thân (HTTP 204). */
    void onSuccess(@Nullable T ketQua);

    /** Thất bại; mọi lỗi đã được chuẩn hóa thành {@link ApiError}. */
    void onError(@NonNull ApiError loi);
}
