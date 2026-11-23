package com.nhom13.quanlymuontra;

import androidx.annotation.NonNull;
import androidx.lifecycle.ViewModel;
import androidx.lifecycle.ViewModelProvider;
import com.nhom13.quanlymuontra.ui.dangnhap.DangNhapSinhVienViewModel;
import com.nhom13.quanlymuontra.ui.hoso.HoSoMuonViewModel;
import com.nhom13.quanlymuontra.ui.khoidong.KhoiDongViewModel;
import com.nhom13.quanlymuontra.ui.tracuu.BoHenGio;
import com.nhom13.quanlymuontra.ui.tracuu.TraCuuThietBiViewModel;
import com.nhom13.quanlymuontra.ui.trangchu.TrangChuViewModel;
import java.time.Clock;

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
        // So sánh đúng lớp (không dùng isAssignableFrom): ViewModel.class hay lớp cha khác phải bị từ chối.
        if (KhoiDongViewModel.class.equals(modelClass)) {
            return modelClass.cast(
                    new KhoiDongViewModel(
                            appContainer.getSessionManager(),
                            appContainer.getXacThucRepository(),
                            Clock.systemUTC()));
        }
        if (DangNhapSinhVienViewModel.class.equals(modelClass)) {
            return modelClass.cast(
                    new DangNhapSinhVienViewModel(
                            appContainer.getSessionManager(), appContainer.getXacThucRepository()));
        }
        if (TrangChuViewModel.class.equals(modelClass)) {
            return modelClass.cast(new TrangChuViewModel(appContainer.getHoSoMuonRepository()));
        }
        if (HoSoMuonViewModel.class.equals(modelClass)) {
            return modelClass.cast(
                    new HoSoMuonViewModel(
                            appContainer.getHoSoMuonRepository(),
                            appContainer.getXacThucRepository()));
        }
        if (TraCuuThietBiViewModel.class.equals(modelClass)) {
            return modelClass.cast(
                    new TraCuuThietBiViewModel(
                            appContainer.getTraCuuRepository(), BoHenGio.trenLuongChinh()));
        }
        // Lớp chưa đăng ký: tạo nhầm lớp lạ phải lộ ngay thay vì trả về null.
        throw new IllegalArgumentException(
                "Chưa đăng ký ViewModel trong AppViewModelFactory: " + modelClass.getName());
    }
}
