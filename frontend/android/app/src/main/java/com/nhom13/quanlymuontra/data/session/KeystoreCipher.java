package com.nhom13.quanlymuontra.data.session;

import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;
import androidx.annotation.NonNull;
import java.io.IOException;
import java.security.GeneralSecurityException;
import java.security.Key;
import java.security.KeyStore;
import java.security.ProviderException;
import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.GCMParameterSpec;

/**
 * Mã hóa token bằng khóa AES-256 trong Android Keystore, chế độ {@code AES/GCM/NoPadding}, IV ngẫu
 * nhiên cho mỗi lần mã hóa (FA 5.4, ĐX-FA-14, FC 6.5). Đây là lớp duy nhất dùng {@code
 * AndroidKeyStore}.
 *
 * <p>Lớp này chỉ chạy được trên thiết bị hoặc emulator; kiểm thử JVM không chứng minh được nó (FA
 * 5.10). Mọi lỗi được trả dưới dạng {@link GeneralSecurityException}.
 */
public final class KeystoreCipher implements TokenCipher {

    private static final String ANDROID_KEYSTORE = "AndroidKeyStore";

    /** Bí danh khóa AES của phiên trong Android Keystore (FC 6.5: hằng số trong lớp này). */
    private static final String BI_DANH_KHOA = "com.nhom13.quanlymuontra.phien.aes";

    private static final String CHUYEN_DOI = "AES/GCM/NoPadding";
    private static final int DO_DAI_KHOA_BIT = 256;
    private static final int DO_DAI_THE_XAC_THUC_BIT = 128;

    @NonNull
    @Override
    public CipherPayload encrypt(@NonNull byte[] plainText) throws GeneralSecurityException {
        try {
            Cipher cipher = Cipher.getInstance(CHUYEN_DOI);
            // Không truyền IV: Keystore tự sinh IV ngẫu nhiên cho mỗi lần mã hóa.
            cipher.init(Cipher.ENCRYPT_MODE, layHoacTaoKhoa());
            byte[] cipherText = cipher.doFinal(plainText);
            return new CipherPayload(cipher.getIV(), cipherText);
        } catch (IOException | ProviderException e) {
            throw new GeneralSecurityException("Không mã hóa được bằng Android Keystore", e);
        }
    }

    @NonNull
    @Override
    public byte[] decrypt(@NonNull CipherPayload payload) throws GeneralSecurityException {
        try {
            Key khoa = moKeystore().getKey(BI_DANH_KHOA, null);
            if (khoa == null) {
                throw new GeneralSecurityException("Khóa phiên không còn trong Android Keystore");
            }
            Cipher cipher = Cipher.getInstance(CHUYEN_DOI);
            cipher.init(
                    Cipher.DECRYPT_MODE,
                    khoa,
                    new GCMParameterSpec(DO_DAI_THE_XAC_THUC_BIT, payload.getIv()));
            return cipher.doFinal(payload.getCipherText());
        } catch (IOException | ProviderException e) {
            throw new GeneralSecurityException("Không giải mã được bằng Android Keystore", e);
        }
    }

    @NonNull
    private static KeyStore moKeystore() throws GeneralSecurityException, IOException {
        KeyStore keyStore = KeyStore.getInstance(ANDROID_KEYSTORE);
        keyStore.load(null);
        return keyStore;
    }

    @NonNull
    private static SecretKey layHoacTaoKhoa() throws GeneralSecurityException, IOException {
        Key khoaHienCo = moKeystore().getKey(BI_DANH_KHOA, null);
        if (khoaHienCo instanceof SecretKey) {
            return (SecretKey) khoaHienCo;
        }
        KeyGenerator boSinhKhoa =
                KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, ANDROID_KEYSTORE);
        boSinhKhoa.init(
                new KeyGenParameterSpec.Builder(
                                BI_DANH_KHOA,
                                KeyProperties.PURPOSE_ENCRYPT | KeyProperties.PURPOSE_DECRYPT)
                        .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                        .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                        .setKeySize(DO_DAI_KHOA_BIT)
                        .build());
        return boSinhKhoa.generateKey();
    }
}
