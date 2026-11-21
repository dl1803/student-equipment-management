package com.nhom13.quanlymuontra;

import androidx.arch.core.executor.ArchTaskExecutor;
import androidx.arch.core.executor.TaskExecutor;
import com.nhom13.quanlymuontra.data.session.FakeSharedPreferences;
import com.nhom13.quanlymuontra.data.session.FakeTokenCipher;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenStore;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.api.XacThucApi;
import com.nhom13.quanlymuontra.generated.model.TokenResponse;
import java.lang.reflect.Proxy;
import java.nio.charset.StandardCharsets;
import java.util.Base64;
import java.util.Map;
import okhttp3.MediaType;
import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Response;

/**
 * Dụng cụ chung cho test xác thực (D06-L): SessionManager chạy trên kho giả, {@link XacThucApi} giả
 * dựng bằng {@link Proxy} theo tên thao tác (không phụ thuộc chữ ký đầy đủ của mã sinh), phản hồi dựng
 * từ fixture và JWT giả có {@code exp} tùy ý.
 */
public final class HoTroXacThuc {

    private static final MediaType PROBLEM_JSON = MediaType.get("application/problem+json");

    private HoTroXacThuc() {}

    /** LiveData kiểm tra luồng chính bằng Looper Android (không có trên JVM): chạy ngay tại luồng gọi. */
    public static void batDauLiveData() {
        ArchTaskExecutor.getInstance()
                .setDelegate(
                        new TaskExecutor() {
                            @Override
                            public void executeOnDiskIO(Runnable runnable) {
                                runnable.run();
                            }

                            @Override
                            public void postToMainThread(Runnable runnable) {
                                runnable.run();
                            }

                            @Override
                            public boolean isMainThread() {
                                return true;
                            }
                        });
    }

    public static void ketThucLiveData() {
        ArchTaskExecutor.getInstance().setDelegate(null);
    }

    public static SessionManager taoSessionManager() {
        return new SessionManager(new TokenStore(new FakeSharedPreferences(), new FakeTokenCipher()));
    }

    /**
     * @param theoTen tên thao tác ({@code authDangNhapSinhVien}, {@code authLamMoi}, {@code authDangXuat})
     *     mà bản giả trả về {@link Call} tương ứng; thao tác không có trong bản đồ làm test thất bại
     */
    public static XacThucApi taoApi(Map<String, Call<?>> theoTen) {
        return (XacThucApi)
                Proxy.newProxyInstance(
                        XacThucApi.class.getClassLoader(),
                        new Class<?>[] {XacThucApi.class},
                        (doiTuong, phuongThuc, thamSo) -> {
                            Call<?> call = theoTen.get(phuongThuc.getName());
                            if (call == null) {
                                throw new AssertionError(
                                        "Không mong đợi gọi " + phuongThuc.getName());
                            }
                            return call;
                        });
    }

    /** Phản hồi 200 từ fixture {@code thanh-cong/...} chứa {@link TokenResponse}. */
    public static Response<TokenResponse> tokenThanhCong(String tepFixture) {
        TokenResponse thanToken =
                new JSON().getGson().fromJson(Fixtures.doc(tepFixture), TokenResponse.class);
        return Response.success(thanToken);
    }

    /** Phản hồi lỗi problem+json từ fixture {@code loi/...}. */
    public static <T> Response<T> phanHoiLoi(int status, String tepFixture) {
        return Response.error(status, ResponseBody.create(Fixtures.doc(tepFixture), PROBLEM_JSON));
    }

    /** JWT giả dạng {@code header.payload.signature} chỉ có {@code exp} (giây từ epoch). */
    public static String jwtCoExp(long hetHanGiay) {
        return jwtTuThan("{\"sub\":\"1\",\"role\":\"SINH_VIEN\",\"exp\":" + hetHanGiay + "}");
    }

    /** JWT giả với thân tùy ý (dùng cho trường hợp thiếu hoặc hỏng {@code exp}). */
    public static String jwtTuThan(String thanJson) {
        Base64.Encoder ma = Base64.getUrlEncoder().withoutPadding();
        return ma.encodeToString("{\"alg\":\"HS256\",\"typ\":\"JWT\"}".getBytes(StandardCharsets.UTF_8))
                + "."
                + ma.encodeToString(thanJson.getBytes(StandardCharsets.UTF_8))
                + ".chu-ky-gia";
    }
}
