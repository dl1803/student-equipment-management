package com.nhom13.quanlymuontra.data.session;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.SharedPreferences;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import java.nio.charset.StandardCharsets;
import java.security.GeneralSecurityException;

/**
 * Lớp duy nhất đọc và ghi {@code SharedPreferences} chứa token (FA 5.4, FC 6.5). Mỗi token được mã
 * hóa riêng qua {@link TokenCipher} rồi ghi dưới dạng chuỗi {@link CipherPayload}; không bao giờ
 * ghi token dạng rõ.
 *
 * <p>Khóa mất, bản mã hỏng hoặc thiếu một trong hai giá trị được coi như không có phiên: dữ liệu
 * bị xóa và {@link #read()} trả {@code null} (FA 5.4, mục Giải mã lỗi). Lớp này không ghi nhật ký.
 */
public final class TokenStore {

    /**
     * Tên tệp {@code SharedPreferences} của phiên. Phải trùng với {@code path} (thêm đuôi {@code
     * .xml}) trong {@code res/xml/backup_rules.xml} và {@code res/xml/data_extraction_rules.xml}; sự
     * trùng khớp được kiểm bằng {@code CauHinhSaoLuuPhienTest}.
     */
    static final String TEN_TEP = "phien_dang_nhap";

    private static final String KHOA_ACCESS_TOKEN = "accessToken";
    private static final String KHOA_REFRESH_TOKEN = "refreshToken";

    @NonNull private final SharedPreferences prefs;
    @NonNull private final TokenCipher cipher;

    public TokenStore(@NonNull SharedPreferences prefs, @NonNull TokenCipher cipher) {
        this.prefs = prefs;
        this.cipher = cipher;
    }

    /** Dựng bản dùng thật: tệp riêng của ứng dụng và {@link KeystoreCipher}. */
    @NonNull
    public static TokenStore create(@NonNull Context context) {
        return new TokenStore(
                context.getSharedPreferences(TEN_TEP, Context.MODE_PRIVATE), new KeystoreCipher());
    }

    /**
     * Đọc cặp token đã lưu.
     *
     * @return cặp token, hoặc {@code null} khi chưa có phiên hoặc dữ liệu không đọc được (khi đó
     *     dữ liệu đã bị xóa)
     */
    @Nullable
    public TokenPair read() {
        String accessMaHoa = prefs.getString(KHOA_ACCESS_TOKEN, null);
        String refreshMaHoa = prefs.getString(KHOA_REFRESH_TOKEN, null);
        if (accessMaHoa == null && refreshMaHoa == null) {
            return null;
        }
        if (accessMaHoa == null || refreshMaHoa == null) {
            clear();
            return null;
        }
        try {
            return new TokenPair(giaiMa(accessMaHoa), giaiMa(refreshMaHoa));
        } catch (GeneralSecurityException | IllegalArgumentException e) {
            clear();
            return null;
        }
    }

    /**
     * Mã hóa rồi ghi cả hai token trong một lần ghi đồng bộ.
     *
     * @return {@code true} khi đã ghi xong; {@code false} khi không mã hóa được hoặc ghi thất bại.
     *     Khi không mã hóa được, dữ liệu phiên cũ bị xóa để không còn cặp token đã bị máy chủ xoay
     *     vòng (FA 3.5)
     */
    // commit() thay vì apply(): cặp token mới phải xuống đĩa trước khi các yêu cầu được gọi lại
    // (FA 3.5, mục Ghi token mới), nên chấp nhận ghi đồng bộ.
    @SuppressLint("ApplySharedPref")
    public boolean save(@NonNull TokenPair capToken) {
        String accessMaHoa;
        String refreshMaHoa;
        try {
            accessMaHoa = maHoa(capToken.getAccessToken());
            refreshMaHoa = maHoa(capToken.getRefreshToken());
        } catch (GeneralSecurityException e) {
            clear();
            return false;
        }
        return prefs.edit()
                .putString(KHOA_ACCESS_TOKEN, accessMaHoa)
                .putString(KHOA_REFRESH_TOKEN, refreshMaHoa)
                .commit();
    }

    /** Xóa cả hai token đã lưu. */
    @SuppressLint("ApplySharedPref") // Xóa phải có hiệu lực ngay, không chờ ghi bất đồng bộ.
    public void clear() {
        prefs.edit().remove(KHOA_ACCESS_TOKEN).remove(KHOA_REFRESH_TOKEN).commit();
    }

    @NonNull
    private String maHoa(@NonNull String token) throws GeneralSecurityException {
        return cipher.encrypt(token.getBytes(StandardCharsets.UTF_8)).encode();
    }

    @NonNull
    private String giaiMa(@NonNull String chuoiDaMaHoa) throws GeneralSecurityException {
        CipherPayload payload = CipherPayload.decode(chuoiDaMaHoa);
        return new String(cipher.decrypt(payload), StandardCharsets.UTF_8);
    }
}
