package com.nhom13.quanlymuontra.data.session;

import androidx.annotation.NonNull;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.util.Base64;

/**
 * Đọc claim {@code exp} của access token JWT để biết token còn hạn hay không (FA 5.4, FA 3.5).
 *
 * <p>Chỉ dùng để tránh một lần gọi chắc chắn bị 401 khi khởi động (SCR-CM-03). Không xác thực chữ ký
 * và không dùng cho quyết định bảo mật: máy chủ luôn kiểm lại token. Không ghi token hay phần thân
 * token ra log. Token không đọc được (sai định dạng, thiếu {@code exp}) được coi là hết hạn, khi đó
 * ứng dụng làm mới phiên và máy chủ quyết định.
 */
public final class HanAccessToken {

    private HanAccessToken() {}

    /**
     * @param accessToken chuỗi JWT dạng {@code header.payload.signature}
     * @param bayGio thời điểm so sánh
     * @return {@code true} khi {@code exp} (giây từ epoch) lớn hơn thời điểm so sánh
     */
    public static boolean conHan(@NonNull String accessToken, @NonNull Instant bayGio) {
        try {
            String[] phan = accessToken.split("\\.", -1);
            if (phan.length != 3) {
                return false;
            }
            byte[] thanGiaiMa = Base64.getUrlDecoder().decode(phan[1]);
            JsonObject thanToken =
                    JsonParser.parseString(new String(thanGiaiMa, StandardCharsets.UTF_8))
                            .getAsJsonObject();
            if (!thanToken.has("exp") || thanToken.get("exp").isJsonNull()) {
                return false;
            }
            long hetHanGiay = thanToken.get("exp").getAsLong();
            return bayGio.getEpochSecond() < hetHanGiay;
        } catch (RuntimeException khongDocDuoc) {
            // IllegalArgumentException (Base64), JsonParseException, IllegalStateException,
            // NumberFormatException, UnsupportedOperationException: đều là token không đọc được.
            return false;
        }
    }
}
