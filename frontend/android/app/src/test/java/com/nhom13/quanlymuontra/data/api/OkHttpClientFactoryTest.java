package com.nhom13.quanlymuontra.data.api;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertSame;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.HoTroLiveData;
import com.nhom13.quanlymuontra.data.session.FakeSharedPreferences;
import com.nhom13.quanlymuontra.data.session.FakeTokenCipher;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenStore;
import okhttp3.Authenticator;
import okhttp3.HttpUrl;
import okhttp3.Interceptor;
import okhttp3.OkHttpClient;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;

public class OkHttpClientFactoryTest {

    private static final int TONG_THOI_GIAN_MS = 90_000;

    private SessionManager phien;

    @Before
    public void chuanBi() {
        HoTroLiveData.batDau();
        TokenStore tokenStore = new TokenStore(new FakeSharedPreferences(), new FakeTokenCipher());
        phien = new SessionManager(tokenStore);
    }

    @After
    public void don() {
        HoTroLiveData.ketThuc();
    }

    @Test
    public void DXFA04_ClientChinh_ThoiGianChoTongCong90GiayVaKhongTuGuiLai() {
        OkHttpClient client =
                OkHttpClientFactory.taoClientChinh(phien, HttpUrl.get("http://localhost/"));

        assertEquals(TONG_THOI_GIAN_MS, client.callTimeoutMillis());
        assertEquals(TONG_THOI_GIAN_MS, client.connectTimeoutMillis());
        assertEquals(TONG_THOI_GIAN_MS, client.readTimeoutMillis());
        assertEquals(TONG_THOI_GIAN_MS, client.writeTimeoutMillis());
        assertFalse(client.retryOnConnectionFailure());
    }

    @Test
    public void FA54_ClientChinh_CoAuthInterceptorVaTokenAuthenticator() {
        OkHttpClient client =
                OkHttpClientFactory.taoClientChinh(phien, HttpUrl.get("http://localhost/"));

        boolean coAuthInterceptor = false;
        for (Interceptor mot : client.interceptors()) {
            coAuthInterceptor = coAuthInterceptor || mot instanceof AuthInterceptor;
        }
        assertTrue(coAuthInterceptor);
        assertTrue(client.authenticator() instanceof TokenAuthenticator);
    }

    @Test
    public void FA54_ClientLamMoi_KhongCoInterceptorVaAuthenticatorNhungCungThoiGianCho() {
        OkHttpClient client = OkHttpClientFactory.taoClientLamMoi();

        assertTrue(client.interceptors().isEmpty());
        assertSame(Authenticator.NONE, client.authenticator());
        assertEquals(TONG_THOI_GIAN_MS, client.callTimeoutMillis());
        assertFalse(client.retryOnConnectionFailure());
    }
}
