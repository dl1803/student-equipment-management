package com.nhom13.quanlymuontra.common.widget;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;

import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.data.model.ApiError;
import org.junit.Test;

/**
 * Kiểm quy tắc chọn nội dung ErrorView theo DS 9.4, 9.8 và FA 3.3, 3.4. Việc từng vùng có ErrorView
 * riêng (lỗi một vùng không che vùng khác, ĐX-UI-04) ở mức giao diện được kiểm thủ công trên thiết
 * bị; ở đây kiểm phần quy tắc: kết quả chỉ phụ thuộc lỗi truyền vào.
 */
public class TrinhBayLoiTest {

    @Test
    public void DS94_MatKetNoi_LoiMangKhongCoMaTheoDoi() {
        TrinhBayLoi.NoiDung noiDung = TrinhBayLoi.choVung(ApiError.mang());
        assertNotNull(noiDung);
        assertEquals(TrinhBayLoi.Kieu.MANG, noiDung.kieu);
        assertEquals(R.string.loi_mang, noiDung.thongDiepResId);
        assertNull(noiDung.traceId);
    }

    @Test
    public void DS94_QuaThoiGianCho_LoiMang() {
        TrinhBayLoi.NoiDung noiDung = TrinhBayLoi.choVung(ApiError.quaThoiGian());
        assertNotNull(noiDung);
        assertEquals(TrinhBayLoi.Kieu.MANG, noiDung.kieu);
        assertEquals(R.string.loi_mang, noiDung.thongDiepResId);
    }

    @Test
    public void DS94_Http500_LoiHeThongKemTraceId() {
        TrinhBayLoi.NoiDung noiDung =
                TrinhBayLoi.choVung(ApiError.http(500, null, null, "00-abc-01", null));
        assertNotNull(noiDung);
        assertEquals(TrinhBayLoi.Kieu.HE_THONG, noiDung.kieu);
        assertEquals(R.string.loi_he_thong, noiDung.thongDiepResId);
        assertEquals("00-abc-01", noiDung.traceId);
    }

    @Test
    public void DS94_Http500KhongCoTraceId_VanHienLoiHeThong() {
        TrinhBayLoi.NoiDung noiDung = TrinhBayLoi.choVung(ApiError.http(500, null, null, null, null));
        assertNotNull(noiDung);
        assertEquals(TrinhBayLoi.Kieu.HE_THONG, noiDung.kieu);
        assertNull(noiDung.traceId);
    }

    @Test
    public void DS94_TraceIdRong_KhongHienDongMaTheoDoi() {
        TrinhBayLoi.NoiDung noiDung = TrinhBayLoi.choVung(ApiError.http(500, null, null, "", null));
        assertNotNull(noiDung);
        assertNull(noiDung.traceId);
    }

    @Test
    public void FA34_PhanHoiKhongPhaiProblemJson_XuLyNhuLoiHeThongChung() {
        // 502 từ cổng trung gian: không có maLoi, không có traceId của máy chủ.
        TrinhBayLoi.NoiDung noiDung = TrinhBayLoi.choVung(ApiError.http(502, null, null, null, null));
        assertNotNull(noiDung);
        assertEquals(TrinhBayLoi.Kieu.HE_THONG, noiDung.kieu);
    }

    @Test
    public void FA34_MaLoiKhongCoXuLyRieng_ThongBaoChungKemTraceId() {
        TrinhBayLoi.NoiDung noiDung =
                TrinhBayLoi.choVung(ApiError.http(403, "MA_LOI_CHUA_BIET", "x", "00-xyz-01", null));
        assertNotNull(noiDung);
        assertEquals(TrinhBayLoi.Kieu.HE_THONG, noiDung.kieu);
        assertEquals("00-xyz-01", noiDung.traceId);
    }

    @Test
    public void FA33_Huy_KhongCoGiDeHienThi() {
        assertNull(TrinhBayLoi.choVung(ApiError.huy()));
    }

    @Test
    public void DXUI04_LoiTungVung_KetQuaChiPhuThuocLoiTruyenVao() {
        // Hai vùng của cùng một màn hình lỗi khác nhau: gọi xen kẽ vẫn ra đúng nội dung từng vùng.
        ApiError loiVungMot = ApiError.mang();
        ApiError loiVungHai = ApiError.http(500, null, null, "00-hai-01", null);

        TrinhBayLoi.NoiDung truoc = TrinhBayLoi.choVung(loiVungMot);
        TrinhBayLoi.NoiDung vungHai = TrinhBayLoi.choVung(loiVungHai);
        TrinhBayLoi.NoiDung sau = TrinhBayLoi.choVung(loiVungMot);

        assertEquals(TrinhBayLoi.Kieu.HE_THONG, vungHai.kieu);
        assertEquals("00-hai-01", vungHai.traceId);
        assertEquals(TrinhBayLoi.Kieu.MANG, truoc.kieu);
        assertEquals(TrinhBayLoi.Kieu.MANG, sau.kieu);
        assertNull(sau.traceId);
    }
}
