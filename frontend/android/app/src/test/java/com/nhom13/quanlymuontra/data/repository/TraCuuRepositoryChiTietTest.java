package com.nhom13.quanlymuontra.data.repository;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiChiTiet;
import java.util.UUID;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import okhttp3.HttpUrl;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import okhttp3.mockwebserver.RecordedRequest;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/** Kiểm đường dẫn, tên tham số và chuẩn hóa lỗi của {@link TraCuuRepository#svTraCuuThietBiChiTiet} (UI-Screens 5.3). */
public class TraCuuRepositoryChiTietTest {

    private static final UUID MA_LOAI = UUID.fromString("11111111-1111-1111-1111-111111111111");
    private static final UUID MA_PHONG = UUID.fromString("22222222-2222-2222-2222-222222222222");

    private MockWebServer mayChu;
    private TraCuuRepository kho;

    @Before
    public void chuanBi() throws Exception {
        mayChu = new MockWebServer();
        mayChu.start();
        Retrofit retrofit =
                new Retrofit.Builder()
                        .baseUrl(mayChu.url("/"))
                        .addConverterFactory(GsonConverterFactory.create(new JSON().getGson()))
                        .build();
        kho = new TraCuuRepository(retrofit.create(SinhVienApi.class));
    }

    @After
    public void don() throws Exception {
        mayChu.shutdown();
    }

    @Test
    public void UISV03_ChiTietConHang_GuiDungDuongDanThamSoVaGiaiMa() throws Exception {
        mayChu.enqueue(
                new MockResponse()
                        .setBody(Fixtures.doc("thanh-cong/sv-tra-cuu-chi-tiet-con-hang.json")));
        ThuThap<TraCuuThietBiChiTiet> thuThap = new ThuThap<>();

        kho.svTraCuuThietBiChiTiet(MA_LOAI, MA_PHONG, thuThap);
        thuThap.cho();

        RecordedRequest yeuCau = mayChu.takeRequest(5, TimeUnit.SECONDS);
        assertNotNull(yeuCau);
        HttpUrl url = yeuCau.getRequestUrl();
        assertEquals("GET", yeuCau.getMethod());
        assertEquals("/api/v1/sv/tra-cuu/thiet-bi/" + MA_LOAI, url.encodedPath());
        assertEquals(MA_PHONG.toString(), url.queryParameter("maPhong"));

        TraCuuThietBiChiTiet chiTiet = thuThap.ketQua;
        assertNotNull(chiTiet);
        assertEquals("Laptop Dell Latitude 5420", chiTiet.getTenLoai());
        assertEquals("Dell", chiTiet.getHangSanXuat());
        assertEquals(Integer.valueOf(5), chiTiet.getSoLuongKhaDung());
        assertEquals(2, chiTiet.getDsThongSoKyThuat().size());
        assertEquals(2, chiTiet.getDsAnh().size());
        assertNull(chiTiet.getNgayDuKienCoSan());
    }

    @Test
    public void UISV03_ChiTietHetHang_GiaiMaNgayDuKienVaKhoiRong() throws Exception {
        mayChu.enqueue(
                new MockResponse()
                        .setBody(Fixtures.doc("thanh-cong/sv-tra-cuu-chi-tiet-het-hang.json")));
        ThuThap<TraCuuThietBiChiTiet> thuThap = new ThuThap<>();

        kho.svTraCuuThietBiChiTiet(MA_LOAI, MA_PHONG, thuThap);
        thuThap.cho();

        TraCuuThietBiChiTiet chiTiet = thuThap.ketQua;
        assertNotNull(chiTiet);
        assertEquals(Integer.valueOf(0), chiTiet.getSoLuongKhaDung());
        assertNull(chiTiet.getMoTa());
        assertTrue(chiTiet.getDsThongSoKyThuat().isEmpty());
        assertTrue(chiTiet.getDsAnh().isEmpty());
        assertNotNull(chiTiet.getNgayDuKienCoSan());
    }

    @Test
    public void UISV03_ChiTiet404_ChuanHoaThanhLoiHttpKhongTimThay() throws Exception {
        mayChu.enqueue(
                new MockResponse()
                        .setResponseCode(404)
                        .addHeader("Content-Type", "application/problem+json")
                        .setBody(Fixtures.doc("loi/404-khong-tim-thay.json")));
        ThuThap<TraCuuThietBiChiTiet> thuThap = new ThuThap<>();

        kho.svTraCuuThietBiChiTiet(MA_LOAI, MA_PHONG, thuThap);
        thuThap.cho();

        assertNull(thuThap.ketQua);
        assertNotNull(thuThap.loi);
        assertEquals(ApiError.Loai.HTTP, thuThap.loi.getLoai());
        assertEquals(Integer.valueOf(404), thuThap.loi.getStatus());
        assertEquals("KHONG_TIM_THAY", thuThap.loi.getMaLoi());
    }

    /** Thu kết quả của {@link ApiCallback} và chờ tối đa 5 giây. */
    private static final class ThuThap<T> implements ApiCallback<T> {
        @Nullable T ketQua;
        @Nullable ApiError loi;
        private final CountDownLatch xong = new CountDownLatch(1);

        @Override
        public void onSuccess(@Nullable T ketQuaNhan) {
            ketQua = ketQuaNhan;
            xong.countDown();
        }

        @Override
        public void onError(@NonNull ApiError loiNhan) {
            loi = loiNhan;
            xong.countDown();
        }

        void cho() throws InterruptedException {
            assertTrue("Hết thời gian chờ phản hồi", xong.await(5, TimeUnit.SECONDS));
        }
    }
}
