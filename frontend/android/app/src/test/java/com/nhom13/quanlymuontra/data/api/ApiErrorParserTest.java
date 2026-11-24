package com.nhom13.quanlymuontra.data.api;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertSame;
import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.data.model.ApiError;
import java.io.IOException;
import java.io.InterruptedIOException;
import java.net.SocketTimeoutException;
import java.util.concurrent.TimeUnit;
import okhttp3.Call;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.ResponseBody;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import org.junit.Test;
import retrofit2.Response;

public class ApiErrorParserTest {

    private static final MediaType PROBLEM_JSON = MediaType.get("application/problem+json");
    private static final String MA_TRUY_VET =
            "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00";

    private static ApiError tuFixture(int status, String tep) {
        Response<Object> phanHoi =
                Response.error(status, ResponseBody.create(Fixtures.doc(tep), PROBLEM_JSON));
        return ApiErrorParser.tuPhanHoiLoi(phanHoi);
    }

    @Test
    public void FA33_ProblemJsonCoMaLoi_GiuNguyenStatusMaLoiVaTraceId() {
        ApiError loi = tuFixture(409, "loi/409-so-luong-kha-dung-thay-doi.json");

        assertEquals(ApiError.Loai.HTTP, loi.getLoai());
        assertEquals(Integer.valueOf(409), loi.getStatus());
        assertEquals("SO_LUONG_KHA_DUNG_THAY_DOI", loi.getMaLoi());
        assertEquals(MA_TRUY_VET, loi.getTraceId());
        assertTrue(loi.getErrors().isEmpty());
    }

    @Test
    public void FA33_ValidationProblemDetails_CoErrorsTheoKhoaTruong() {
        ApiError loi = tuFixture(400, "loi/400-validation.json");

        assertEquals("DU_LIEU_KHONG_HOP_LE", loi.getMaLoi());
        assertEquals(1, loi.getErrors().get("dsLoaiVaSoLuong[0].soLuong").size());
    }

    @Test
    public void FA33_Loi500ChiCoTraceId_MaLoiLaNull() {
        ApiError loi = tuFixture(500, "loi/500-loi-he-thong.json");

        assertEquals(Integer.valueOf(500), loi.getStatus());
        assertNull(loi.getMaLoi());
        assertEquals(MA_TRUY_VET, loi.getTraceId());
    }

    @Test
    public void FA33_ThanKhongPhaiJson_HttpKhongCoMaLoi() {
        Response<Object> phanHoi =
                Response.error(
                        502,
                        ResponseBody.create(
                                Fixtures.doc("loi/khong-phai-json.html"),
                                MediaType.get("text/html")));

        ApiError loi = ApiErrorParser.tuPhanHoiLoi(phanHoi);

        assertEquals(ApiError.Loai.HTTP, loi.getLoai());
        assertEquals(Integer.valueOf(502), loi.getStatus());
        assertNull(loi.getMaLoi());
        assertNull(loi.getTraceId());
    }

    @Test
    public void FA33_ThanJsonNhungKhongPhaiProblemJson_KhongDocMaLoi() {
        ApiError loi =
                ApiErrorParser.tuPhanHoiLoi(
                        401,
                        MediaType.get("application/json"),
                        Fixtures.doc("loi/401-chua-xac-thuc.json"));

        assertEquals(Integer.valueOf(401), loi.getStatus());
        assertNull(loi.getMaLoi());
    }

    @Test
    public void FA33_ProblemJsonHongCuPhap_HttpKhongCoMaLoi() {
        ApiError loi = ApiErrorParser.tuPhanHoiLoi(500, PROBLEM_JSON, "{\"status\":");

        assertEquals(ApiError.Loai.HTTP, loi.getLoai());
        assertEquals(Integer.valueOf(500), loi.getStatus());
        assertNull(loi.getMaLoi());
    }

    @Test
    public void FA33_ThanRongHoacKhongCoThan_HttpKhongCoMaLoi() {
        ApiError rong = ApiErrorParser.tuPhanHoiLoi(409, PROBLEM_JSON, "");
        ApiError khongThan = ApiErrorParser.tuPhanHoiLoi(409, null, null);

        assertNull(rong.getMaLoi());
        assertNull(khongThan.getMaLoi());
        assertEquals(Integer.valueOf(409), khongThan.getStatus());
    }

