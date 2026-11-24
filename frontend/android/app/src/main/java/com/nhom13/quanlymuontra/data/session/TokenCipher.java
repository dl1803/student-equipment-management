package com.nhom13.quanlymuontra.data.session;

import androidx.annotation.NonNull;
import java.security.GeneralSecurityException;

/**
 * Ranh giới mã hóa của lưu phiên (FA 5.4, FC 6.5): {@link TokenStore} chỉ biết giao diện này nên
 * kiểm thử trên JVM thay bằng bản giả; bản thật là {@link KeystoreCipher}.
 *
 * <p>Mọi lỗi mã hóa hoặc giải mã (khóa mất, bản mã hỏng, Keystore không dùng được) đều là {@link
 * GeneralSecurityException}.
 */
public interface TokenCipher {

    /** Mã hóa {@code plainText}; IV do bên cài đặt sinh ngẫu nhiên cho mỗi lần gọi. */
    @NonNull
    CipherPayload encrypt(@NonNull byte[] plainText) throws GeneralSecurityException;

    /** Giải mã {@code payload}; ném lỗi khi khóa mất hoặc bản mã bị sửa/hỏng. */
    @NonNull
    byte[] decrypt(@NonNull CipherPayload payload) throws GeneralSecurityException;
}
