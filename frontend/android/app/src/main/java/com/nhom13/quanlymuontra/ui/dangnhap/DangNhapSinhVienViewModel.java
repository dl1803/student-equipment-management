package com.nhom13.quanlymuontra.ui.dangnhap;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MediatorLiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.repository.ApiCallback;
import com.nhom13.quanlymuontra.data.repository.XacThucRepository;
import com.nhom13.quanlymuontra.data.session.SessionManager;

/**
 * SCR-CM-01 Đăng nhập sinh viên (UF-CM-01). Giữ trạng thái gửi, lỗi đang hiển thị và các sự kiện một lần.
 *
 * <ul>
 *   <li>Chống gửi trùng: cờ {@code dangGui} bật đồng bộ trước khi tạo yêu cầu (FA 3.11, ĐX-FA-22).
 *   <li>Không hủy yêu cầu đăng nhập khi ViewModel bị hủy: đăng nhập đã phát có thể đã tạo phiên, và
 *       {@code XacThucRepository} ghi token trước khi báo kết quả (FA 3.11).
 *   <li>Lý do kết thúc phiên (làm mới thất bại, hết hạn giữa chừng) đến từ {@code SessionManager}, không
 *       qua tham số Safe Args (FA 5.3 quy tắc 1), và được hiển thị như một lỗi của màn hình này.
 *   <li>Sau mỗi kết quả của lần đăng nhập (thành công hay lỗi) phát sự kiện xóa ô mật khẩu (FA 3.5).
 * </ul>
 */
public final class DangNhapSinhVienViewModel extends ViewModel {

    private final XacThucRepository xacThucRepository;
    private final MutableLiveData<Boolean> dangGui = new MutableLiveData<>(false);
    private final MediatorLiveData<ApiError> loiHienThi = new MediatorLiveData<>();
    private final MutableLiveData<Event<Boolean>> daDangNhap = new MutableLiveData<>();
    private final MutableLiveData<Event<Boolean>> xoaMatKhau = new MutableLiveData<>();

    public DangNhapSinhVienViewModel(
            @NonNull SessionManager sessionManager, @NonNull XacThucRepository xacThucRepository) {
        this.xacThucRepository = xacThucRepository;
        // Nguồn chỉ hoạt động khi có người quan sát loiHienThi; Event được tiêu thụ một lần rồi giữ lại
        // trong loiHienThi nên xoay màn hình không làm mất lý do.
        loiHienThi.addSource(
                sessionManager.getLyDoKetThucPhien(),
                suKien -> {
                    if (suKien == null) {
                        return;
                    }
                    ApiError lyDo = suKien.getContentIfNotHandled();
                    if (lyDo != null) {
                        loiHienThi.setValue(lyDo);
                    }
                });
    }

    /** {@code true} khi đang chờ phản hồi đăng nhập. */
    @NonNull
    public LiveData<Boolean> getDangGui() {
        return dangGui;
    }

    /** Lỗi đang hiển thị; {@code null} khi không có. */
    @NonNull
    public LiveData<ApiError> getLoiHienThi() {
        return loiHienThi;
    }

    /** Phát một lần khi đăng nhập thành công: màn hình mở Trang chủ. */
    @NonNull
    public LiveData<Event<Boolean>> getDaDangNhap() {
        return daDangNhap;
    }

    /** Phát một lần sau mỗi kết quả của lần đăng nhập: màn hình xóa ô mật khẩu. */
    @NonNull
    public LiveData<Event<Boolean>> getXoaMatKhau() {
        return xoaMatKhau;
    }

    /** Gửi đăng nhập. Bỏ qua khi đang gửi hoặc thiếu trường bắt buộc. */
    public void dangNhap(@NonNull String mssv, @NonNull String matKhau) {
        if (Boolean.TRUE.equals(dangGui.getValue()) || mssv.isEmpty() || matKhau.isEmpty()) {
            return;
        }
        dangGui.setValue(true);
        loiHienThi.setValue(null);
        xacThucRepository.authDangNhapSinhVien(
                mssv,
                matKhau,
                new ApiCallback<Void>() {
                    @Override
                    public void onSuccess(@Nullable Void ketQua) {
                        ketThucLanGui();
                        daDangNhap.setValue(new Event<>(true));
                    }

                    @Override
                    public void onError(@NonNull ApiError loi) {
                        ketThucLanGui();
                        loiHienThi.setValue(loi);
                    }
                });
    }

    /**
     * Gỡ lỗi "sai thông tin đăng nhập" khi người dùng sửa lại ô nhập. Không gỡ các lỗi khác (thông báo
     * phiên hết hạn, chờ, mạng, hệ thống): chúng chỉ biến mất ở lần gửi kế tiếp.
     */
    public void xoaLoiDangNhapSai() {
        ApiError loi = loiHienThi.getValue();
        if (loi != null && TrinhBayLoiDangNhap.laLoiDangNhapSai(loi)) {
            loiHienThi.setValue(null);
        }
    }

    private void ketThucLanGui() {
        dangGui.setValue(false);
        xoaMatKhau.setValue(new Event<>(true));
    }
}
