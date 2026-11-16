package com.nhom13.quanlymuontra.data.api;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertSame;
import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.HoTroLiveData;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.session.FakeSharedPreferences;
import com.nhom13.quanlymuontra.data.session.FakeTokenCipher;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import com.nhom13.quanlymuontra.data.session.TokenStore;
import com.nhom13.quanlymuontra.data.session.TrangThaiPhien;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicInteger;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import okhttp3.mockwebserver.Dispatcher;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import okhttp3.mockwebserver.RecordedRequest;
import okhttp3.mockwebserver.SocketPolicy;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;

/**
 * Kiểm thử {@link TokenAuthenticator} bằng {@code MockWebServer} (FA 5.10, FC 9.3). Máy chủ giả
 * chấp nhận duy nhất {@code Bearer access-moi}; mọi token khác nhận 401.
 */
public class TokenAuthenticatorTest {

    private static final String TOKEN_CU = "access-cu";
    private static final String REFRESH_CU = "refresh-cu";
    private static final String TOKEN_MOI = "access-moi";
    private static final String REFRESH_MOI = "refresh-moi";
    private static final String DUONG_DAN_YEU_CAU = "/api/v1/sv/toi";
    private static final String MA_TRUY_VET =
            "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00";
    // Đủ để các yêu cầu cùng 401 kịp xếp hàng chờ lần làm mới đang chạy.
    private static final long TRE_LAM_MOI_MS = 300;
    private static final long THOI_GIAN_CHO_LAM_MOI_MS = 5_000;

    private FakeSharedPreferences prefs;
    private FakeTokenCipher cipher;
    private SessionManager phien;
    private MayChuGia may;
    private MockWebServer maychu;

    @Before
    public void chuanBi() throws IOException {
        HoTroLiveData.batDau();
        prefs = new FakeSharedPreferences();
        cipher = new FakeTokenCipher();
        phien = new SessionManager(new TokenStore(prefs, cipher));
        phien.save(new TokenPair(TOKEN_CU, REFRESH_CU));
        may = new MayChuGia();
        maychu = new MockWebServer();
        maychu.setDispatcher(may);
        maychu.start();
    }

    @After
    public void don() throws IOException {
        maychu.shutdown();
        HoTroLiveData.ketThuc();
    }

    // ---- Làm mới thành công ----

    @Test
    public void FA54_BaYeuCauCung401_ChiMotLanLamMoiVaMoiYeuCauGoiLaiDungMotLan()
            throws Exception {
        may.phanHoiLamMoi = lamMoiThanhCong();
        OkHttpClient client = taoClient(THOI_GIAN_CHO_LAM_MOI_MS);
        int soYeuCau = 3;

        List<Future<Integer>> ketQua = chayDongThoi(client, soYeuCau);

        for (Future<Integer> mot : ketQua) {
            assertEquals(Integer.valueOf(200), mot.get(30, TimeUnit.SECONDS));
        }
        assertEquals(1, may.soLanLamMoi.get());
        assertEquals(soYeuCau, may.soYeuCauTokenMoi.get());
        assertTrue(may.soYeuCauTokenCu.get() >= 1);
        assertTrue(may.soYeuCauTong.get() <= 2 * soYeuCau);
    }

    @Test
    public void FA35_LamMoiThanhCong_LuuCapTokenMoiTruocKhiGoiLaiVaGoiLaiDungMotLan()
            throws Exception {
        may.phanHoiLamMoi = lamMoiThanhCong();
        OkHttpClient client = taoClient(THOI_GIAN_CHO_LAM_MOI_MS);

        try (Response phanHoi = client.newCall(yeuCau()).execute()) {
            assertEquals(200, phanHoi.code());
        }

        assertTrue("Cặp token mới phải xuống kho lưu trước khi gọi lại", may.daLuuKhiGoiLai.get());
        assertEquals(1, may.soLanLamMoi.get());
        assertEquals(1, may.soYeuCauTokenCu.get());
        assertEquals(1, may.soYeuCauTokenMoi.get());
        TokenPair hienTai = phien.getTokenPair();
        assertNotNull(hienTai);
        assertEquals(TOKEN_MOI, hienTai.getAccessToken());
        assertEquals(REFRESH_MOI, hienTai.getRefreshToken());
        assertEquals(TrangThaiPhien.CO_PHIEN, phien.getTrangThaiPhien().getValue());
    }

