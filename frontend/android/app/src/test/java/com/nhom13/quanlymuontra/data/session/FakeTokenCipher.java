package com.nhom13.quanlymuontra.data.session;

import androidx.annotation.NonNull;
import java.security.GeneralSecurityException;
import javax.crypto.AEADBadTagException;

/**
 * Ranh giới mã hóa giả cho kiểm thử JVM (FA 5.10, FC 9.1): đổi byte có thể đảo ngược và gắn một
 * byte kiểm tra để phát hiện bản mã bị sửa. Không an toàn và không chứng minh gì về Android
 * Keystore thật.
 */
final class FakeTokenCipher implements TokenCipher {

    private static final int MAT_NA = 0x5A;
    private static final int MAT_NA_KIEM_TRA = 0x33;

    boolean loiKhiMaHoa = false;
    boolean loiKhiGiaiMa = false;
    private int boDemIv = 0;

    @NonNull
    @Override
    public CipherPayload encrypt(@NonNull byte[] plainText) throws GeneralSecurityException {
        if (loiKhiMaHoa) {
            throw new GeneralSecurityException("Khóa giả bị mất khi mã hóa");
        }
        byte[] iv = new byte[12];
        iv[0] = (byte) ++boDemIv;
        byte[] cipherText = new byte[plainText.length + 1];
        int kiemTra = MAT_NA_KIEM_TRA;
        for (int i = 0; i < plainText.length; i++) {
            cipherText[i] = (byte) (plainText[i] ^ MAT_NA);
            kiemTra ^= plainText[i];
        }
        cipherText[plainText.length] = (byte) kiemTra;
        return new CipherPayload(iv, cipherText);
    }

    @NonNull
    @Override
    public byte[] decrypt(@NonNull CipherPayload payload) throws GeneralSecurityException {
        if (loiKhiGiaiMa) {
            throw new GeneralSecurityException("Khóa giả bị mất khi giải mã");
        }
        byte[] cipherText = payload.getCipherText();
        byte[] plainText = new byte[cipherText.length - 1];
        int kiemTra = MAT_NA_KIEM_TRA;
        for (int i = 0; i < plainText.length; i++) {
            plainText[i] = (byte) (cipherText[i] ^ MAT_NA);
            kiemTra ^= plainText[i];
        }
        if ((byte) kiemTra != cipherText[plainText.length]) {
            throw new AEADBadTagException("Bản mã giả không khớp thẻ xác thực");
        }
        return plainText;
    }
}
