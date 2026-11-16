package com.nhom13.quanlymuontra.data.api;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.generated.api.XacThucApi;
import java.lang.reflect.Method;
import okhttp3.HttpUrl;
import org.junit.Test;
import retrofit2.http.POST;

public class DuongDanApiTest {

    private static HttpUrl url(String duongDan) {
        return HttpUrl.get("http://localhost" + duongDan);
    }

    @Test
    public void FA35_BonEndpointCongKhai_KhongCanToken() {
        assertTrue(DuongDanApi.khongCanToken(url(DuongDanApi.DANG_NHAP_SINH_VIEN)));
        assertTrue(DuongDanApi.khongCanToken(url(DuongDanApi.DANG_NHAP_NHAN_VIEN)));
        assertTrue(DuongDanApi.khongCanToken(url(DuongDanApi.LAM_MOI)));
        assertTrue(DuongDanApi.khongCanToken(url(DuongDanApi.DANG_XUAT)));
    }

    @Test
    public void FA35_EndpointKhac_CanToken() {
        assertFalse(DuongDanApi.khongCanToken(url("/api/v1/sv/toi")));
        assertFalse(DuongDanApi.khongCanToken(url("/api/v1/auth/toi")));
        assertFalse(DuongDanApi.khongCanToken(url("/api/v1/auth/lam-moi/khac")));
    }

    // Đối chiếu hằng số với chú thích @POST của client sinh từ OpenAPI. Mã sinh đã chứa sẵn
    // `api/v1/...` (mâu thuẫn M1 trong CHECKLIST.md), nên địa chỉ gốc không được thêm /api/v1.
    @Test
    public void FA32_HangSoDuongDan_KhopChuThichCuaMaSinh() {
        assertEquals(DuongDanApi.DANG_NHAP_SINH_VIEN, duongDanPost("authDangNhapSinhVien"));
        assertEquals(DuongDanApi.DANG_NHAP_NHAN_VIEN, duongDanPost("authDangNhapNhanVien"));
        assertEquals(DuongDanApi.LAM_MOI, duongDanPost("authLamMoi"));
        assertEquals(DuongDanApi.DANG_XUAT, duongDanPost("authDangXuat"));
    }

    private static String duongDanPost(String tenThaoTac) {
        for (Method phuongThuc : XacThucApi.class.getMethods()) {
            if (phuongThuc.getName().equals(tenThaoTac)) {
                POST post = phuongThuc.getAnnotation(POST.class);
                assertNotNull(tenThaoTac, post);
                String giaTri = post.value();
                return giaTri.startsWith("/") ? giaTri : "/" + giaTri;
            }
        }
        throw new AssertionError("Không tìm thấy thao tác " + tenThaoTac + " trong XacThucApi");
    }
}
