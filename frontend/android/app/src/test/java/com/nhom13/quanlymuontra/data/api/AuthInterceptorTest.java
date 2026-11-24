package com.nhom13.quanlymuontra.data.api;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;

import com.nhom13.quanlymuontra.HoTroLiveData;
import com.nhom13.quanlymuontra.data.session.FakeSharedPreferences;
import com.nhom13.quanlymuontra.data.session.FakeTokenCipher;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import com.nhom13.quanlymuontra.data.session.TokenStore;
import java.io.IOException;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import okhttp3.mockwebserver.RecordedRequest;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;

public class AuthInterceptorTest {

    private SessionManager phien;
    private MockWebServer maychu;
    private OkHttpClient client;

    @Before
    public void chuanBi() throws IOException {
        HoTroLiveData.batDau();
        TokenStore tokenStore = new TokenStore(new FakeSharedPreferences(), new FakeTokenCipher());
        phien = new SessionManager(tokenStore);
        maychu = new MockWebServer();
        maychu.start();
        client = new OkHttpClient.Builder().addInterceptor(new AuthInterceptor(phien)).build();
    }

    @After
    public void don() throws IOException {
        maychu.shutdown();
        HoTroLiveData.ketThuc();
    }

    @Test
    public void FA35_CoPhien_GanBearerAccessTokenChoYeuCauThuong() throws Exception {
        phien.save(new TokenPair("access-1", "refresh-1"));

        RecordedRequest daNhan = guiGet("/api/v1/sv/toi");

        assertEquals("Bearer access-1", daNhan.getHeader("Authorization"));
    }

    @Test
    public void FA35_BonEndpointCongKhai_KhongGanAuthorizationDuCoPhien() throws Exception {
        phien.save(new TokenPair("access-1", "refresh-1"));
        String[] cacDuongDan = {
            DuongDanApi.DANG_NHAP_SINH_VIEN,
            DuongDanApi.DANG_NHAP_NHAN_VIEN,
            DuongDanApi.LAM_MOI,
            DuongDanApi.DANG_XUAT
        };

        for (String duongDan : cacDuongDan) {
            assertNull(duongDan, guiGet(duongDan).getHeader("Authorization"));
        }
    }

    @Test
    public void FA35_ChuaCoPhien_GuiKhongKemToken() throws Exception {
        RecordedRequest daNhan = guiGet("/api/v1/sv/toi");

        assertNull(daNhan.getHeader("Authorization"));
    }

    @Test
    public void FA35_TokenDoiSauKhiLamMoi_YeuCauSauDungTokenMoiNhat() throws Exception {
        phien.save(new TokenPair("access-1", "refresh-1"));
        guiGet("/api/v1/sv/toi");

        phien.save(new TokenPair("access-2", "refresh-2"));
        RecordedRequest daNhan = guiGet("/api/v1/sv/toi");

        assertEquals("Bearer access-2", daNhan.getHeader("Authorization"));
    }

    private RecordedRequest guiGet(String duongDan) throws Exception {
        maychu.enqueue(new MockResponse().setResponseCode(200).setBody("{}"));
        client.newCall(new Request.Builder().url(maychu.url(duongDan)).build()).execute().close();
        return maychu.takeRequest();
    }
}
