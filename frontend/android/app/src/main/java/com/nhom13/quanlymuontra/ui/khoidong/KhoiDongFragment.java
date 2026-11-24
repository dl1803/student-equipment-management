package com.nhom13.quanlymuontra.ui.khoidong;

import android.os.Bundle;
import android.view.View;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.navigation.NavController;
import androidx.navigation.fragment.NavHostFragment;
import com.nhom13.quanlymuontra.AppViewModelFactory;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.UngDung;

/**
 * SCR-CM-03 Khởi động và khôi phục phiên (FA 5.3: điểm bắt đầu của nav graph, không có thanh điều
 * hướng dưới). Chỉ hiển thị chỉ báo đang tải; quyết định điều hướng do {@link KhoiDongViewModel}.
 */
public class KhoiDongFragment extends Fragment {

    public KhoiDongFragment() {
        super(R.layout.fragment_khoi_dong);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        UngDung ungDung = (UngDung) requireActivity().getApplication();
        KhoiDongViewModel viewModel =
                new ViewModelProvider(this, new AppViewModelFactory(ungDung.getAppContainer()))
                        .get(KhoiDongViewModel.class);

        viewModel
                .getSuKienDieuHuong()
                .observe(
                        getViewLifecycleOwner(),
                        suKien -> {
                            KhoiDongViewModel.DichDen dich = suKien.getContentIfNotHandled();
                            if (dich == null) {
                                return;
                            }
                            NavController dieuHuong = NavHostFragment.findNavController(this);
                            if (dich == KhoiDongViewModel.DichDen.TRANG_CHU) {
                                dieuHuong.navigate(
                                        KhoiDongFragmentDirections
                                                .actionKhoiDongFragmentToTrangChuFragment());
                            } else {
                                dieuHuong.navigate(
                                        KhoiDongFragmentDirections
                                                .actionKhoiDongFragmentToDangNhapSinhVienFragment());
                            }
                        });
        viewModel.khoiDong();
    }
}
