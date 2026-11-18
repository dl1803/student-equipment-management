package com.nhom13.quanlymuontra.common.format;

import static org.junit.Assert.assertEquals;

import java.time.LocalDate;
import java.time.OffsetDateTime;
import org.junit.Test;

/** Kiểm FormatThoiGian theo UI-Screens 3.4, ĐX-UI-12 và FC 7.2, 9.3. */
public class FormatThoiGianTest {

    private static final String THIEU = "\u2014";

    @Test
    public void UI34_NgayGio_DoiUtcSangGioVietNam() {
        assertEquals(
                "15/11/2026 14:30", FormatThoiGian.ngayGio(OffsetDateTime.parse("2026-11-15T07:30:00Z")));
    }

    @Test
    public void UI34_NgayGio_QuaNuaDem_SangNgayHomSau() {
        // 17:00 UTC là 00:00 giờ Việt Nam của ngày kế tiếp; 24 giờ viết 00 chứ không phải 24.
        assertEquals(
                "16/11/2026 00:00", FormatThoiGian.ngayGio(OffsetDateTime.parse("2026-11-15T17:00:00Z")));
        assertEquals(
                "16/11/2026 00:59", FormatThoiGian.ngayGio(OffsetDateTime.parse("2026-11-15T17:59:59Z")));
    }

    @Test
    public void UI34_NgayGio_SatNuaDemTruoc_VanCungNgay() {
        assertEquals(
                "15/11/2026 23:59", FormatThoiGian.ngayGio(OffsetDateTime.parse("2026-11-15T16:59:00Z")));
    }

    @Test
    public void UI34_NgayGio_QuaNamMoi_DoiCaNam() {
        assertEquals(
                "01/01/2027 00:00", FormatThoiGian.ngayGio(OffsetDateTime.parse("2026-12-31T17:00:00Z")));
    }

    @Test
    public void UI34_NgayGio_DoBuLechKhacKhong_QuyVeGioVietNam() {
        // 20:30 giờ +07:00 đã là giờ Việt Nam; 10:00 giờ -05:00 là 15:00 UTC là 22:00 giờ Việt Nam.
        assertEquals(
                "15/11/2026 20:30", FormatThoiGian.ngayGio(OffsetDateTime.parse("2026-11-15T20:30:00+07:00")));
        assertEquals(
                "15/11/2026 22:00", FormatThoiGian.ngayGio(OffsetDateTime.parse("2026-11-15T10:00:00-05:00")));
    }

    @Test
    public void DXDS12_NgayGio_Null_HienGachDai() {
        assertEquals(THIEU, FormatThoiGian.ngayGio(null));
    }

    @Test
    public void UI34_Ngay_NgayThuan_KhongDoiMuiGio() {
        assertEquals("05/03/2026", FormatThoiGian.ngay(LocalDate.of(2026, 3, 5)));
        assertEquals("31/12/2026", FormatThoiGian.ngay(LocalDate.of(2026, 12, 31)));
    }

    @Test
    public void DXDS12_Ngay_Null_HienGachDai() {
        assertEquals(THIEU, FormatThoiGian.ngay(null));
    }

    @Test
    public void UI34_Gio_CatLayNamKyTuDau() {
        assertEquals("07:30", FormatThoiGian.gio("07:30:00"));
        assertEquals("07:30", FormatThoiGian.gio("07:30"));
        assertEquals("17:05", FormatThoiGian.gio("17:05:59.123"));
    }

    @Test
    public void FA22_Gio_ChuoiLa_TraNguyenVanKhongNemLoi() {
        assertEquals("abc", FormatThoiGian.gio("abc"));
        assertEquals("7:30", FormatThoiGian.gio("7:30"));
        assertEquals("", FormatThoiGian.gio(""));
    }

    @Test
    public void DXDS12_Gio_Null_HienGachDai() {
        assertEquals(THIEU, FormatThoiGian.gio(null));
    }
}
