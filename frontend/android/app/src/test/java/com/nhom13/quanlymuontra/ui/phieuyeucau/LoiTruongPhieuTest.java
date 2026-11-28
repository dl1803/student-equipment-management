package com.nhom13.quanlymuontra.ui.phieuyeucau;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import java.util.Arrays;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import org.junit.Test;

public class LoiTruongPhieuTest {

    private static final UUID A = UUID.fromString("00000000-0000-0000-0000-000000000001");
    private static final UUID B = UUID.fromString("00000000-0000-0000-0000-000000000002");
    private static final List<UUID> THU_TU = Arrays.asList(A, B);

    private static Map<String, List<String>> loi(String khoa, String... thongDiep) {
        Map<String, List<String>> m = new LinkedHashMap<>();
        m.put(khoa, Arrays.asList(thongDiep));
        return m;
    }

    @Test
    public void FC66_KhoaDongLoai_VeDungMaLoaiTheoChiSo() {
        LoiTruongPhieu kq =
                LoiTruongPhieu.phanTich(loi("dsLoaiVaSoLuong[1].soLuong", "Số lượng phải lớn hơn 0."), THU_TU);

        assertEquals(Collections.singleton(B), kq.getLoiSoLuong().keySet());
        assertEquals("Số lượng phải lớn hơn 0.", kq.getLoiSoLuong().get(B));
        assertNull(kq.getLoiThoiGian());
        assertTrue(kq.getLoiKhac().isEmpty());
        assertTrue(kq.coLoiTruong());
    }

    @Test
    public void FC66_KhoaKhongPhanBietHoaThuong() {
        LoiTruongPhieu kq = LoiTruongPhieu.phanTich(loi("DsLoaiVaSoLuong[0].SoLuong", "Sai."), THU_TU);

        assertEquals("Sai.", kq.getLoiSoLuong().get(A));
    }

    @Test
    public void FC66_KhoaThoiGian_VeOThoiGian() {
        LoiTruongPhieu kq =
                LoiTruongPhieu.phanTich(loi("thoiGianMuonDuKienGio", "Phải lớn hơn 0.", "Quá nhỏ."), THU_TU);

        assertEquals("Phải lớn hơn 0. Quá nhỏ.", kq.getLoiThoiGian());
        assertTrue(kq.getLoiSoLuong().isEmpty());
        assertTrue(kq.coLoiTruong());
    }

    @Test
    public void FC66_KhoaLaVaChiSoNgoaiPhamVi_VaoLoiKhac() {
        Map<String, List<String>> m = new LinkedHashMap<>();
        m.put("dsLoaiVaSoLuong[5].soLuong", Arrays.asList("Ngoài phạm vi."));
        m.put("maPhong", Arrays.asList("Không hợp lệ."));
        m.put("dsLoaiVaSoLuong", Arrays.asList("Cần ít nhất một dòng."));

        LoiTruongPhieu kq = LoiTruongPhieu.phanTich(m, THU_TU);

        assertEquals(Arrays.asList("Ngoài phạm vi.", "Không hợp lệ.", "Cần ít nhất một dòng."), kq.getLoiKhac());
        assertFalse(kq.coLoiTruong());
    }

    @Test
    public void FC66_RongHoacNull_TraRong() {
        assertFalse(LoiTruongPhieu.phanTich(null, THU_TU).coLoiTruong());
        assertFalse(LoiTruongPhieu.phanTich(Collections.<String, List<String>>emptyMap(), THU_TU).coLoiTruong());
        assertTrue(LoiTruongPhieu.rong().getLoiKhac().isEmpty());
    }

    @Test
    public void FC66_ThongDiepRongBiBo() {
        LoiTruongPhieu kq = LoiTruongPhieu.phanTich(loi("thoiGianMuonDuKienGio", "  ", ""), THU_TU);

        assertNull(kq.getLoiThoiGian());
    }
}
