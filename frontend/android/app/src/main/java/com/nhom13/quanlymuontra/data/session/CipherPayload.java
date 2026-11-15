package com.nhom13.quanlymuontra.data.session;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import java.util.Base64;

/**
 * Gói IV và bản mã của một lần mã hóa AES-GCM, đổi qua lại với một chuỗi để ghi vào {@code
 * SharedPreferences} (FA 5.4, ĐX-FA-14).
 *
 * <p>Dạng chuỗi là {@code Base64(IV)}, dấu {@code ':'}, rồi {@code Base64(bản mã)}. Dấu {@code ':'}
 * không thuộc bảng chữ Base64 chuẩn nên không gây nhập nhằng. Đối tượng bất biến. {@link
 * #toString()} và thông điệp của mọi ngoại lệ không chứa nội dung gói (FA 5.4, mục Nhật ký).
 */
public final class CipherPayload {

    private static final char DAU_NGAN_CACH = ':';
    private static final String LOI_DINH_DANG = "Chuỗi không đúng định dạng gói mã hóa";

    @NonNull private final byte[] iv;
    @NonNull private final byte[] cipherText;

    /**
     * @throws IllegalArgumentException khi {@code iv} hoặc {@code cipherText} rỗng
     */
    public CipherPayload(@NonNull byte[] iv, @NonNull byte[] cipherText) {
        if (iv.length == 0 || cipherText.length == 0) {
            throw new IllegalArgumentException("IV và bản mã không được rỗng");
        }
        this.iv = iv.clone();
        this.cipherText = cipherText.clone();
    }

    /** Bản sao của IV. */
    @NonNull
    public byte[] getIv() {
        return iv.clone();
    }

    /** Bản sao của bản mã. */
    @NonNull
    public byte[] getCipherText() {
        return cipherText.clone();
    }

    /** Đổi gói thành chuỗi để lưu. */
    @NonNull
    public String encode() {
        Base64.Encoder boMaHoa = Base64.getEncoder();
        return boMaHoa.encodeToString(iv) + DAU_NGAN_CACH + boMaHoa.encodeToString(cipherText);
    }

    /**
     * Đọc lại gói từ chuỗi do {@link #encode()} tạo ra.
     *
     * @throws IllegalArgumentException khi chuỗi {@code null}, thiếu dấu ngăn cách, sai Base64 (kể
     *     cả thừa dấu ngăn cách, vì {@code ':'} không hợp lệ trong Base64) hoặc có phần rỗng;
     *     thông điệp lỗi không chứa nội dung chuỗi
     */
    @NonNull
    public static CipherPayload decode(@Nullable String chuoi) {
        if (chuoi == null) {
            throw new IllegalArgumentException(LOI_DINH_DANG);
        }
        int viTri = chuoi.indexOf(DAU_NGAN_CACH);
        if (viTri <= 0 || viTri == chuoi.length() - 1) {
            throw new IllegalArgumentException(LOI_DINH_DANG);
        }
        try {
            byte[] iv = Base64.getDecoder().decode(chuoi.substring(0, viTri));
            byte[] cipherText = Base64.getDecoder().decode(chuoi.substring(viTri + 1));
            return new CipherPayload(iv, cipherText);
        } catch (IllegalArgumentException e) {
            // Cố ý không gắn nguyên nhân: thông điệp của bộ giải mã Base64 có thể lộ ký tự của dữ liệu.
            throw new IllegalArgumentException(LOI_DINH_DANG);
        }
    }

    @NonNull
    @Override
    public String toString() {
        return "CipherPayload[đã ẩn]";
    }
}
