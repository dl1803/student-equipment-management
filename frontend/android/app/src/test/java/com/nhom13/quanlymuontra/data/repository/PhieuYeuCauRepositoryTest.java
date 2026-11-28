package com.nhom13.quanlymuontra.data.repository;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.HoTroTraCuuApi;
import com.nhom13.quanlymuontra.HoTroXacThuc;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.model.GuiYeuCauMuonRequest;
import com.nhom13.quanlymuontra.generated.model.LoaiVaSoLuongRequest;
import com.nhom13.quanlymuontra.generated.model.PhieuYeuCauMuon;
import java.io.IOException;
import java.net.SocketTimeoutException;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;
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
 * Kiểm thử {@link PhieuYeuCauRepository} (UF-SV-02, FA 3.3, FA 3.11): kết quả, chuẩn hóa lỗi, không lặp
 * yêu cầu ghi, và đường dẫn thật qua Retrofit với MockWebServer. Chưa gọi máy chủ backend thật.
 */
public class PhieuYeuCauRepositoryTest {

    private static final String THAO_TAC = "svGuiPhieuYeuCauMuon";
    private static final UUID MA_PHONG = UUID.fromString("00000000-0000-0000-0000-000000001500");
    private static final UUID MA_LOAI = UUID.fromString("00000000-0000-0000-0000-000000000001");

    private static final class KetQuaCho implements ApiCallback<PhieuYeuCauMuon> {
        private final CountDownLatch xong = new CountDownLatch(1);
        volatile boolean thanhCong;
        volatile PhieuYeuCauMuon phieu;
        volatile ApiError loi;