    @Test
    public void FA35_LamMoiGuiRefreshTokenCuTrongThanVaKhongKemAuthorization() throws Exception {
        may.phanHoiLamMoi = lamMoiThanhCong();
        OkHttpClient client = taoClient(THOI_GIAN_CHO_LAM_MOI_MS);

        client.newCall(yeuCau()).execute().close();

        assertEquals("POST", may.phuongThucLamMoi);
        assertTrue(may.thanLamMoi.contains("\"refreshToken\":\"" + REFRESH_CU + "\""));
        assertNull(may.tieuDeXacThucLamMoi);
    }

    @Test
    public void FA54_TokenDaDuocLamMoiThanhCong_GoiLaiVoiTokenHienTaiKhongLamMoiLai()
            throws Exception {
        // Giả lập một yêu cầu khác đã làm mới xong trong lúc yêu cầu này đang chờ phản hồi 401.
        may.truocKhiTraVe401 = () -> phien.save(new TokenPair(TOKEN_MOI, REFRESH_MOI));
        OkHttpClient client = taoClient(THOI_GIAN_CHO_LAM_MOI_MS);

        try (Response phanHoi = client.newCall(yeuCau()).execute()) {
            assertEquals(200, phanHoi.code());
        }

        assertEquals(0, may.soLanLamMoi.get());
        assertEquals(1, may.soYeuCauTokenCu.get());
        assertEquals(1, may.soYeuCauTokenMoi.get());
    }

    @Test
    public void FA54_YeuCauDaGoiLaiVanNhan401_KhongLamMoiLaiVaTraVePhanHoi401() throws Exception {
        may.phanHoiLamMoi = lamMoiThanhCong();
        may.luonTraVe401 = true;
        OkHttpClient client = taoClient(THOI_GIAN_CHO_LAM_MOI_MS);

        try (Response phanHoi = client.newCall(yeuCau()).execute()) {
            assertEquals(401, phanHoi.code());
        }

        assertEquals(1, may.soLanLamMoi.get());
        assertEquals(2, may.soYeuCauTong.get());
    }

    // ---- Không kích hoạt làm mới ----

    @Test
    public void FA54_KhongCoPhien_YeuCau401TraVeNguyenKhongLamMoi() throws Exception {
        phien.clear();
        OkHttpClient client = taoClient(THOI_GIAN_CHO_LAM_MOI_MS);

        try (Response phanHoi = client.newCall(yeuCau()).execute()) {
            assertEquals(401, phanHoi.code());
        }

        assertEquals(0, may.soLanLamMoi.get());
        assertEquals(TrangThaiPhien.KHONG_CO_PHIEN, phien.getTrangThaiPhien().getValue());
    }

    @Test
    public void FA35_DangNhapNhan401_KhongLamMoiVaGiuNguyenPhien() throws Exception {
        OkHttpClient client = taoClient(THOI_GIAN_CHO_LAM_MOI_MS);
        Request dangNhap =
                new Request.Builder()
                        .url(maychu.url(DuongDanApi.DANG_NHAP_SINH_VIEN))
                        .post(RequestBody.create("{}", MediaType.get("application/json")))
                        .build();

        try (Response phanHoi = client.newCall(dangNhap).execute()) {
            assertEquals(401, phanHoi.code());
        }

        assertEquals(0, may.soLanLamMoi.get());
        TokenPair hienTai = phien.getTokenPair();
        assertNotNull(hienTai);
        assertEquals(TOKEN_CU, hienTai.getAccessToken());
    }

    // ---- Làm mới thất bại: mọi nhánh dừng yêu cầu gốc, xóa phiên, không replay ----

    @Test
    public void DXFA23_LamMoiBi401_KetThucPhienGiuNguyen401() {
        ApiError loi =
                chayVaKiemTraKetThucPhien(
                        loiProblem(401, "loi/401-chua-xac-thuc.json"), THOI_GIAN_CHO_LAM_MOI_MS);

        assertEquals(ApiError.Loai.HTTP, loi.getLoai());
        assertEquals(Integer.valueOf(401), loi.getStatus());
        assertEquals("CHUA_XAC_THUC", loi.getMaLoi());
        assertEquals(MA_TRUY_VET, loi.getTraceId());
    }

    @Test
    public void DXFA23_LamMoiBi429_GiuNguyenStatusMaLoiTraceIdKhongDoiThanhLoiMang() {
        ApiError loi =
                chayVaKiemTraKetThucPhien(
                        loiProblem(429, "loi/429-qua-nhieu-yeu-cau.json"),
                        THOI_GIAN_CHO_LAM_MOI_MS);

        assertEquals(ApiError.Loai.HTTP, loi.getLoai());
        assertEquals(Integer.valueOf(429), loi.getStatus());
        assertEquals("QUA_NHIEU_YEU_CAU", loi.getMaLoi());
        assertEquals(MA_TRUY_VET, loi.getTraceId());
    }

