package com.nhom13.quanlymuontra;

import androidx.arch.core.executor.ArchTaskExecutor;
import androidx.arch.core.executor.TaskExecutor;

/**
 * Hỗ trợ kiểm thử JVM cho lớp phát {@code LiveData} bằng {@code postValue}. {@code LiveData} kiểm
 * tra luồng chính bằng {@code Looper} của Android, không có trên JVM: thay bằng bộ thực thi chạy
 * ngay tại luồng gọi, cùng cách {@code InstantTaskExecutorRule} của core-testing làm và cùng cách
 * {@code SessionManagerTest} đang dùng (FC 9.1 cấm thêm thư viện kiểm thử).
 */
public final class HoTroLiveData {

    private HoTroLiveData() {}

    /** Gọi ở {@code @Before}. */
    public static void batDau() {
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

    /** Gọi ở {@code @After}. */
    public static void ketThuc() {
        ArchTaskExecutor.getInstance().setDelegate(null);
    }
}
