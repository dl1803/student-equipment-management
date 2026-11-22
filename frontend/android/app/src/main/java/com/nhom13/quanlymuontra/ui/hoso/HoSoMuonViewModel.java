package com.nhom13.quanlymuontra.ui.hoso;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.repository.ApiCallback;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.data.repository.XacThucRepository;

/**
 * SCR-SV-14 Hồ sơ mượn (UF-SV-08): tải hồ sơ qua {@link TaiHoSoViewModel} và xử lý nút Đăng xuất.
 *
 * <p>Đăng xuất (UF-CM-03, FA 5.4, UI-Screens 4.1): không hộp xác nhận; {@code XacThucRepository} xóa phiên cục
 * bộ trước khi gửi nên {@code MainActivity} đưa người dùng về đăng nhập ngay. Kết quả gửi lên máy chủ bị bỏ
 * qua và không tự gửi lại (User-Flows 3.1). Yêu cầu đăng xuất đã phát không bị hủy khi {@code ViewModel} bị
 * hủy (FA 3.11: không hủy im lặng thao tác ghi). Cờ chống bấm trùng bật đồng bộ trước khi tạo yêu cầu.
 */
public final class HoSoMuonViewModel extends TaiHoSoViewModel {

    private final XacThucRepository xacThucRepository;
    private final MutableLiveData<Boolean> dangXuat = new MutableLiveData<>(false);
    private boolean daPhatDangXuat = false;

    public HoSoMuonViewModel(
            @NonNull HoSoMuonRepository hoSoMuonRepository,
            @NonNull XacThucRepository xacThucRepository) {
        super(hoSoMuonRepository);
        this.xacThucRepository = xacThucRepository;
    }

    /** {@code true} từ lúc đã bấm Đăng xuất; màn hình dùng để giữ nút ở trạng thái đang xử lý. */
    @NonNull
    public LiveData<Boolean> getDangXuat() {
        return dangXuat;
    }

    /** Đăng xuất. Bấm nhiều lần chỉ gửi một yêu cầu. */
    public void dangXuat() {
        if (daPhatDangXuat) {
            return;
        }
        daPhatDangXuat = true;
        dangXuat.setValue(true);
        xacThucRepository.authDangXuat(
                new ApiCallback<Void>() {
                    @Override
                    public void onSuccess(@Nullable Void ketQua) {
                        // Bỏ qua kết quả: phiên cục bộ đã được xóa (FA 5.4).
                    }

                    @Override
                    public void onError(@NonNull ApiError loi) {
                        // Chưa biết máy chủ đã thu hồi chưa; không tự gửi lại, không báo lỗi (UF-CM-03).
                    }
                });
    }
}
