package com.nhom13.quanlymuontra;

import androidx.annotation.NonNull;
import androidx.lifecycle.ViewModel;
import androidx.lifecycle.ViewModelProvider;

/**
 * Nhà máy duy nhất tạo {@link ViewModel} và đưa phụ thuộc từ {@link AppContainer} vào hàm tạo
 * (FA 5.1, FC 6.3). Mỗi {@code ViewModel} mới được đăng ký bằng một nhánh trong {@link #create}.
 */
public final class AppViewModelFactory implements ViewModelProvider.Factory {

    private final AppContainer appContainer;

    public AppViewModelFactory(@NonNull AppContainer appContainer) {
        this.appContainer = appContainer;
    }

    @NonNull
    @Override
    public <T extends ViewModel> T create(@NonNull Class<T> modelClass) {
        // Chưa có ViewModel nào đăng ký: tạo nhầm lớp lạ phải lộ ngay thay vì trả về null.
        throw new IllegalArgumentException(
                "Chưa đăng ký ViewModel trong AppViewModelFactory: " + modelClass.getName());
    }
}
