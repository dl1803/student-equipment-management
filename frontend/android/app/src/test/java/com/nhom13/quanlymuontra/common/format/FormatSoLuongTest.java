package com.nhom13.quanlymuontra.common.format;

import static org.junit.Assert.assertEquals;

import com.nhom13.quanlymuontra.common.HangSo;
import org.junit.Test;

public class FormatSoLuongTest {

    @Test
    public void DS97_HaiSo_GhepDangNTrenM() {
        assertEquals("1/3", FormatSoLuong.tren(1, 3));
        assertEquals("0/5", FormatSoLuong.tren(0, 5));
    }

    @Test
    public void DXDS12_GiaTriThieu_HienThiDauGachDai() {
        assertEquals(HangSo.GIA_TRI_THIEU + "/3", FormatSoLuong.tren(null, 3));
        assertEquals("2/" + HangSo.GIA_TRI_THIEU, FormatSoLuong.tren(2, null));
    }
}