    @Test
    public void DXFC21_MaLoiChuaTungThay_GiuNguyenMaVaBoQuaTruongLa() {
        String thanLoi =
                "{\"status\":422,\"maLoi\":\"MA_LOI_CHUA_TUNG_THAY\",\"truongLa\":{\"a\":1},"
                        + "\"traceId\":\"trace-1\"}";

        ApiError loi = ApiErrorParser.tuPhanHoiLoi(422, PROBLEM_JSON, thanLoi);

        assertEquals("MA_LOI_CHUA_TUNG_THAY", loi.getMaLoi());
        assertEquals("trace-1", loi.getTraceId());
    }

    @Test
    public void FA55_SocketTimeoutVaInterruptedIO_LaQuaThoiGian() {
        ApiError tuSocket = ApiErrorParser.tuNgoaiLe(false, new SocketTimeoutException("doc"));
        ApiError tuInterrupted = ApiErrorParser.tuNgoaiLe(false, new InterruptedIOException("x"));

        assertEquals(ApiError.Loai.QUA_THOI_GIAN, tuSocket.getLoai());
        assertEquals(ApiError.Loai.QUA_THOI_GIAN, tuInterrupted.getLoai());
    }

    @Test
    public void FA55_IOExceptionKhac_LaMang() {
        ApiError loi = ApiErrorParser.tuNgoaiLe(false, new IOException("mat ket noi"));

        assertEquals(ApiError.Loai.MANG, loi.getLoai());
    }

    @Test
    public void FA55_DaHuy_LaHuy() {
        ApiError loi = ApiErrorParser.tuNgoaiLe(true, new IOException("Canceled"));

        assertEquals(ApiError.Loai.HUY, loi.getLoai());
    }

    @Test
    public void FA55_HetThoiGianKemDaHuyNgam_VanLaQuaThoiGianKhongPhaiHuy() {
        ApiError loi = ApiErrorParser.tuNgoaiLe(true, new InterruptedIOException("timeout"));

        assertEquals(ApiError.Loai.QUA_THOI_GIAN, loi.getLoai());
    }

    @Test
    public void FA54_LoiLamMoiPhien_GiuNguyenApiErrorKhongDoiThanhMangHayHuy() {
        ApiError loi429 = ApiError.http(429, "QUA_NHIEU_YEU_CAU", null, "trace-429", null);
        LamMoiPhienThatBaiException ngoaiLe = new LamMoiPhienThatBaiException(loi429);

        assertSame(loi429, ApiErrorParser.tuNgoaiLe(false, ngoaiLe));
        assertSame(loi429, ApiErrorParser.tuNgoaiLe(true, ngoaiLe));
    }

    @Test
    public void FA33_NgoaiLeKhongPhaiIO_LaLoiHeThongChung() {
        ApiError loi = ApiErrorParser.tuNgoaiLe(false, new IllegalStateException("x"));

        assertEquals(ApiError.Loai.HTTP, loi.getLoai());
        assertNull(loi.getMaLoi());
        assertNull(loi.getTraceId());
    }

    // Kiểm tra bằng OkHttp thật: hết callTimeout phải ra InterruptedIOException dù isCanceled() là
    // true hay false, nên ApiErrorParser phải xếp QUA_THOI_GIAN trước HUY (FA 5.5, ĐX-FA-04).
    @Test
    public void DXFA04_HetCallTimeoutThat_LaQuaThoiGian() throws Exception {
        MockWebServer maychu = new MockWebServer();
        try {
            maychu.enqueue(new MockResponse().setBody("{}").setHeadersDelay(1, TimeUnit.SECONDS));
            maychu.start();
            OkHttpClient client =
                    new OkHttpClient.Builder()
                            .callTimeout(200, TimeUnit.MILLISECONDS)
                            .retryOnConnectionFailure(false)
                            .build();
            Call call = client.newCall(new Request.Builder().url(maychu.url("/")).build());

            IOException loi = assertThrows(IOException.class, () -> call.execute().close());

            assertEquals(
                    ApiError.Loai.QUA_THOI_GIAN,
                    ApiErrorParser.tuNgoaiLe(call.isCanceled(), loi).getLoai());
        } finally {
            maychu.shutdown();
        }
    }

    @Test
    public void FA55_HuyChuDongTruocKhiGoi_LaHuy() throws Exception {
        MockWebServer maychu = new MockWebServer();
        try {
            maychu.start();
            Call call =
                    new OkHttpClient.Builder()
                            .build()
                            .newCall(new Request.Builder().url(maychu.url("/")).build());
            call.cancel();

            IOException loi = assertThrows(IOException.class, () -> call.execute().close());

            assertEquals(
                    ApiError.Loai.HUY,
                    ApiErrorParser.tuNgoaiLe(call.isCanceled(), loi).getLoai());
        } finally {
            maychu.shutdown();
        }
    }
}