    @Test
    public void DXFA23_LamMoiBi500_GiuTraceIdVaKhongCoMaLoi() {
        ApiError loi =
                chayVaKiemTraKetThucPhien(
                        loiProblem(500, "loi/500-loi-he-thong.json"), THOI_GIAN_CHO_LAM_MOI_MS);

        assertEquals(Integer.valueOf(500), loi.getStatus());
        assertNull(loi.getMaLoi());
        assertEquals(MA_TRUY_VET, loi.getTraceId());
    }

    @Test
    public void DXFA23_LamMoiMatKetNoi_KetThucPhienVoiLoiMang() {
        ApiError loi =
                chayVaKiemTraKetThucPhien(
                        new MockResponse().setSocketPolicy(SocketPolicy.DISCONNECT_AFTER_REQUEST),
                        THOI_GIAN_CHO_LAM_MOI_MS);

        assertEquals(ApiError.Loai.MANG, loi.getLoai());
    }

    @Test
    public void DXFA23_LamMoiQuaThoiGianCho_KetThucPhienVoiLoiQuaThoiGian() {
        ApiError loi =
                chayVaKiemTraKetThucPhien(
                        new MockResponse().setSocketPolicy(SocketPolicy.NO_RESPONSE), 300);

        assertEquals(ApiError.Loai.QUA_THOI_GIAN, loi.getLoai());
    }

    @Test
    public void DXFA23_LamMoi200NhungThanThieuToken_KetThucPhienVoiLoiHeThongChung() {
        MockResponse thanHong =
                new MockResponse()
                        .setResponseCode(200)
                        .setHeader("Content-Type", "application/json")
                        .setBody("{\"accessToken\":\"chi-co-access\"}");

        ApiError loi = chayVaKiemTraKetThucPhien(thanHong, THOI_GIAN_CHO_LAM_MOI_MS);

        assertEquals(ApiError.Loai.HTTP, loi.getLoai());
        assertNull(loi.getMaLoi());
        assertNull(loi.getTraceId());
    }

    @Test
    public void FA54_NguoiChoSauKhiLamMoiThatBai_NhanCungLoiKhongLamMoiLaiVaKhongGoiLai()
            throws Exception {
        may.phanHoiLamMoi =
                loiProblem(429, "loi/429-qua-nhieu-yeu-cau.json")
                        .setHeadersDelay(TRE_LAM_MOI_MS, TimeUnit.MILLISECONDS);
        OkHttpClient client = taoClient(THOI_GIAN_CHO_LAM_MOI_MS);
        int soYeuCau = 3;

        List<Future<Integer>> ketQua = chayDongThoi(client, soYeuCau);

        ApiError loiDauTien = null;
        for (Future<Integer> mot : ketQua) {
            ExecutionException loi =
                    assertThrows(ExecutionException.class, () -> mot.get(30, TimeUnit.SECONDS));
            assertTrue(loi.getCause() instanceof LamMoiPhienThatBaiException);
            ApiError apiError = ((LamMoiPhienThatBaiException) loi.getCause()).getApiError();
            if (loiDauTien == null) {
                loiDauTien = apiError;
            } else {
                assertSame(loiDauTien, apiError);
            }
        }
        assertNotNull(loiDauTien);
        assertEquals(Integer.valueOf(429), loiDauTien.getStatus());
        assertEquals("QUA_NHIEU_YEU_CAU", loiDauTien.getMaLoi());
        assertEquals(1, may.soLanLamMoi.get());
        assertEquals(soYeuCau, may.soYeuCauTong.get());
        assertNull(phien.getTokenPair());
    }

    // ---- Hỗ trợ ----

    private ApiError chayVaKiemTraKetThucPhien(MockResponse phanHoiLamMoi, long thoiGianChoMs) {
        may.phanHoiLamMoi = phanHoiLamMoi;
        OkHttpClient client = taoClient(thoiGianChoMs);

        LamMoiPhienThatBaiException loi =
                assertThrows(
                        LamMoiPhienThatBaiException.class,
                        () -> client.newCall(yeuCau()).execute().close());

        // Dừng yêu cầu gốc: một lần làm mới, một lần gửi yêu cầu, không gọi lại.
        assertEquals(1, may.soLanLamMoi.get());
        assertEquals(1, may.soYeuCauTong.get());
        // Xóa phiên khỏi bộ nhớ và kho lưu, rồi phát lý do đúng một lần.
        assertNull(phien.getTokenPair());
        assertEquals(TrangThaiPhien.KHONG_CO_PHIEN, phien.getTrangThaiPhien().getValue());
        assertTrue(prefs.getAll().isEmpty());
        Event<ApiError> suKien = phien.getLyDoKetThucPhien().getValue();
        assertNotNull(suKien);
        assertSame(loi.getApiError(), suKien.getContentIfNotHandled());
        return loi.getApiError();
    }