        @Override
        public void onSuccess(PhieuYeuCauMuon ketQua) {
            thanhCong = true;
            phieu = ketQua;
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

    private static GuiYeuCauMuonRequest yeuCauMau() {
        LoaiVaSoLuongRequest dong = new LoaiVaSoLuongRequest();
        dong.setMaLoai(MA_LOAI);
        dong.setSoLuong(2);
        List<LoaiVaSoLuongRequest> ds = new ArrayList<>();
        ds.add(dong);
        GuiYeuCauMuonRequest yeuCau = new GuiYeuCauMuonRequest();
        yeuCau.setMaPhong(MA_PHONG);
        yeuCau.setThoiGianMuonDuKienGio(2);
        yeuCau.setDsLoaiVaSoLuong(ds);
        return yeuCau;
    }

    private static PhieuYeuCauRepository khoVoi(FakeCall<PhieuYeuCauMuon> goi) {
        return new PhieuYeuCauRepository(new HoTroTraCuuApi().chuanBi(THAO_TAC, goi).api());
    }

    private static PhieuYeuCauMuon phieuMau() {
        return new JSON()
                .getGson()
                .fromJson(Fixtures.doc("thanh-cong/sv-gui-phieu-yeu-cau.json"), PhieuYeuCauMuon.class);
    }

    @Test
    public void UFSV02_ThanhCong_TraVePhieuVaChiPhatMotLan() throws Exception {
        FakeCall<PhieuYeuCauMuon> goi = FakeCall.traVe(Response.success(phieuMau()));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svGuiPhieuYeuCauMuon(yeuCauMau(), ketQua);
        ketQua.cho();

        assertNull(ketQua.loi);
        assertEquals("YC-2026-0003", ketQua.phieu.getMaYeuCau());
        assertEquals(1, goi.soLanEnqueue());
    }

    @Test
    public void UFSV02_PhanHoi2xxKhongCoThan_BaoThanhCongVoiNull() throws Exception {
        FakeCall<PhieuYeuCauMuon> goi = FakeCall.traVe(Response.<PhieuYeuCauMuon>success(null));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svGuiPhieuYeuCauMuon(yeuCauMau(), ketQua);
        ketQua.cho();

        assertTrue(ketQua.thanhCong);
        assertNull(ketQua.phieu);
        assertNull(ketQua.loi);
    }

    @Test
    public void UFSV02_Loi422_ChuanHoaVoiMaLoiVaTraceId() throws Exception {
        FakeCall<PhieuYeuCauMuon> goi =
                FakeCall.traVe(
                        HoTroXacThuc.<PhieuYeuCauMuon>phanHoiLoi(
                                422, "loi/422-vuot-gioi-han-thiet-bi.json"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svGuiPhieuYeuCauMuon(yeuCauMau(), ketQua);
        ketQua.cho();

        assertNotNull(ketQua.loi);
        assertEquals(ApiError.Loai.HTTP, ketQua.loi.getLoai());
        assertEquals(Integer.valueOf(422), ketQua.loi.getStatus());
        assertEquals("VUOT_GIOI_HAN_THIET_BI", ketQua.loi.getMaLoi());
        assertNotNull(ketQua.loi.getTraceId());
    }

    @Test
    public void UFSV02_Loi400_GiuDanhSachLoiTheoTruong() throws Exception {
        FakeCall<PhieuYeuCauMuon> goi =
                FakeCall.traVe(
                        HoTroXacThuc.<PhieuYeuCauMuon>phanHoiLoi(400, "loi/400-validation.json"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svGuiPhieuYeuCauMuon(yeuCauMau(), ketQua);
        ketQua.cho();

        assertEquals("DU_LIEU_KHONG_HOP_LE", ketQua.loi.getMaLoi());
        assertTrue(ketQua.loi.getErrors().containsKey("dsLoaiVaSoLuong[0].soLuong"));
    }

    @Test
    public void UFSV02_MatKetNoi_LaLoiMangKhongTuGuiLai() throws Exception {
        FakeCall<PhieuYeuCauMuon> goi = FakeCall.nem(new IOException("mat ket noi"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svGuiPhieuYeuCauMuon(yeuCauMau(), ketQua);
        ketQua.cho();

        assertEquals(ApiError.Loai.MANG, ketQua.loi.getLoai());
        assertEquals(1, goi.soLanEnqueue());
    }

    @Test
    public void UFSV02_QuaThoiGian_LaLoiQuaThoiGian() throws Exception {
        FakeCall<PhieuYeuCauMuon> goi = FakeCall.nem(new SocketTimeoutException("timeout"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svGuiPhieuYeuCauMuon(yeuCauMau(), ketQua);
        ketQua.cho();

        assertEquals(ApiError.Loai.QUA_THOI_GIAN, ketQua.loi.getLoai());
    }

    @Test
    public void UFSV02_HuySauKhiPhat_LaLoiHuyKhongBiNuot() throws Exception {
        FakeCall<PhieuYeuCauMuon> goi = FakeCall.cho();
        KetQuaCho ketQua = new KetQuaCho();
        khoVoi(goi).svGuiPhieuYeuCauMuon(yeuCauMau(), ketQua);

        goi.cancel();
        goi.that(new IOException("Canceled"));
        ketQua.cho();

        assertEquals(ApiError.Loai.HUY, ketQua.loi.getLoai());
    }

    @Test
    public void UFSV02_NgoaiLeGiaiMaSauKhiPhat_CoiNhuMatPhanHoi() throws Exception {
        FakeCall<PhieuYeuCauMuon> goi = FakeCall.nem(new IllegalStateException("giai ma loi"));
        KetQuaCho ketQua = new KetQuaCho();

        khoVoi(goi).svGuiPhieuYeuCauMuon(yeuCauMau(), ketQua);
        ketQua.cho();

        assertEquals(ApiError.Loai.MANG, ketQua.loi.getLoai());
    }

    @Test
    public void UFSV02_DuongDanThat_PostDungDuongDanVaThanYeuCau() throws Exception {
        try (MockWebServer maySu = new MockWebServer()) {
            maySu.enqueue(
                    new MockResponse()
                            .setResponseCode(201)
                            .setHeader("Content-Type", "application/json")
                            .setBody(Fixtures.doc("thanh-cong/sv-gui-phieu-yeu-cau.json")));
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
            PhieuYeuCauRepository kho = new PhieuYeuCauRepository(retrofit.create(SinhVienApi.class));
            KetQuaCho ketQua = new KetQuaCho();

            kho.svGuiPhieuYeuCauMuon(yeuCauMau(), ketQua);
            ketQua.cho();
            RecordedRequest yeuCau = maySu.takeRequest(10, TimeUnit.SECONDS);

            assertNotNull(yeuCau);
            assertEquals("POST", yeuCau.getMethod());
            assertEquals("/api/v1/sv/phieu-yeu-cau-muon", yeuCau.getPath());
            JsonObject than = JsonParser.parseString(yeuCau.getBody().readUtf8()).getAsJsonObject();
            assertEquals(MA_PHONG.toString(), than.get("maPhong").getAsString());
            assertEquals(2, than.get("thoiGianMuonDuKienGio").getAsInt());
            JsonObject dong = than.getAsJsonArray("dsLoaiVaSoLuong").get(0).getAsJsonObject();
            assertEquals(MA_LOAI.toString(), dong.get("maLoai").getAsString());
            assertEquals(2, dong.get("soLuong").getAsInt());
            assertEquals("YC-2026-0003", ketQua.phieu.getMaYeuCau());
        }
    }
}
