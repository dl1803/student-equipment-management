package com.nhom13.quanlymuontra.data.repository;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.HoTroXacThuc;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import com.nhom13.quanlymuontra.data.session.TrangThaiPhien;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.api.XacThucApi;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import okhttp3.OkHttpClient;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import okhttp3.mockwebserver.RecordedRequest;
import okhttp3.mockwebserver.SocketPolicy;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/**
 * Kiểm thử {@link XacThucRepository} với Retrofit thật và MockWebServer: đường dẫn, thân yêu cầu, ghi
 * token, và giữ nguyên {@link ApiError} khi kết thúc phiên. Chưa gọi máy chủ backend thật (D05-K).
 */
public class XacThucRepositoryTest {

    private static final String DUONG_DAN_DANG_NHAP = "/api/v1/auth/sinh-vien/dang-nhap";
    private static final String DUONG_DAN_LAM_MOI = "/api/v1/auth/lam-moi";
    private static final String DUONG_DAN_DANG_XUAT = "/api/v1/auth/dang-xuat";

    private MockWebServer maySu;
    private SessionManager phien;
    private XacThucRepository kho;

    /** Gom kết quả của một lần gọi bất đồng bộ để test chờ được trên luồng chính của test. */
    private static final class KetQuaCho implements ApiCallback<Void> {
        private final CountDownLatch xong = new CountDownLatch(1);
        volatile boolean thanhCong;
        volatile ApiError loi;

        @Override
        public void onSuccess(Void ketQua) {
            thanhCong = true;
            xong.countDown();
        }

        @Override
        public void onError(ApiError loi) {
            this.loi = loi;
            xong.countDown();
        }

        void cho() throws InterruptedException {
            assertTrue("Hết thời gian chờ kết quả", xong.await(10, TimeUnit.SECONDS));
        }
    }

    @Before
    public void chuanBi() throws Exception {
        HoTroXacThuc.batDauLiveData();
        maySu = new MockWebServer();
        maySu.start();
        phien = HoTroXacThuc.taoSessionManager();
        // Không thử lại khi mất kết nối: MockWebServer chỉ có đúng số phản hồi đã xếp hàng.
        OkHttpClient client =
                new OkHttpClient.Builder()
                        .retryOnConnectionFailure(false)
                        .callTimeout(10, TimeUnit.SECONDS)
                        .build();
        Retrofit retrofit =
                new Retrofit.Builder()
                        .baseUrl(maySu.url("/"))
                        .client(client)
                        .addConverterFactory(GsonConverterFactory.create(new JSON().getGson()))
                        .build();
        kho = new XacThucRepository(retrofit.create(XacThucApi.class), phien);
    }

    @After
    public void don() throws Exception {
        maySu.shutdown();
        HoTroXacThuc.ketThucLiveData();
    }

    private static MockResponse json(int status, String tepFixture, String loaiNoiDung) {
        return new MockResponse()
                .setResponseCode(status)
                .setHeader("Content-Type", loaiNoiDung)
                .setBody(Fixtures.doc(tepFixture));
    }

    // ---------- Đăng nhập ----------

    @Test
    public void UFCM01_DangNhapThanhCong_GhiTokenVaGuiDungDuongDanVaThan() throws Exception {
        maySu.enqueue(json(200, "thanh-cong/auth-dang-nhap-sinh-vien.json", "application/json"));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authDangNhapSinhVien("20201234", "mat-khau-1", ketQua);
        ketQua.cho();

        assertTrue(ketQua.thanhCong);
        TokenPair cap = phien.getTokenPair();
        assertNotNull(cap);
        assertEquals("access-dang-nhap", cap.getAccessToken());
        assertEquals("refresh-dang-nhap", cap.getRefreshToken());
        assertEquals(TrangThaiPhien.CO_PHIEN, phien.getTrangThaiPhien().getValue());
        RecordedRequest yeuCau = maySu.takeRequest(5, TimeUnit.SECONDS);
        assertEquals("POST", yeuCau.getMethod());
        assertEquals(DUONG_DAN_DANG_NHAP, yeuCau.getPath());
        String than = yeuCau.getBody().readUtf8();
        assertTrue(than, than.contains("\"mssv\":\"20201234\""));
        assertTrue(than, than.contains("\"matKhau\":\"mat-khau-1\""));
    }

