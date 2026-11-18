package com.nhom13.quanlymuontra.common.format;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

import org.junit.Test;

/** Kiểm FormatTien theo DS 9.7, ĐX-UI-12 và FC 7.2, 9.3. */
public class FormatTienTest {

    // U+00A0 là khoảng trắng không ngắt; U+0110 là chữ Đ hoa.
    private static final String NBSP = "\u00A0";
    private static final String DON_VI = "VN\u0110";

    @Test
    public void DS97_Tien_DauChamPhanCachHangNghin_KhoangTrangKhongNgat() {
        assertEquals("240.000" + NBSP + DON_VI, FormatTien.tien(240000L));
    }

    @Test
    public void DS97_Tien_KhoangTrangGiuaSoVaDonViLaU00A0_KhongPhaiDauCachThuong() {
        String ketQua = FormatTien.tien(1500000L);
        assertEquals("1.500.000" + NBSP + DON_VI, ketQua);
        assertTrue(ketQua.indexOf(' ') < 0);
        assertEquals(ketQua.length() - 4, ketQua.indexOf('\u00A0'));
    }

    @Test
    public void DS97_Tien_KhongVaSoNho_KhongCoDauNhom() {
        assertEquals("0" + NBSP + DON_VI, FormatTien.tien(0L));
        assertEquals("999" + NBSP + DON_VI, FormatTien.tien(999L));
        assertEquals("1.000" + NBSP + DON_VI, FormatTien.tien(1000L));
    }

    @Test
    public void DS97_Tien_SoRatLon_VanDungDinhDang() {
        assertEquals("9.223.372.036.854.775.807" + NBSP + DON_VI, FormatTien.tien(Long.MAX_VALUE));
    }

    @Test
    public void DS97_Tien_KhongCoPhanThapPhan() {
        assertEquals("2.500.000" + NBSP + DON_VI, FormatTien.tien(2500000L));
        assertTrue(FormatTien.tien(2500000L).indexOf(',') < 0);
    }

    @Test
    public void DS97_Tien_Long_Null_HienGachDai() {
        assertEquals("\u2014", FormatTien.tien((Long) null));
    }

    @Test
    public void DS97_Tien_LongKhacNull_NhuSoNguyenThuy() {
        assertEquals(FormatTien.tien(240000L), FormatTien.tien(Long.valueOf(240000L)));
    }
}
