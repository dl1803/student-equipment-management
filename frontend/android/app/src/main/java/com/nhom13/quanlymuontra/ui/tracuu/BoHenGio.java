package com.nhom13.quanlymuontra.ui.tracuu;

import android.os.Handler;
import android.os.Looper;
import androidx.annotation.NonNull;

/**
 * Bộ hẹn giờ cho độ trễ gõ của ô tìm kiếm (ĐX-FA-05). Tách thành giao diện để {@code ViewModel} kiểm thử
 * được trên JVM mà không cần {@code Looper}: test dùng bản giả chạy việc theo yêu cầu.
 */
public interface BoHenGio {

    /**
     * Hẹn chạy {@code viec} sau {@code doTreMs} mili giây trên luồng chính.
     *
     * @return hành động hủy lịch; gọi sau khi việc đã chạy thì không có tác dụng
     */
    @NonNull
    Runnable datLich(@NonNull Runnable viec, long doTreMs);

    /** Bản dùng thật: {@link Handler} của luồng chính. */
    @NonNull
    static BoHenGio trenLuongChinh() {
        return (viec, doTreMs) -> {
            Handler handler = new Handler(Looper.getMainLooper());
            handler.postDelayed(viec, doTreMs);
            return () -> handler.removeCallbacks(viec);
        };
    }
}