    @Test
    public void UFCM01_SaiThongTin_GiuMaLoiVaKhongGhiToken() throws Exception {
        maySu.enqueue(json(401, "loi/401-dang-nhap-sai.json", "application/problem+json"));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authDangNhapSinhVien("20201234", "sai", ketQua);
        ketQua.cho();

        assertFalse(ketQua.thanhCong);
        assertEquals(ApiError.Loai.HTTP, ketQua.loi.getLoai());
        assertEquals(Integer.valueOf(401), ketQua.loi.getStatus());
        assertEquals("DANG_NHAP_SAI", ketQua.loi.getMaLoi());
        assertNull(phien.getTokenPair());
    }

    @Test
    public void UFCM01_QuaNhieuYeuCau_GiuNguyen429() throws Exception {
        maySu.enqueue(json(429, "loi/429-qua-nhieu-yeu-cau.json", "application/problem+json"));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authDangNhapSinhVien("20201234", "x", ketQua);
        ketQua.cho();

        assertEquals(Integer.valueOf(429), ketQua.loi.getStatus());
        assertEquals("QUA_NHIEU_YEU_CAU", ketQua.loi.getMaLoi());
        assertNull(phien.getTokenPair());
    }

    @Test
    public void UFCM01_MatKetNoi_BaoLoiMang() throws Exception {
        maySu.enqueue(new MockResponse().setSocketPolicy(SocketPolicy.DISCONNECT_AT_START));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authDangNhapSinhVien("20201234", "x", ketQua);
        ketQua.cho();

        assertEquals(ApiError.Loai.MANG, ketQua.loi.getLoai());
        assertNull(phien.getTokenPair());
    }

    @Test
    public void UFCM01_Phan200ThieuToken_LaLoiHeThongChungVaKhongGhiToken() throws Exception {
        maySu.enqueue(
                new MockResponse()
                        .setResponseCode(200)
                        .setHeader("Content-Type", "application/json")
                        .setBody("{\"accessToken\":\"\",\"refreshToken\":\"\",\"vaiTro\":\"SINH_VIEN\"}"));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authDangNhapSinhVien("20201234", "x", ketQua);
        ketQua.cho();

        assertEquals(ApiError.Loai.HTTP, ketQua.loi.getLoai());
        assertNull(ketQua.loi.getMaLoi());
        assertNull(phien.getTokenPair());
    }

    // ---------- Làm mới khi khởi động ----------

    @Test
    public void UFCM03_LamMoiThanhCong_GhiCapTokenMoiVaGuiRefreshTokenCu() throws Exception {
        phien.save(new TokenPair("access-cu", "refresh-cu"));
        maySu.enqueue(json(200, "thanh-cong/auth-lam-moi.json", "application/json"));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authLamMoi(ketQua);
        ketQua.cho();

        assertTrue(ketQua.thanhCong);
        assertEquals("access-moi", phien.getTokenPair().getAccessToken());
        assertEquals("refresh-moi", phien.getTokenPair().getRefreshToken());
        RecordedRequest yeuCau = maySu.takeRequest(5, TimeUnit.SECONDS);
        assertEquals(DUONG_DAN_LAM_MOI, yeuCau.getPath());
        assertTrue(yeuCau.getBody().readUtf8().contains("\"refreshToken\":\"refresh-cu\""));
    }

    @Test
    public void UFCM03_LamMoi401_KetThucPhienGiuNguyenLyDo() throws Exception {
        phien.save(new TokenPair("access-cu", "refresh-cu"));
        maySu.enqueue(json(401, "loi/401-chua-xac-thuc.json", "application/problem+json"));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authLamMoi(ketQua);
        ketQua.cho();

        assertEquals("CHUA_XAC_THUC", ketQua.loi.getMaLoi());
        assertNull(phien.getTokenPair());
        assertEquals(TrangThaiPhien.KHONG_CO_PHIEN, phien.getTrangThaiPhien().getValue());
        Event<ApiError> lyDo = phien.getLyDoKetThucPhien().getValue();
        assertNotNull(lyDo);
        ApiError loiGiuLai = lyDo.getContentIfNotHandled();
        assertEquals(Integer.valueOf(401), loiGiuLai.getStatus());
        assertEquals("CHUA_XAC_THUC", loiGiuLai.getMaLoi());
    }

    @Test
    public void UFCM03_LamMoi429_KetThucPhienVaKhongTuLap() throws Exception {
        phien.save(new TokenPair("access-cu", "refresh-cu"));
        maySu.enqueue(json(429, "loi/429-qua-nhieu-yeu-cau.json", "application/problem+json"));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authLamMoi(ketQua);
        ketQua.cho();

        assertEquals("QUA_NHIEU_YEU_CAU", ketQua.loi.getMaLoi());
        assertEquals(
                "QUA_NHIEU_YEU_CAU",
                phien.getLyDoKetThucPhien().getValue().getContentIfNotHandled().getMaLoi());
        assertEquals("Không tự gửi lại khi bị 429", 1, maySu.getRequestCount());
    }

