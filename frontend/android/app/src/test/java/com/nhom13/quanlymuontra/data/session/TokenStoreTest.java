package com.nhom13.quanlymuontra.data.session;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import java.util.Map;
import org.junit.Before;
import org.junit.Test;

public class TokenStoreTest {

    private static final String ACCESS = "access-token-bi-mat-AAA";
    private static final String REFRESH = "refresh-token-bi-mat-BBB";
    private static final String KHOA_ACCESS = "accessToken";
    private static final String KHOA_REFRESH = "refreshToken";

    private FakeSharedPreferences prefs;
    private FakeTokenCipher cipher;
    private TokenStore store;

    @Before
    public void chuanBi() {
        prefs = new FakeSharedPreferences();
        cipher = new FakeTokenCipher();
        store = new TokenStore(prefs, cipher);
    }

    @Test
    public void DXFA14_LuuRoiDoc_TraDungCapToken() {
        assertTrue(store.save(new TokenPair(ACCESS, REFRESH)));

        TokenPair daDoc = store.read();

        assertNotNull(daDoc);
        assertEquals(ACCESS, daDoc.getAccessToken());
        assertEquals(REFRESH, daDoc.getRefreshToken());
    }

    @Test
    public void DXFA14_LuuTokenCoDauVaKyTuDacBiet_DocLaiNguyenVan() {
        String access = "tiếng-việt-\u0111-/+=-ünï";
        assertTrue(store.save(new TokenPair(access, REFRESH)));

        TokenPair daDoc = store.read();

        assertNotNull(daDoc);
        assertEquals(access, daDoc.getAccessToken());
    }

    @Test
    public void DXFA14_GiaTriDaGhi_LaGoiMaHoaVaKhongChuaTokenDangRo() {
        store.save(new TokenPair(ACCESS, REFRESH));

        Map<String, String> daGhi = prefs.saoChepDuLieu();

        assertEquals(2, daGhi.size());
        for (String giaTri : daGhi.values()) {
            assertFalse(giaTri.contains(ACCESS));
            assertFalse(giaTri.contains(REFRESH));
            CipherPayload.decode(giaTri); // đúng định dạng gói, không ném lỗi
        }
    }

    @Test
    public void FA35_Luu_GhiHaiGiaTriTrongMotLanCommitDongBoKhongDungApply() {
        store.save(new TokenPair(ACCESS, REFRESH));

        assertEquals(1, prefs.soLanCommit);
        assertEquals(0, prefs.soLanApply);
        assertTrue(prefs.contains(KHOA_ACCESS));
        assertTrue(prefs.contains(KHOA_REFRESH));
    }

    @Test
    public void FA35_GhiDiaThatBai_LuuTraFalse() {
        prefs.commitThanhCong = false;

        assertFalse(store.save(new TokenPair(ACCESS, REFRESH)));
    }

    @Test
    public void FA54_ChuaCoPhien_DocTraNull() {
        assertNull(store.read());
        assertEquals(0, prefs.soLanCommit);
    }

    @Test
    public void FA54_BanMaBiSua_DocTraNullVaXoaDuLieu() {
        store.save(new TokenPair(ACCESS, REFRESH));
        PayloadHong.suaBanMa(prefs, KHOA_ACCESS);

        assertNull(store.read());
        assertTrue(prefs.saoChepDuLieu().isEmpty());
    }

    @Test
    public void FA54_KhoaMatKhiGiaiMa_DocTraNullVaXoaDuLieu() {
        store.save(new TokenPair(ACCESS, REFRESH));
        cipher.loiKhiGiaiMa = true;

        assertNull(store.read());
        assertTrue(prefs.saoChepDuLieu().isEmpty());
    }

    @Test
    public void FA54_GiaTriKhongPhaiGoiMaHoa_DocTraNullVaXoaDuLieu() {
        prefs.datTruc(KHOA_ACCESS, "chuoi-khong-phai-goi-ma-hoa");
        prefs.datTruc(KHOA_REFRESH, "AQID:BAUG");

        assertNull(store.read());
        assertTrue(prefs.saoChepDuLieu().isEmpty());
    }

    @Test
    public void FA54_ThieuMotTrongHaiGiaTri_DocTraNullVaXoaGiaTriConLai() {
        store.save(new TokenPair(ACCESS, REFRESH));
        prefs.edit().remove(KHOA_REFRESH).commit();

        assertNull(store.read());
        assertTrue(prefs.saoChepDuLieu().isEmpty());
    }

    @Test
    public void FA54_KhongMaHoaDuoc_LuuTraFalseVaXoaPhienCu() {
        store.save(new TokenPair("access-cu", "refresh-cu"));
        cipher.loiKhiMaHoa = true;

        assertFalse(store.save(new TokenPair(ACCESS, REFRESH)));

        assertTrue(prefs.saoChepDuLieu().isEmpty());
    }

    @Test
    public void FA54_Clear_XoaCaHaiGiaTriVaDocTraNull() {
        store.save(new TokenPair(ACCESS, REFRESH));

        store.clear();

        assertTrue(prefs.saoChepDuLieu().isEmpty());
        assertNull(store.read());
    }

    @Test
    public void FC68_ToStringCuaCapToken_KhongChuaGiaTriToken() {
        String chuoi = new TokenPair(ACCESS, REFRESH).toString();

        assertFalse(chuoi.contains(ACCESS));
        assertFalse(chuoi.contains(REFRESH));
    }

    /** Dựng dữ liệu hỏng bằng cách đổi một ký tự Base64 của bản mã đã lưu. */
    private static final class PayloadHong {

        static void suaBanMa(FakeSharedPreferences prefs, String khoa) {
            CipherPayload goc = CipherPayload.decode(prefs.getString(khoa, null));
            byte[] banMa = goc.getCipherText();
            banMa[0] ^= 0x01;
            prefs.datTruc(khoa, new CipherPayload(goc.getIv(), banMa).encode());
        }
    }
}
