package com.nhom13.quanlymuontra.data.repository;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.data.api.ApiErrorParser;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import com.nhom13.quanlymuontra.generated.api.XacThucApi;
import com.nhom13.quanlymuontra.generated.model.DangNhapSinhVienRequest;
import com.nhom13.quanlymuontra.generated.model.DangXuatRequest;
import com.nhom13.quanlymuontra.generated.model.LamMoiRequest;
import com.nhom13.quanlymuontra.generated.model.TokenResponse;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Nhóm API Xác thực của sinh viên (FA 5.1, FC 6.4): đăng nhập, làm mới phiên khi khởi động, đăng xuất.
 *
 * <p>Token chỉ đi giữa lớp này và {@link SessionManager}; không trả token ra ngoài tầng dữ liệu
 * (FC 6.5). Mọi kết quả trả qua {@link ApiCallback} trên luồng gọi lại của Retrofit (luồng chính trên
 * Android). Phương thức nào cũng trả {@link Call} để bên gọi có thể hủy; riêng đăng nhập và đăng xuất,
 * bên gọi không được hủy yêu cầu đã phát (FA 3.11: không hủy im lặng thao tác ghi).
 */
public final class XacThucRepository {

    private final XacThucApi api;
    private final SessionManager sessionManager;

    public XacThucRepository(@NonNull XacThucApi api, @NonNull SessionManager sessionManager) {
        this.api = api;
        this.sessionManager = sessionManager;
    }

    /**
     * {@code POST /auth/sinh-vien/dang-nhap} ({@code authDangNhapSinhVien}). Thành công thì ghi cặp
     * token mới vào {@link SessionManager} rồi gọi {@code onSuccess(null)}; phản hồi 200 thiếu token
     * được coi là lỗi hệ thống chung.
     */
    @NonNull
    public Call<TokenResponse> authDangNhapSinhVien(
            @NonNull String mssv, @NonNull String matKhau, @NonNull ApiCallback<Void> callback) {
        DangNhapSinhVienRequest yeuCau = new DangNhapSinhVienRequest();
        yeuCau.setMssv(mssv);
        yeuCau.setMatKhau(matKhau);
        Call<TokenResponse> call = api.authDangNhapSinhVien(yeuCau);
        call.enqueue(
                new Callback<TokenResponse>() {
                    @Override
                    public void onResponse(
                            @NonNull Call<TokenResponse> goi,
                            @NonNull Response<TokenResponse> phanHoi) {
                        if (!phanHoi.isSuccessful()) {
                            callback.onError(ApiErrorParser.tuPhanHoiLoi(phanHoi));
                            return;
                        }
                        TokenPair capMoi = docCapToken(phanHoi.body());
                        if (capMoi == null) {
                            callback.onError(ApiErrorParser.loiHeThongChung());
                            return;
                        }
                        sessionManager.save(capMoi);
                        callback.onSuccess(null);
                    }

                    @Override
                    public void onFailure(@NonNull Call<TokenResponse> goi, @NonNull Throwable loi) {
                        callback.onError(ApiErrorParser.tuNgoaiLe(goi, loi));
                    }
                });
        return call;
    }

