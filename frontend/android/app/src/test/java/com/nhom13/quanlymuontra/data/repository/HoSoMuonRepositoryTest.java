package com.nhom13.quanlymuontra.data.repository;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.HoTroSinhVienApi;
import com.nhom13.quanlymuontra.HoTroXacThuc;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import java.io.IOException;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import okhttp3.OkHttpClient;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import okhttp3.mockwebserver.RecordedRequest;
import org.junit.Test;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/**
 * Kiểm thử {@link HoSoMuonRepository} (UF-SV-08, FA 5.3 quy tắc 3): kết quả, chuẩn hóa lỗi, không đệm, và
 * đường dẫn thật qua Retrofit với MockWebServer. Chưa gọi máy chủ backend thật.
 */
public class HoSoMuonRepositoryTest {

    /** Gom kết quả của một lần gọi; hoàn tất ngay khi dùng {@link FakeCall}, chờ khi dùng mạng giả. */
    private static final class KetQuaCho implements ApiCallback<SinhVienTheInfo> {
        private final CountDownLatch xong = new CountDownLatch(1);
        volatile SinhVienTheInfo hoSo;
        volatile ApiError loi;

        @Override
        public void onSuccess(SinhVienTheInfo ketQua) {
            hoSo = ketQua;
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

    private static HoSoMuonRepository khoVoi(FakeCall<SinhVienTheInfo>... cacLan) {
        return new HoSoMuonRepository(HoTroSinhVienApi.taoApiSvToi(cacLan));
    }

    @Test
    public void UFSV08_ThanhCong_TraVeHoSoDungGiaTri() throws Exception {
        FakeCall<SinhVienTheInfo> goi =
                FakeCall.traVe(HoTroSinhVienApi.hoSoThanhCong("thanh-cong/sv-toi-bi-chan.json"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svToi(ketQua);
        ketQua.cho();

        assertNull(ketQua.loi);
        assertEquals("23520001", ketQua.hoSo.getMaSV());
        assertEquals("TAM_KHOA", ketQua.hoSo.getTrangThaiQuyenMuon());
    }

    @Test
    public void UFSV08_Loi401_GiuStatusVaMaLoi() throws Exception {
        FakeCall<SinhVienTheInfo> goi =
                FakeCall.traVe(
                        HoTroXacThuc.<SinhVienTheInfo>phanHoiLoi(
                                401, "loi/401-chua-xac-thuc.json"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svToi(ketQua);
        ketQua.cho();

        assertNull(ketQua.hoSo);
        assertEquals(ApiError.Loai.HTTP, ketQua.loi.getLoai());
        assertEquals(Integer.valueOf(401), ketQua.loi.getStatus());
        assertEquals("CHUA_XAC_THUC", ketQua.loi.getMaLoi());
    }

    @Test
    public void UFSV08_Loi403_GiuMaLoiKhongDuQuyen() throws Exception {
        FakeCall<SinhVienTheInfo> goi =
                FakeCall.traVe(
                        HoTroXacThuc.<SinhVienTheInfo>phanHoiLoi(
                                403, "loi/403-khong-du-quyen.json"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svToi(ketQua);
        ketQua.cho();

        assertEquals(Integer.valueOf(403), ketQua.loi.getStatus());
        assertEquals("KHONG_DU_QUYEN", ketQua.loi.getMaLoi());
    }

    @Test
    public void FA33_Loi500_LaLoiHeThongKemTraceId() throws Exception {
        FakeCall<SinhVienTheInfo> goi =
                FakeCall.traVe(
                        HoTroXacThuc.<SinhVienTheInfo>phanHoiLoi(
                                500, "loi/500-loi-he-thong.json"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svToi(ketQua);
        ketQua.cho();

        assertEquals(Integer.valueOf(500), ketQua.loi.getStatus());
        assertNotNull(ketQua.loi.getTraceId());
    }

    @Test
    public void FA33_MatKetNoi_LaLoiMang() throws Exception {
        FakeCall<SinhVienTheInfo> goi = FakeCall.nem(new IOException("mất kết nối"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svToi(ketQua);
        ketQua.cho();

        assertEquals(ApiError.Loai.MANG, ketQua.loi.getLoai());
    }

    @Test
    public void FA33_Phan200KhongCoThan_LaLoiHeThongChung() throws Exception {
        FakeCall<SinhVienTheInfo> goi = FakeCall.traVe(Response.success((SinhVienTheInfo) null));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svToi(ketQua);
        ketQua.cho();

        assertEquals(ApiError.Loai.HTTP, ketQua.loi.getLoai());
        assertNull(ketQua.loi.getMaLoi());
    }

    @Test
    public void FA53_GoiHaiLan_PhatHaiYeuCauKhongDem() throws Exception {
        FakeCall<SinhVienTheInfo> goi =
                FakeCall.traVe(HoTroSinhVienApi.hoSoThanhCong("thanh-cong/sv-toi-binh-thuong.json"));
        HoSoMuonRepository kho = khoVoi(goi, goi);

        kho.svToi(new KetQuaCho());
        kho.svToi(new KetQuaCho());

        assertEquals(2, goi.soLanEnqueue());
    }

    @Test
    public void FA32_GoiThat_GetDungDuongDanApiV1SvToi() throws Exception {
        try (MockWebServer maySu = new MockWebServer()) {
            maySu.enqueue(
                    new MockResponse()
                            .setHeader("Content-Type", "application/json")
                            .setBody(Fixtures.doc("thanh-cong/sv-toi-binh-thuong.json")));
            maySu.start();
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
            HoSoMuonRepository kho = new HoSoMuonRepository(retrofit.create(SinhVienApi.class));
            KetQuaCho ketQua = new KetQuaCho();

            kho.svToi(ketQua);
            ketQua.cho();
            RecordedRequest yeuCau = maySu.takeRequest(10, TimeUnit.SECONDS);

            assertNotNull(yeuCau);
            assertEquals("GET", yeuCau.getMethod());
            assertEquals("/api/v1/sv/toi", yeuCau.getPath());
            assertEquals("23520002", ketQua.hoSo.getMaSV());
        }
    }
}
