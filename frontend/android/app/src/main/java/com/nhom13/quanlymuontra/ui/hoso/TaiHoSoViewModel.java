package com.nhom13.quanlymuontra.ui.hoso;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.data.repository.ApiCallback;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import retrofit2.Call;

/**
 * Nền chung của hai màn hình cùng tải {@code GET /sv/toi}: SCR-SV-01 và SCR-SV-14 (FA 5.1, 5.6; FC 6.2, 6.3).
 *
 * <ul>
 *   <li>{@link #load()} chỉ tải khi {@code canTaiLai} là {@code true} rồi đặt {@code false}: mở màn hình và quay
 *       lại từ ngăn xếp tải từ máy chủ, xoay màn hình không tải lại (ĐX-FC-22).
 *   <li>{@link #refresh()} tải vô điều kiện (kéo để làm mới, Thử lại).
 *   <li>Khi tải lại, dữ liệu cũ vẫn nằm trong {@link UiState} để giao diện không nhấp nháy.
 *   <li>Phản hồi của yêu cầu đã bị thay bằng yêu cầu mới hơn bị bỏ qua, để kết quả cũ không ghi đè kết quả mới.
 *   <li>{@code svToi} là truy vấn đọc nên hủy im lặng được khi {@code ViewModel} bị hủy (FC 6.3).
 * </ul>
 */
public abstract class TaiHoSoViewModel extends ViewModel {

    private final HoSoMuonRepository hoSoMuonRepository;
    private final MutableLiveData<UiState<SinhVienTheInfo>> uiState =
            new MutableLiveData<>(UiState.<SinhVienTheInfo>dangTai(null));
    private boolean canTaiLai = true;
    private int lanTai = 0;
    @Nullable private Call<SinhVienTheInfo> callDangChay;

    protected TaiHoSoViewModel(@NonNull HoSoMuonRepository hoSoMuonRepository) {
        this.hoSoMuonRepository = hoSoMuonRepository;
    }

    /** Trạng thái tải hồ sơ mượn; luôn có giá trị. */
    @NonNull
    public LiveData<UiState<SinhVienTheInfo>> getUiState() {
        return uiState;
    }

    /** Tải nếu đang cần tải lại (mở màn hình hoặc quay lại từ ngăn xếp). */
    public void load() {
        if (!canTaiLai) {
            return;
        }
        canTaiLai = false;
        tai();
    }

    /** Tải vô điều kiện: kéo để làm mới hoặc Thử lại. */
    public void refresh() {
        canTaiLai = false;
        tai();
    }

    /** Fragment gọi ở {@code onDestroyView} khi không phải xoay màn hình: lần mở sau phải tải lại. */
    public void danhDauCanTaiLai() {
        canTaiLai = true;
    }

    private void tai() {
        huyYeuCauDangChay();
        final int lan = ++lanTai;
        UiState<SinhVienTheInfo> hienTai = uiState.getValue();
        uiState.setValue(UiState.dangTai(hienTai == null ? null : hienTai.getDuLieu()));
        callDangChay =
                hoSoMuonRepository.svToi(
                        new ApiCallback<SinhVienTheInfo>() {
                            @Override
                            public void onSuccess(@Nullable SinhVienTheInfo ketQua) {
                                if (lan != lanTai) {
                                    return;
                                }
                                if (ketQua == null) {
                                    uiState.setValue(
                                            UiState.loi(
                                                    ApiError.http(500, null, null, null, null),
                                                    duLieuHienCo()));
                                    return;
                                }
                                uiState.setValue(UiState.thanhCong(ketQua));
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanTai) {
                                    return;
                                }
                                uiState.setValue(UiState.loi(loi, duLieuHienCo()));
                            }
                        });
    }

    @Nullable
    private SinhVienTheInfo duLieuHienCo() {
        UiState<SinhVienTheInfo> hienTai = uiState.getValue();
        return hienTai == null ? null : hienTai.getDuLieu();
    }

    private void huyYeuCauDangChay() {
        if (callDangChay != null) {
            callDangChay.cancel();
            callDangChay = null;
        }
    }

    @Override
    protected void onCleared() {
        huyYeuCauDangChay();
        super.onCleared();
    }
}
