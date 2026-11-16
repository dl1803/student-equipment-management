package com.nhom13.quanlymuontra.data.api;

import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.common.HangSo;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import java.util.concurrent.TimeUnit;
import okhttp3.HttpUrl;
import okhttp3.OkHttpClient;

/**
 * Dựng {@link OkHttpClient} của ứng dụng (FA 5.5, FC 6.4). Cả hai client dùng cùng thời gian chờ
 * 90 giây (ĐX-FA-04) và tắt {@code retryOnConnectionFailure} để OkHttp không tự gửi lại thao tác
 * ghi khi mất kết nối (FA-08, FA 3.11).
 */
public final class OkHttpClientFactory {

    private OkHttpClientFactory() {}

    /**
     * Client cho mọi yêu cầu nghiệp vụ: gắn token bằng {@link AuthInterceptor} và làm mới phiên
     * bằng {@link TokenAuthenticator}.
     *
     * @param diaChiGoc địa chỉ gốc của máy chủ, không có {@code /api/v1} (FA 5.8)
     */
    @NonNull
    public static OkHttpClient taoClientChinh(
            @NonNull SessionManager sessionManager, @NonNull HttpUrl diaChiGoc) {
        TokenAuthenticator authenticator =
                new TokenAuthenticator(sessionManager, taoClientLamMoi(), diaChiGoc);
        return xayDungCoBan()
                .addInterceptor(new AuthInterceptor(sessionManager))
                .authenticator(authenticator)
                .build();
    }

    /**
     * Client riêng cho lần làm mới token: không có interceptor và không có {@code Authenticator}
     * (FA 5.4).
     */
    @NonNull
    public static OkHttpClient taoClientLamMoi() {
        return xayDungCoBan().build();
    }

    @NonNull
    private static OkHttpClient.Builder xayDungCoBan() {
        long thoiGianCho = HangSo.THOI_GIAN_CHO_TOI_DA_GIAY;
        // callTimeout là giới hạn tổng của cả yêu cầu; ba mốc còn lại đặt cùng giá trị để không
        // mốc nào ngắn hơn mức tổng.
        return new OkHttpClient.Builder()
                .callTimeout(thoiGianCho, TimeUnit.SECONDS)
                .connectTimeout(thoiGianCho, TimeUnit.SECONDS)
                .readTimeout(thoiGianCho, TimeUnit.SECONDS)
                .writeTimeout(thoiGianCho, TimeUnit.SECONDS)
                .retryOnConnectionFailure(false);
    }
}
