package com.nhom13.quanlymuontra.data.session;

import androidx.annotation.NonNull;

/**
 * Cặp token của một phiên đăng nhập (FA 3.5): access token và refresh token. Bất biến.
 *
 * <p>{@link #toString()} không chứa giá trị token để không lọt vào nhật ký (FA 5.4, FC 6.8).
 */
public final class TokenPair {

    @NonNull private final String accessToken;
    @NonNull private final String refreshToken;

    public TokenPair(@NonNull String accessToken, @NonNull String refreshToken) {
        this.accessToken = accessToken;
        this.refreshToken = refreshToken;
    }

    @NonNull
    public String getAccessToken() {
        return accessToken;
    }

    @NonNull
    public String getRefreshToken() {
        return refreshToken;
    }

    @NonNull
    @Override
    public String toString() {
        return "TokenPair[đã ẩn]";
    }
}
