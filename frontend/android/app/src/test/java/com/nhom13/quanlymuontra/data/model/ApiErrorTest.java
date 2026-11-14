package com.nhom13.quanlymuontra.data.model;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import org.junit.Test;

public class ApiErrorTest {

    @Test
    public void FA33_Http_GiuNguyenStatusMaLoiDetailVaTraceId() {
        ApiError loi = ApiError.http(409, "MA_LOI_CHUA_TUNG_THAY", "chi tiet", "trace-1", null);

        assertEquals(ApiError.Loai.HTTP, loi.getLoai());
        assertEquals(Integer.valueOf(409), loi.getStatus());
        assertEquals("MA_LOI_CHUA_TUNG_THAY", loi.getMaLoi());
        assertEquals("chi tiet", loi.getDetail());
        assertEquals("trace-1", loi.getTraceId());
    }

    @Test
    public void FA33_HttpKhongCoMaLoi_MaLoiLaNull() {
        ApiError loi = ApiError.http(500, null, null, "trace-2", null);

        assertNull(loi.getMaLoi());
        assertEquals("trace-2", loi.getTraceId());
    }

    @Test
    public void FA33_HttpKhongCoErrors_TraVeBanDoRong() {
        ApiError loi = ApiError.http(401, "CHUA_XAC_THUC", null, "trace-3", null);

        assertTrue(loi.getErrors().isEmpty());
    }

    @Test
    public void FA33_HttpCoErrors_SuaBanDoGocKhongLamDoiApiError() {
        Map<String, List<String>> goc = new HashMap<>();
        goc.put("dsLoaiVaSoLuong[0].soLuong", new ArrayList<>(Arrays.asList("Phai lon hon 0.")));

        ApiError loi = ApiError.http(400, "DU_LIEU_KHONG_HOP_LE", null, "trace-4", goc);
        goc.get("dsLoaiVaSoLuong[0].soLuong").add("Them sau");
        goc.put("truongKhac", new ArrayList<String>());

        assertEquals(1, loi.getErrors().size());
        assertEquals(1, loi.getErrors().get("dsLoaiVaSoLuong[0].soLuong").size());
    }

    @Test
    public void FA33_HttpCoErrors_KhongSuaDuocTuBenNgoai() {
        Map<String, List<String>> goc = new HashMap<>();
        goc.put("truong", new ArrayList<>(Arrays.asList("loi")));
        ApiError loi = ApiError.http(400, "DU_LIEU_KHONG_HOP_LE", null, "trace-5", goc);

        assertThrows(
                UnsupportedOperationException.class,
                () -> loi.getErrors().put("truongMoi", new ArrayList<String>()));
        assertThrows(
                UnsupportedOperationException.class,
                () -> loi.getErrors().get("truong").add("them"));
    }

    @Test
    public void FA33_MangQuaThoiGianVaHuy_ChiCoLoaiVaKhongCoPhanHoiHttp() {
        ApiError[] cacLoi = {ApiError.mang(), ApiError.quaThoiGian(), ApiError.huy()};
        ApiError.Loai[] cacLoai = {
            ApiError.Loai.MANG, ApiError.Loai.QUA_THOI_GIAN, ApiError.Loai.HUY
        };

        for (int i = 0; i < cacLoi.length; i++) {
            assertEquals(cacLoai[i], cacLoi[i].getLoai());
            assertNull(cacLoi[i].getStatus());
            assertNull(cacLoi[i].getMaLoi());
            assertNull(cacLoi[i].getTraceId());
            assertTrue(cacLoi[i].getErrors().isEmpty());
        }
    }
}
