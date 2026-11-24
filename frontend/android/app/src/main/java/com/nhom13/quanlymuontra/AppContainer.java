package com.nhom13.quanlymuontra;

import android.content.Context;
import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.data.api.OkHttpClientFactory;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.data.repository.TraCuuRepository;
import com.nhom13.quanlymuontra.data.repository.XacThucRepository;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenStore;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.api.XacThucApi;
import okhttp3.HttpUrl;
import okhttp3.OkHttpClient;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/**
 * Điểm lắp ráp duy nhất của ứng dụng, tạo một lần trong {@link UngDung} (FA 5.1, ĐX-FA-12).
 *
 * <p>Không dùng framework DI: các thành phần dùng chung ({@code OkHttpClient}, client Retrofit,
 * {@code SessionManager}, các {@code Repository}) được tạo thủ công tại đây và chỉ được thêm vào
 * khi lớp của chúng tồn tại, để không có trường nào không có người dùng. {@code ViewModel} nhận
 * phụ thuộc qua {@link AppViewModelFactory}, không tự truy cập lớp này.
 *
 * <p>Hàm tạo nhận sẵn các thành phần để kiểm thử dựng bằng bản giả; bản dùng thật do {@link
 * #taoChoUngDung(Context)} lắp ráp. D07-L thêm {@link HoSoMuonRepository}; D08-L thêm {@link
 * TraCuuRepository}.
 */
public final class AppContainer {

    private final SessionManager sessionManager;
    private final XacThucRepository xacThucRepository;
    private final HoSoMuonRepository hoSoMuonRepository;
    private final TraCuuRepository traCuuRepository;

    public AppContainer(
            @NonNull SessionManager sessionManager,
            @NonNull XacThucRepository xacThucRepository,
            @NonNull HoSoMuonRepository hoSoMuonRepository,
            @NonNull TraCuuRepository traCuuRepository) {
        this.sessionManager = sessionManager;
        this.xacThucRepository = xacThucRepository;
        this.hoSoMuonRepository = hoSoMuonRepository;
        this.traCuuRepository = traCuuRepository;
    }

    /**
     * Lắp ráp bản dùng thật (FA 5.5, 5.8): {@code SessionManager} trên kho mã hóa, {@code OkHttpClient}
     * chính (có {@code AuthInterceptor} và {@code TokenAuthenticator}), {@code Retrofit} trỏ tới {@code
     * BuildConfig.API_BASE_URL} với bộ chuyển đổi Gson của bộ sinh, rồi các {@code Repository}.
     */
    @NonNull
    public static AppContainer taoChoUngDung(@NonNull Context context) {
        HttpUrl diaChiGoc = HttpUrl.get(BuildConfig.API_BASE_URL);
        SessionManager sessionManager = new SessionManager(TokenStore.create(context));
        OkHttpClient client = OkHttpClientFactory.taoClientChinh(sessionManager, diaChiGoc);
        Retrofit retrofit =
                new Retrofit.Builder()
                        .baseUrl(diaChiGoc)
                        .client(client)
                        .addConverterFactory(GsonConverterFactory.create(new JSON().getGson()))
                        .build();
        XacThucRepository xacThucRepository =
                new XacThucRepository(retrofit.create(XacThucApi.class), sessionManager);
        SinhVienApi sinhVienApi = retrofit.create(SinhVienApi.class);
        HoSoMuonRepository hoSoMuonRepository = new HoSoMuonRepository(sinhVienApi);
        TraCuuRepository traCuuRepository = new TraCuuRepository(sinhVienApi);
        return new AppContainer(
                sessionManager, xacThucRepository, hoSoMuonRepository, traCuuRepository);
    }

    @NonNull
    public SessionManager getSessionManager() {
        return sessionManager;
    }

    @NonNull
    public XacThucRepository getXacThucRepository() {
        return xacThucRepository;
    }

    @NonNull
    public HoSoMuonRepository getHoSoMuonRepository() {
        return hoSoMuonRepository;
    }

    @NonNull
    public TraCuuRepository getTraCuuRepository() {
        return traCuuRepository;
    }
}
