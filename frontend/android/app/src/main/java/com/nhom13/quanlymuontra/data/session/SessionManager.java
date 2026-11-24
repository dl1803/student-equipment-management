package com.nhom13.quanlymuontra.data.session;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;

/**
 * Lớp duy nhất giữ token trong bộ nhớ và phát {@link LiveData} trạng thái phiên (FA 5.1, FC 6.5).
 * Mã khác lấy token qua {@link #getTokenPair()}; việc ghi xuống đĩa do {@link TokenStore} đảm nhận.
 *
 * <p>An toàn khi gọi từ nhiều luồng (luồng chính và luồng mạng của OkHttp): thay đổi phiên được
 * tuần tự hóa, còn {@link #getTokenPair()} đọc không khóa. Trạng thái phát bằng {@code postValue},
 * nên người quan sát nhận ở luồng chính và có thể gộp các thay đổi liên tiếp; mã cần token mới nhất
 * phải gọi {@link #getTokenPair()}, không đọc từ {@link LiveData}.
 *
 * <p>Phiên kết thúc do làm mới thất bại (D04-L, FA 5.4, ĐX-FA-23) đi qua {@link
 * #ketThucPhien(ApiError)}: ngoài trạng thái {@link TrangThaiPhien#KHONG_CO_PHIEN}, lý do được phát
 * một lần qua {@link #getLyDoKetThucPhien()} để màn hình đăng nhập báo đúng nguyên nhân.
 */
public final class SessionManager {

    @NonNull private final TokenStore tokenStore;

    @NonNull
    private final MutableLiveData<TrangThaiPhien> trangThaiPhien =
            new MutableLiveData<>(TrangThaiPhien.CHUA_KHOI_PHUC);

    @NonNull
    private final MutableLiveData<Event<ApiError>> lyDoKetThucPhien = new MutableLiveData<>();

    @Nullable private volatile TokenPair capToken;

    public SessionManager(@NonNull TokenStore tokenStore) {
        this.tokenStore = tokenStore;
    }

    /** Trạng thái phiên, bắt đầu là {@link TrangThaiPhien#CHUA_KHOI_PHUC}. */
    @NonNull
    public LiveData<TrangThaiPhien> getTrangThaiPhien() {
        return trangThaiPhien;
    }

    /**
     * Lý do phiên bị kết thúc do lần làm mới thất bại, giữ nguyên status, maLoi, traceId của lần
     * làm mới (FA 5.4). Chưa có sự kiện nào cho tới khi {@link #ketThucPhien(ApiError)} được gọi;
     * đăng xuất chủ động qua {@link #clear()} không phát sự kiện này.
     */
    @NonNull
    public LiveData<Event<ApiError>> getLyDoKetThucPhien() {
        return lyDoKetThucPhien;
    }

    /** Cặp token hiện tại, hoặc {@code null} khi không có phiên. */
    @Nullable
    public TokenPair getTokenPair() {
        return capToken;
    }

    /**
     * Đọc phiên đã lưu khi mở ứng dụng. Có cặp token hợp lệ thì thành {@link
     * TrangThaiPhien#CO_PHIEN}; chưa có phiên, khóa mất hoặc bản mã hỏng thì dữ liệu lưu bị xóa
     * (do {@link TokenStore#read()}) và thành {@link TrangThaiPhien#KHONG_CO_PHIEN} (FA 5.4).
     */
    public synchronized void restore() {
        TokenPair daLuu = tokenStore.read();
        capToken = daLuu;
        trangThaiPhien.postValue(
                daLuu == null ? TrangThaiPhien.KHONG_CO_PHIEN : TrangThaiPhien.CO_PHIEN);
    }

    /**
     * Ghi cặp token mới xuống kho lưu rồi đặt vào bộ nhớ, thành {@link TrangThaiPhien#CO_PHIEN}.
     * Khi ghi xuống kho thất bại, phiên vẫn dùng được trong bộ nhớ của lần chạy này nhưng sẽ không
     * còn sau khi mở lại ứng dụng.
     *
     * @return {@code true} khi cặp token đã được lưu bền vững
     */
    public synchronized boolean save(@NonNull TokenPair cap) {
        boolean daLuu = tokenStore.save(cap);
        capToken = cap;
        trangThaiPhien.postValue(TrangThaiPhien.CO_PHIEN);
        return daLuu;
    }

    /** Xóa phiên khỏi kho lưu và bộ nhớ, thành {@link TrangThaiPhien#KHONG_CO_PHIEN}. */
    public synchronized void clear() {
        tokenStore.clear();
        capToken = null;
        trangThaiPhien.postValue(TrangThaiPhien.KHONG_CO_PHIEN);
    }

    /**
     * Kết thúc phiên vì lần làm mới token thất bại: phát {@code lyDo} một lần rồi xóa phiên như
     * {@link #clear()}. Lý do được phát trước trạng thái để người quan sát trạng thái đọc được
     * lý do khi điều hướng về đăng nhập (FA 5.4, ĐX-FA-23).
     */
    public synchronized void ketThucPhien(@NonNull ApiError lyDo) {
        lyDoKetThucPhien.postValue(new Event<>(lyDo));
        clear();
    }
}
