package com.nhom13.quanlymuontra.data.api;

import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.data.model.ApiError;
import java.io.IOException;

/**
 * Cách duy nhất để {@link TokenAuthenticator} báo "lần làm mới phiên thất bại" ra khỏi OkHttp (FA
 * 5.4, ĐX-FA-23). Trả {@code null} từ {@code Authenticator} sẽ khiến OkHttp giao lại chính phản
 * hồi 401 gốc và làm mất nguyên nhân thật (401, 429, mạng, quá thời gian chờ, 500); ném ngoại lệ
 * này thì {@link ApiErrorParser#tuNgoaiLe} lấy lại nguyên {@link ApiError} của lần làm mới.
 *
 * <p>Thông điệp cố định, không chứa token hay nội dung phản hồi (FC 6.8).
 */
public final class LamMoiPhienThatBaiException extends IOException {

    private static final long serialVersionUID = 1L;

    // transient vì ApiError không Serializable; ngoại lệ này không được tuần tự hóa trong app.
    @NonNull private final transient ApiError apiError;

    public LamMoiPhienThatBaiException(@NonNull ApiError apiError) {
        super("Làm mới phiên thất bại");
        this.apiError = apiError;
    }

    /** Lỗi đã chuẩn hóa của lần làm mới, giữ nguyên status, maLoi, traceId nếu có. */
    @NonNull
    public ApiError getApiError() {
        return apiError;
    }
}
