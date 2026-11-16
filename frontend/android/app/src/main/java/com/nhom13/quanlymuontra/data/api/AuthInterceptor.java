package com.nhom13.quanlymuontra.data.api;

import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import java.io.IOException;
import okhttp3.Interceptor;
import okhttp3.Request;
import okhttp3.Response;

/**
 * Gắn {@code Authorization: Bearer <accessToken>} cho mọi yêu cầu, trừ bốn endpoint không yêu cầu
 * xác thực (FA 3.5, FA 5.5, FC 6.4). Token lấy từ {@link SessionManager} tại thời điểm gửi nên
 * luôn là token mới nhất. Khi chưa có phiên thì gửi yêu cầu không kèm token.
 *
 * <p>Lớp này không ghi nhật ký (FC 6.8).
 */
public final class AuthInterceptor implements Interceptor {

    static final String TIEU_DE_XAC_THUC = "Authorization";
    static final String TIEN_TO_BEARER = "Bearer ";

    @NonNull private final SessionManager sessionManager;

    public AuthInterceptor(@NonNull SessionManager sessionManager) {
        this.sessionManager = sessionManager;
    }

    @NonNull
    @Override
    public Response intercept(@NonNull Chain chain) throws IOException {
        Request yeuCau = chain.request();
        if (DuongDanApi.khongCanToken(yeuCau.url())) {
            return chain.proceed(yeuCau);
        }
        TokenPair capToken = sessionManager.getTokenPair();
        if (capToken == null) {
            return chain.proceed(yeuCau);
        }
        return chain.proceed(
                yeuCau.newBuilder()
                        .header(TIEU_DE_XAC_THUC, TIEN_TO_BEARER + capToken.getAccessToken())
                        .build());
    }
}
