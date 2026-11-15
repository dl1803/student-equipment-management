package com.nhom13.quanlymuontra.data.session;

import static org.junit.Assert.assertArrayEquals;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertThrows;

import org.junit.Test;

public class CipherPayloadTest {

    @Test
    public void DXFA14_EncodeRoiDecode_KhoiPhucDungIvVaBanMa() {
        byte[] iv = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
        byte[] banMa = {(byte) 0xFF, 0, 42, (byte) 0x80, 7};

        CipherPayload khoiPhuc = CipherPayload.decode(new CipherPayload(iv, banMa).encode());

        assertArrayEquals(iv, khoiPhuc.getIv());
        assertArrayEquals(banMa, khoiPhuc.getCipherText());
    }

    @Test
    public void DXFA14_Encode_LaBase64CuaIvDauHaiChamBase64CuaBanMa() {
        CipherPayload payload = new CipherPayload(new byte[] {1, 2, 3}, new byte[] {4, 5, 6});

        assertEquals("AQID:BAUG", payload.encode());
    }

    @Test
    public void DXFA14_SuaMangSauKhiTao_KhongLamDoiGoi() {
        byte[] iv = {1, 2, 3};
        byte[] banMa = {4, 5, 6};
        CipherPayload payload = new CipherPayload(iv, banMa);

        iv[0] = 99;
        banMa[0] = 99;
        payload.getIv()[1] = 99;
        payload.getCipherText()[1] = 99;

        assertArrayEquals(new byte[] {1, 2, 3}, payload.getIv());
        assertArrayEquals(new byte[] {4, 5, 6}, payload.getCipherText());
    }

    @Test
    public void FA54_IvHoacBanMaRong_NemIllegalArgumentException() {
        assertThrows(
                IllegalArgumentException.class,
                () -> new CipherPayload(new byte[0], new byte[] {1}));
        assertThrows(
                IllegalArgumentException.class,
                () -> new CipherPayload(new byte[] {1}, new byte[0]));
    }

    @Test
    public void FA54_ChuoiSaiDinhDang_NemIllegalArgumentException() {
        String[] chuoiHong = {
            "",
            "khongcodaungancach",
            "AQID:",
            ":BAUG",
            "AQID:BAUG:AQID",
            "!!!!:BAUG",
            "AQID:!!!!"
        };

        for (String chuoi : chuoiHong) {
            assertThrows(
                    "Chuỗi phải bị từ chối: " + chuoi,
                    IllegalArgumentException.class,
                    () -> CipherPayload.decode(chuoi));
        }
        assertThrows(IllegalArgumentException.class, () -> CipherPayload.decode(null));
    }

    @Test
    public void FC68_ThongDiepLoiVaToString_KhongChuaNoiDungDauVao() {
        String bimat = "BIMAT-TOKEN-123:phan-hai";

        IllegalArgumentException loi =
                assertThrows(IllegalArgumentException.class, () -> CipherPayload.decode(bimat));
        String toString = new CipherPayload(new byte[] {1, 2, 3}, new byte[] {4, 5, 6}).toString();

        assertFalse(loi.getMessage().contains("BIMAT"));
        assertFalse(loi.getMessage().contains("phan-hai"));
        assertFalse(toString.contains("AQID"));
        assertFalse(toString.contains("BAUG"));
    }
}
