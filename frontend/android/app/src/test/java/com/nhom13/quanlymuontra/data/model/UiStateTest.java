package com.nhom13.quanlymuontra.data.model;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertSame;

import org.junit.Test;

public class UiStateTest {

    @Test
    public void FC63_DangTaiVoiDuLieuCu_GiuNguyenDuLieuCu() {
        UiState<String> trangThai = UiState.dangTai("du lieu cu");

        assertEquals(UiState.TrangThai.DANG_TAI, trangThai.getTrangThai());
        assertEquals("du lieu cu", trangThai.getDuLieu());
        assertNull(trangThai.getLoi());
    }

    @Test
    public void FC63_DangTaiLanDau_KhongCoDuLieu() {
        UiState<String> trangThai = UiState.dangTai(null);

        assertEquals(UiState.TrangThai.DANG_TAI, trangThai.getTrangThai());
        assertNull(trangThai.getDuLieu());
    }

    @Test
    public void FC63_ThanhCong_CoDuLieuVaKhongCoLoi() {
        UiState<String> trangThai = UiState.thanhCong("du lieu moi");

        assertEquals(UiState.TrangThai.THANH_CONG, trangThai.getTrangThai());
        assertEquals("du lieu moi", trangThai.getDuLieu());
        assertNull(trangThai.getLoi());
    }

    @Test
    public void FC63_Rong_KhongCoDuLieuVaKhongCoLoi() {
        UiState<String> trangThai = UiState.rong();

        assertEquals(UiState.TrangThai.RONG, trangThai.getTrangThai());
        assertNull(trangThai.getDuLieu());
        assertNull(trangThai.getLoi());
    }

    @Test
    public void FC63_LoiVoiDuLieuCu_GiuApiErrorVaDuLieuCu() {
        ApiError loi = ApiError.mang();

        UiState<String> trangThai = UiState.loi(loi, "du lieu cu");

        assertEquals(UiState.TrangThai.LOI, trangThai.getTrangThai());
        assertSame(loi, trangThai.getLoi());
        assertEquals("du lieu cu", trangThai.getDuLieu());
    }
}
