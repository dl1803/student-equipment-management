package com.nhom13.quanlymuontra.data.session;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.HoTroXacThuc;
import java.time.Instant;
import org.junit.Test;

public class HanAccessTokenTest {

    private static final Instant BAY_GIO = Instant.parse("2026-11-20T03:00:00Z");
    private static final long BAY_GIO_GIAY = BAY_GIO.getEpochSecond();

    @Test
    public void FA54_ExpSauBayGio_ConHan() {
        assertTrue(HanAccessToken.conHan(HoTroXacThuc.jwtCoExp(BAY_GIO_GIAY + 1), BAY_GIO));
    }

    @Test
    public void FA54_ExpBangBayGio_DaHetHan() {
        assertFalse(HanAccessToken.conHan(HoTroXacThuc.jwtCoExp(BAY_GIO_GIAY), BAY_GIO));
    }

    @Test
    public void FA54_ExpTruocBayGio_DaHetHan() {
        assertFalse(HanAccessToken.conHan(HoTroXacThuc.jwtCoExp(BAY_GIO_GIAY - 3600), BAY_GIO));
    }

    @Test
    public void FA54_ThieuExp_CoiNhuHetHan() {
        assertFalse(HanAccessToken.conHan(HoTroXacThuc.jwtTuThan("{\"sub\":\"1\"}"), BAY_GIO));
    }

    @Test
    public void FA54_ExpLaNullHoacKhongPhaiSo_CoiNhuHetHan() {
        assertFalse(HanAccessToken.conHan(HoTroXacThuc.jwtTuThan("{\"exp\":null}"), BAY_GIO));
        assertFalse(HanAccessToken.conHan(HoTroXacThuc.jwtTuThan("{\"exp\":\"mai\"}"), BAY_GIO));
    }

    @Test
    public void FA54_ThanKhongPhaiDoiTuongJson_CoiNhuHetHan() {
        assertFalse(HanAccessToken.conHan(HoTroXacThuc.jwtTuThan("123"), BAY_GIO));
        assertFalse(HanAccessToken.conHan(HoTroXacThuc.jwtTuThan(""), BAY_GIO));
        assertFalse(HanAccessToken.conHan(HoTroXacThuc.jwtTuThan("{khong-phai-json"), BAY_GIO));
    }

    @Test
    public void FA54_KhongDungBaPhan_CoiNhuHetHan() {
        assertFalse(HanAccessToken.conHan("access-gia", BAY_GIO));
        assertFalse(HanAccessToken.conHan("a.b", BAY_GIO));
        assertFalse(HanAccessToken.conHan("a.b.c.d", BAY_GIO));
        assertFalse(HanAccessToken.conHan("", BAY_GIO));
    }

    @Test
    public void FA54_PhanThanKhongPhaiBase64_CoiNhuHetHan() {
        assertFalse(HanAccessToken.conHan("a.@@@.c", BAY_GIO));
    }
}
