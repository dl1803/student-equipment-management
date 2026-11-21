package com.nhom13.quanlymuontra.ui.khoidong;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.repository.ApiCallback;
import com.nhom13.quanlymuontra.data.repository.XacThucRepository;
import com.nhom13.quanlymuontra.data.session.HanAccessToken;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import java.time.Clock;
import java.time.Instant;

/**
 * SCR-CM-03 Khởi động và khôi phục phiên (UF-CM-03, FA 5.4): quyết định vào Trang chủ hay về đăng nhập.
 *
 * <ul>
 *   <li>Không có phiên: về đăng nhập (không kèm thông báo vì trước đó không có phiên).
 *   <li>Access token còn hạn ({@code exp}): vào Trang chủ.
 *   <li>Access token hết hạn: làm mới bằng refresh token; 200 thì vào Trang chủ, mọi lỗi thì phiên bị kết
 *       thúc kèm {@code ApiError} trong {@code SessionManager} và về đăng nhập. 429 không tự lặp.
 * </ul>
 *
 * Quyết định phát đúng một lần bằng {@link Event}; xoay màn hình trong lúc làm mới không làm mới lần hai.
 */
public final class KhoiDongViewModel extends ViewModel {

    /** Đích sau khi kiểm tra phiên. */
    public enum DichDen {
        TRANG_CHU,
        DANG_NHAP
    }

    private final SessionManager sessionManager;
    private final XacThucRepository xacThucRepository;
    private final Clock dongHo;
    private final MutableLiveData<Event<DichDen>> suKienDieuHuong = new MutableLiveData<>();
    private boolean daKhoiDong;

    public KhoiDongViewModel(
            @NonNull SessionManager sessionManager,
            @NonNull XacThucRepository xacThucRepository,
            @NonNull Clock dongHo) {
        this.sessionManager = sessionManager;
        this.xacThucRepository = xacThucRepository;
        this.dongHo = dongHo;
    }

    @NonNull
    public LiveData<Event<DichDen>> getSuKienDieuHuong() {
        return suKienDieuHuong;
    }

    /** Gọi từ {@code onViewCreated}; chỉ có tác dụng ở lần gọi đầu của ViewModel này. */
    public void khoiDong() {
        if (daKhoiDong) {
            return;
        }
        daKhoiDong = true;
        sessionManager.restore();
        // getTokenPair() có giá trị ngay sau restore(); không chờ LiveData trạng thái (postValue).
        TokenPair capToken = sessionManager.getTokenPair();
        if (capToken == null) {
            phat(DichDen.DANG_NHAP);
            return;
        }
        if (HanAccessToken.conHan(capToken.getAccessToken(), Instant.now(dongHo))) {
            phat(DichDen.TRANG_CHU);
            return;
        }
        xacThucRepository.authLamMoi(
                new ApiCallback<Void>() {
                    @Override
                    public void onSuccess(@Nullable Void ketQua) {
                        phat(DichDen.TRANG_CHU);
                    }

                    @Override
                    public void onError(@NonNull ApiError loi) {
                        // Repository đã kết thúc phiên và giữ lý do cho màn hình đăng nhập.
                        phat(DichDen.DANG_NHAP);
                    }
                });
    }

    private void phat(@NonNull DichDen dich) {
        suKienDieuHuong.setValue(new Event<>(dich));
    }
}