    @Test
    public void UFCM03_LamMoiMatKetNoi_KetThucPhienKemLyDoMang() throws Exception {
        phien.save(new TokenPair("access-cu", "refresh-cu"));
        maySu.enqueue(new MockResponse().setSocketPolicy(SocketPolicy.DISCONNECT_AT_START));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authLamMoi(ketQua);
        ketQua.cho();

        assertEquals(ApiError.Loai.MANG, ketQua.loi.getLoai());
        assertEquals(
                ApiError.Loai.MANG,
                phien.getLyDoKetThucPhien().getValue().getContentIfNotHandled().getLoai());
        assertNull(phien.getTokenPair());
    }

    @Test
    public void UFCM03_LamMoi500_KetThucPhienKemTraceId() throws Exception {
        phien.save(new TokenPair("access-cu", "refresh-cu"));
        maySu.enqueue(json(500, "loi/500-loi-he-thong.json", "application/problem+json"));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authLamMoi(ketQua);
        ketQua.cho();

        ApiError lyDo = phien.getLyDoKetThucPhien().getValue().getContentIfNotHandled();
        assertEquals(Integer.valueOf(500), lyDo.getStatus());
        assertNotNull(lyDo.getTraceId());
        assertNull(phien.getTokenPair());
    }

    @Test
    public void UFCM03_LamMoiKhiKhongCoPhien_LaLoiCuaBenGoi() {
        assertThrows(IllegalStateException.class, () -> kho.authLamMoi(new KetQuaCho()));
    }

    // ---------- Đăng xuất ----------

    @Test
    public void UFCM03_DangXuatThanhCong_XoaPhienVaGuiRefreshToken() throws Exception {
        phien.save(new TokenPair("access-1", "refresh-1"));
        maySu.enqueue(new MockResponse().setResponseCode(204));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authDangXuat(ketQua);
        ketQua.cho();

        assertTrue(ketQua.thanhCong);
        assertNull(phien.getTokenPair());
        assertEquals(TrangThaiPhien.KHONG_CO_PHIEN, phien.getTrangThaiPhien().getValue());
        RecordedRequest yeuCau = maySu.takeRequest(5, TimeUnit.SECONDS);
        assertEquals(DUONG_DAN_DANG_XUAT, yeuCau.getPath());
        assertTrue(yeuCau.getBody().readUtf8().contains("\"refreshToken\":\"refresh-1\""));
    }

    @Test
    public void UFCM03_DangXuatKhiMatMang_VanXoaPhienCucBoVaBaoChuaThuHoi() throws Exception {
        phien.save(new TokenPair("access-1", "refresh-1"));
        maySu.enqueue(new MockResponse().setSocketPolicy(SocketPolicy.DISCONNECT_AT_START));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authDangXuat(ketQua);

        // Phiên cục bộ đã xóa ngay khi gọi, trước mọi phản hồi của mạng.
        assertNull(phien.getTokenPair());
        assertEquals(TrangThaiPhien.KHONG_CO_PHIEN, phien.getTrangThaiPhien().getValue());
        ketQua.cho();
        assertFalse("Chưa biết máy chủ đã thu hồi hay chưa", ketQua.thanhCong);
        assertEquals(ApiError.Loai.MANG, ketQua.loi.getLoai());
    }

    @Test
    public void UFCM03_DangXuatKhiMayChu500_VanXoaPhienCucBo() throws Exception {
        phien.save(new TokenPair("access-1", "refresh-1"));
        maySu.enqueue(json(500, "loi/500-loi-he-thong.json", "application/problem+json"));
        KetQuaCho ketQua = new KetQuaCho();

        kho.authDangXuat(ketQua);
        ketQua.cho();

        assertNull(phien.getTokenPair());
        assertEquals(Integer.valueOf(500), ketQua.loi.getStatus());
    }

    @Test
    public void UFCM03_DangXuatKhiKhongCoPhien_KhongGuiYeuCauVaBaoThanhCong() {
        KetQuaCho ketQua = new KetQuaCho();

        assertNull(kho.authDangXuat(ketQua));

        assertTrue(ketQua.thanhCong);
        assertEquals(0, maySu.getRequestCount());
    }
}