    /**
     * {@code POST /auth/lam-moi} ({@code authLamMoi}) bằng refresh token hiện có, dùng khi khởi động mà
     * access token đã hết hạn (FA 5.4). 200 thì ghi cặp token mới. Mọi lỗi (401, 429, mạng, quá thời gian,
     * 500, phản hồi thiếu token) kết thúc phiên bằng {@link SessionManager#ketThucPhien(ApiError)} giữ
     * nguyên {@link ApiError} rồi gọi {@code onError}; không tự làm mới hay đăng nhập lại.
     *
     * @throws IllegalStateException khi không có phiên để làm mới (lỗi của bên gọi)
     */
    @NonNull
    public Call<TokenResponse> authLamMoi(@NonNull ApiCallback<Void> callback) {
        TokenPair hienTai = sessionManager.getTokenPair();
        if (hienTai == null) {
            throw new IllegalStateException("Không có phiên để làm mới");
        }
        LamMoiRequest yeuCau = new LamMoiRequest();
        yeuCau.setRefreshToken(hienTai.getRefreshToken());
        Call<TokenResponse> call = api.authLamMoi(yeuCau);
        call.enqueue(
                new Callback<TokenResponse>() {
                    @Override
                    public void onResponse(
                            @NonNull Call<TokenResponse> goi,
                            @NonNull Response<TokenResponse> phanHoi) {
                        if (!phanHoi.isSuccessful()) {
                            ketThucPhienVaBaoLoi(ApiErrorParser.tuPhanHoiLoi(phanHoi), callback);
                            return;
                        }
                        TokenPair capMoi = docCapToken(phanHoi.body());
                        if (capMoi == null) {
                            ketThucPhienVaBaoLoi(ApiErrorParser.loiHeThongChung(), callback);
                            return;
                        }
                        sessionManager.save(capMoi);
                        callback.onSuccess(null);
                    }

                    @Override
                    public void onFailure(@NonNull Call<TokenResponse> goi, @NonNull Throwable loi) {
                        ketThucPhienVaBaoLoi(ApiErrorParser.tuNgoaiLe(goi, loi), callback);
                    }
                });
        return call;
    }

    /**
     * {@code POST /auth/dang-xuat} ({@code authDangXuat}). Xóa phiên cục bộ <b>trước</b> khi gửi và không
     * phụ thuộc kết quả (UI-Screens 4.6: xóa phiên "kể cả khi yêu cầu đăng xuất lỗi"), nên người dùng không
     * phải chờ mạng. Kết quả gửi lên máy chủ chỉ báo qua {@code callback}: {@code onSuccess} nghĩa là máy chủ
     * đã thu hồi refresh token; {@code onError} nghĩa là chưa biết (không tự gửi lại, User-Flows 3.1).
     *
     * @return yêu cầu đã phát, hoặc {@code null} khi không có refresh token để gửi (đã xóa phiên và gọi
     *     {@code onSuccess(null)} ngay)
     */
    @Nullable
    public Call<Void> authDangXuat(@NonNull ApiCallback<Void> callback) {
        TokenPair hienTai = sessionManager.getTokenPair();
        sessionManager.clear();
        if (hienTai == null) {
            callback.onSuccess(null);
            return null;
        }
        DangXuatRequest yeuCau = new DangXuatRequest();
        yeuCau.setRefreshToken(hienTai.getRefreshToken());
        Call<Void> call = api.authDangXuat(yeuCau);
        call.enqueue(
                new Callback<Void>() {
                    @Override
                    public void onResponse(@NonNull Call<Void> goi, @NonNull Response<Void> phanHoi) {
                        if (phanHoi.isSuccessful()) {
                            callback.onSuccess(null);
                        } else {
                            callback.onError(ApiErrorParser.tuPhanHoiLoi(phanHoi));
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<Void> goi, @NonNull Throwable loi) {
                        callback.onError(ApiErrorParser.tuNgoaiLe(goi, loi));
                    }
                });
        return call;
    }

    private void ketThucPhienVaBaoLoi(@NonNull ApiError loi, @NonNull ApiCallback<Void> callback) {
        sessionManager.ketThucPhien(loi);
        callback.onError(loi);
    }

    /** @return cặp token hợp lệ từ phản hồi, hoặc {@code null} khi thiếu hoặc rỗng */
    @Nullable
    private static TokenPair docCapToken(@Nullable TokenResponse phanHoi) {
        if (phanHoi == null) {
            return null;
        }
        String accessToken = phanHoi.getAccessToken();
        String refreshToken = phanHoi.getRefreshToken();
        if (accessToken == null || accessToken.isEmpty() || refreshToken == null || refreshToken.isEmpty()) {
            return null;
        }
        return new TokenPair(accessToken, refreshToken);
    }
}
