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
import com.nhom13.quanlymuontra.generated.model.LoaiThietBiTomTatPage;
import com.nhom13.quanlymuontra.generated.model.PhongThietBiPage;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItemPage;
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

/** Kiểm đường dẫn, tên tham số và chuẩn hóa lỗi của {@link TraCuuRepository} (UI-Screens 5.2, FA 3.3). */
public class TraCuuRepositoryTest {

    private static final UUID MA_LOAI = UUID.fromString("11111111-1111-1111-1111-111111111111");
    private static final UUID MA_PHONG = UUID.fromString("22222222-2222-2222-2222-222222222222");
    private static final String TRANG_RONG =
            "{\"tongSo\":0,\"trang\":1,\"kichThuoc\":20,\"danhSach\":[]}";

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
    public void UISV02_TraCuuDayDuBoLoc_GuiDungDuongDanVaTenThamSo() throws Exception {
        mayChu.enqueue(new MockResponse().setBody(TRANG_RONG));
        ThuThap<TraCuuThietBiItemPage> thuThap = new ThuThap<>();

        kho.svTraCuuThietBi("laptop", MA_LOAI, MA_PHONG, Boolean.TRUE, 2, 20, thuThap);
        thuThap.cho();

        RecordedRequest yeuCau = mayChu.takeRequest(5, TimeUnit.SECONDS);
        assertNotNull(yeuCau);
        HttpUrl url = yeuCau.getRequestUrl();
        assertEquals("GET", yeuCau.getMethod());
        assertEquals("/api/v1/sv/tra-cuu/thiet-bi", url.encodedPath());
        assertEquals("laptop", url.queryParameter("ten"));
        assertEquals(MA_LOAI.toString(), url.queryParameter("maLoai"));
        assertEquals(MA_PHONG.toString(), url.queryParameter("maPhong"));
        assertEquals("true", url.queryParameter("conHang"));
        assertEquals("2", url.queryParameter("trang"));
        assertEquals("20", url.queryParameter("kichThuoc"));
        assertNotNull(thuThap.ketQua);
    }

    @Test
    public void UISV02_TraCuuKhongBoLoc_KhongGuiThamSoLoc() throws Exception {
        mayChu.enqueue(new MockResponse().setBody(TRANG_RONG));
        ThuThap<TraCuuThietBiItemPage> thuThap = new ThuThap<>();

        kho.svTraCuuThietBi(null, null, null, null, 1, 20, thuThap);
        thuThap.cho();

        HttpUrl url = mayChu.takeRequest(5, TimeUnit.SECONDS).getRequestUrl();
        assertNull(url.queryParameter("ten"));
        assertNull(url.queryParameter("maLoai"));
        assertNull(url.queryParameter("maPhong"));
        assertNull(url.queryParameter("conHang"));
        assertEquals("1", url.queryParameter("trang"));
    }

    @Test
    public void UISV02_NguonLoai_DungDuongDanTrangVaGiaiMa() throws Exception {
        mayChu.enqueue(
                new MockResponse()
                        .setBody(
                                "{\"tongSo\":1,\"trang\":1,\"kichThuoc\":100,\"danhSach\":"
                                        + "[{\"maLoai\":\"11111111-1111-1111-1111-111111111111\","
                                        + "\"tenLoai\":\"Laptop\"}]}"));
        ThuThap<LoaiThietBiTomTatPage> thuThap = new ThuThap<>();

        kho.svDsLoaiThietBi(1, 100, thuThap);
        thuThap.cho();

        HttpUrl url = mayChu.takeRequest(5, TimeUnit.SECONDS).getRequestUrl();
        assertEquals("/api/v1/sv/loai-thiet-bi", url.encodedPath());
        assertEquals("1", url.queryParameter("trang"));
        assertEquals("100", url.queryParameter("kichThuoc"));
        assertEquals("Laptop", thuThap.ketQua.getDanhSach().get(0).getTenLoai());
    }

    @Test
    public void UISV02_NguonPhong_DungDuongDanTrangVaGiaiMa() throws Exception {
        mayChu.enqueue(
                new MockResponse()
                        .setBody(
                                "{\"tongSo\":1,\"trang\":1,\"kichThuoc\":100,\"danhSach\":"
                                        + "[{\"maPhong\":\"22222222-2222-2222-2222-222222222222\","
                                        + "\"tenPhong\":\"Phòng A1\",\"coSo\":\"Cơ sở 1\"}]}"));
        ThuThap<PhongThietBiPage> thuThap = new ThuThap<>();

        kho.svDsPhongThietBi(1, 100, thuThap);
        thuThap.cho();

        HttpUrl url = mayChu.takeRequest(5, TimeUnit.SECONDS).getRequestUrl();
        assertEquals("/api/v1/sv/phong-thiet-bi", url.encodedPath());
        assertEquals("Phòng A1", thuThap.ketQua.getDanhSach().get(0).getTenPhong());
        assertEquals("Cơ sở 1", thuThap.ketQua.getDanhSach().get(0).getCoSo());
    }

    @Test
    public void FA33_Loi500_ChuyenThanhApiErrorHttp() throws Exception {
        mayChu.enqueue(
                new MockResponse()
                        .setResponseCode(500)
                        .setHeader("Content-Type", "application/problem+json")
                        .setBody(Fixtures.doc("loi/500-loi-he-thong.json")));
        ThuThap<TraCuuThietBiItemPage> thuThap = new ThuThap<>();

        kho.svTraCuuThietBi(null, null, null, null, 1, 20, thuThap);
        thuThap.cho();

        assertNotNull(thuThap.loi);
        assertEquals(ApiError.Loai.HTTP, thuThap.loi.getLoai());
        assertEquals(Integer.valueOf(500), thuThap.loi.getStatus());
    }

    /** Bộ thu kết quả bất đồng bộ: Retrofit trên JVM gọi lại ở luồng của OkHttp nên cần chờ. */
    private static final class ThuThap<T> implements ApiCallback<T> {
        private final CountDownLatch xong = new CountDownLatch(1);
        volatile T ketQua;
        volatile ApiError loi;

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