    private OkHttpClient taoClient(long thoiGianChoLamMoiMs) {
        OkHttpClient clientLamMoi =
                new OkHttpClient.Builder()
                        .readTimeout(thoiGianChoLamMoiMs, TimeUnit.MILLISECONDS)
                        .retryOnConnectionFailure(false)
                        .build();
        return new OkHttpClient.Builder()
                .addInterceptor(new AuthInterceptor(phien))
                .authenticator(new TokenAuthenticator(phien, clientLamMoi, maychu.url("/")))
                .retryOnConnectionFailure(false)
                .build();
    }

    private Request yeuCau() {
        return new Request.Builder().url(maychu.url(DUONG_DAN_YEU_CAU)).build();
    }

    private List<Future<Integer>> chayDongThoi(OkHttpClient client, int soYeuCau) {
        ExecutorService luong = Executors.newFixedThreadPool(soYeuCau);
        CountDownLatch batDau = new CountDownLatch(1);
        List<Future<Integer>> ketQua = new ArrayList<>();
        for (int i = 0; i < soYeuCau; i++) {
            ketQua.add(
                    luong.submit(
                            () -> {
                                batDau.await();
                                try (Response phanHoi = client.newCall(yeuCau()).execute()) {
                                    return phanHoi.code();
                                }
                            }));
        }
        batDau.countDown();
        luong.shutdown();
        return ketQua;
    }

    private static MockResponse lamMoiThanhCong() {
        return new MockResponse()
                .setResponseCode(200)
                .setHeader("Content-Type", "application/json")
                .setBody(Fixtures.doc("thanh-cong/auth-lam-moi.json"))
                .setHeadersDelay(TRE_LAM_MOI_MS, TimeUnit.MILLISECONDS);
    }

    private static MockResponse loiProblem(int status, String tepFixture) {
        return new MockResponse()
                .setResponseCode(status)
                .setHeader("Content-Type", "application/problem+json")
                .setBody(Fixtures.doc(tepFixture));
    }

    /** Máy chủ giả: đếm yêu cầu theo token và ghi lại yêu cầu làm mới gần nhất. */
    private final class MayChuGia extends Dispatcher {

        final AtomicInteger soLanLamMoi = new AtomicInteger();
        final AtomicInteger soYeuCauTong = new AtomicInteger();
        final AtomicInteger soYeuCauTokenCu = new AtomicInteger();
        final AtomicInteger soYeuCauTokenMoi = new AtomicInteger();
        final AtomicBoolean daLuuKhiGoiLai = new AtomicBoolean(false);
        volatile MockResponse phanHoiLamMoi;
        volatile boolean luonTraVe401 = false;
        volatile Runnable truocKhiTraVe401;
        volatile String thanLamMoi;
        volatile String phuongThucLamMoi;
        volatile String tieuDeXacThucLamMoi;

        @Override
        public MockResponse dispatch(RecordedRequest yeuCau) {
            if (DuongDanApi.LAM_MOI.equals(yeuCau.getPath())) {
                thanLamMoi = yeuCau.getBody().readUtf8();
                phuongThucLamMoi = yeuCau.getMethod();
                tieuDeXacThucLamMoi = yeuCau.getHeader("Authorization");
                soLanLamMoi.incrementAndGet();
                return phanHoiLamMoi;
            }
            soYeuCauTong.incrementAndGet();
            String tieuDe = yeuCau.getHeader("Authorization");
            boolean mangTokenMoi = ("Bearer " + TOKEN_MOI).equals(tieuDe);
            if (mangTokenMoi) {
                soYeuCauTokenMoi.incrementAndGet();
            } else {
                soYeuCauTokenCu.incrementAndGet();
            }
            if (mangTokenMoi && !luonTraVe401) {
                daLuuKhiGoiLai.set(daLuuCapTokenMoi());
                return new MockResponse()
                        .setResponseCode(200)
                        .setHeader("Content-Type", "application/json")
                        .setBody("{}");
            }
            Runnable viec = truocKhiTraVe401;
            if (viec != null) {
                viec.run();
            }
            return loiProblem(401, "loi/401-chua-xac-thuc.json");
        }

        private boolean daLuuCapTokenMoi() {
            TokenPair daLuu = new TokenStore(prefs, cipher).read();
            return daLuu != null
                    && TOKEN_MOI.equals(daLuu.getAccessToken())
                    && REFRESH_MOI.equals(daLuu.getRefreshToken());
        }
    }
}
