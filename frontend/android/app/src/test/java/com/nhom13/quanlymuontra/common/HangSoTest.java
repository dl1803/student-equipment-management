package com.nhom13.quanlymuontra.common;

import static org.junit.Assert.assertEquals;

import org.junit.Test;

/** Giữ giá trị các hằng số dùng chung đúng tài liệu nguồn (FC 7.1). */
public class HangSoTest {

    @Test
    public void DXFA05_DoTreGoTimKiem_Bang400MiliGiay() {
        assertEquals(400, HangSo.DO_TRE_GO_TIM_KIEM_MS);
    }

    @Test
    public void UISV3_KichThuocTrang_MacDinh20ToiDa100() {
        assertEquals(20, HangSo.KICH_THUOC_TRANG);
        assertEquals(100, HangSo.KICH_THUOC_TRANG_TOI_DA);
    }
}
