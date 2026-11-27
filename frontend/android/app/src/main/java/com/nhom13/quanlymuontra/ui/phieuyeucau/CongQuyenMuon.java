package com.nhom13.quanlymuontra.ui.phieuyeucau;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import com.nhom13.quanlymuontra.common.nhan.QuyenMuon;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.repository.ApiCallback;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;
import retrofit2.Call;

/**
 * Cổng kiểm tra quyền mượn dùng chung cho các lối vào SCR-SV-04 và hành động Đăng ký chờ (FA 5.3 quy tắc 3, ĐX-UF-03,
 * ĐX-UF-04): mỗi lần {@link #kiemTra} gọi {@code GET /sv/toi} mới, không bộ đệm; chỉ dựa vào {@code
 * trangThaiQuyenMuon}, mã khác Bình thường (kể cả mã chưa biết) là bị chặn. Cờ chống bấm trùng bật đồng bộ trước khi
 * tạo {@code Call} (FA 3.11). Lỗi của {@code svToi} không đi tiếp (hiển thị bằng khối thông báo ở màn hình gọi).
 *
 * <p>Lớp này không phải {@code ViewModel}: do {@code ViewModel} của màn hình gọi tạo và gọi {@link #huy} ở {@code
 * onCleared}. {@code svToi} là truy vấn đọc nên hủy im lặng được (FC 6.3); {@code HUY} không hiển thị gì.
 * {@code ChiTietLoaiThietBiViewModel} (D09-L) vẫn giữ bản cổng riêng của nó; việc gộp không thuộc D10-L.
 */
public final class CongQuyenMuon {

    /** Kết quả cổng: được phép đi tiếp hay bị chặn, kèm hành động sinh viên đã chọn. */
    public static final class KetQua {
        private final boolean duocPhep;
        @NonNull private final HanhDong hanhDong;

        KetQua(boolean duocPhep, @NonNull HanhDong hanhDong) {
            this.duocPhep = duocPhep;
            this.hanhDong = hanhDong;
        }

        /** {@code true} khi quyền mượn là Bình thường. */
        public boolean isDuocPhep() {
            return duocPhep;
        }

        @NonNull
        public HanhDong getHanhDong() {
            return hanhDong;
        }
    }

    private final HoSoMuonRepository hoSoMuonRepository;
    private final MutableLiveData<Boolean> dangKiemTra = new MutableLiveData<>(false);
    private final MutableLiveData<ApiError> loi = new MutableLiveData<>();
    private final MutableLiveData<Event<KetQua>> ketQua = new MutableLiveData<>();
    private boolean dangChay = false;
    @Nullable private Call<SinhVienTheInfo> call;

    public CongQuyenMuon(@NonNull HoSoMuonRepository hoSoMuonRepository) {
        this.hoSoMuonRepository = hoSoMuonRepository;
    }

    /** {@code true} khi đang gọi {@code svToi}. */
    @NonNull
    public LiveData<Boolean> getDangKiemTra() {
        return dangKiemTra;
    }

    /** Lỗi của lần kiểm tra gần nhất; {@code null} khi không có lỗi. */
    @NonNull
    public LiveData<ApiError> getLoi() {
        return loi;
    }

    /** Kết quả cổng, phát một lần (đọc bằng {@code getContentIfNotHandled}). */
    @NonNull
    public LiveData<Event<KetQua>> getKetQua() {
        return ketQua;
    }

    /** Sinh viên chọn {@code hanhDong}: gọi {@code svToi} mới; bỏ qua nếu một lần kiểm tra đang chạy. */
    public void kiemTra(@NonNull final HanhDong hanhDong) {
        if (dangChay) {
            return;
        }
        dangChay = true;
        loi.setValue(null);
        dangKiemTra.setValue(true);
        call =
                hoSoMuonRepository.svToi(
                        new ApiCallback<SinhVienTheInfo>() {
                            @Override
                            public void onSuccess(@Nullable SinhVienTheInfo hoSo) {
                                dangChay = false;
                                dangKiemTra.setValue(false);
                                if (hoSo == null) {
                                    loi.setValue(ApiError.http(500, null, null, null, null));
                                    return;
                                }
                                boolean duocPhep = QuyenMuon.laBinhThuong(hoSo.getTrangThaiQuyenMuon());
                                ketQua.setValue(new Event<>(new KetQua(duocPhep, hanhDong)));
                            }

                            @Override
                            public void onError(@NonNull ApiError loiMoi) {
                                dangChay = false;
                                dangKiemTra.setValue(false);
                                if (loiMoi.getLoai() != ApiError.Loai.HUY) {
                                    loi.setValue(loiMoi);
                                }
                            }
                        });
    }

    /** Hủy lần kiểm tra đang chạy (gọi ở {@code onCleared} của {@code ViewModel} chủ). */
    public void huy() {
        if (call != null) {
            call.cancel();
            call = null;
        }
    }
}
