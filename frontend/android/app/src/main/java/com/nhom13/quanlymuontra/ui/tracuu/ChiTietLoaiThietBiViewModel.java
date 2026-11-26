package com.nhom13.quanlymuontra.ui.tracuu;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;
import com.nhom13.quanlymuontra.common.nhan.QuyenMuon;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.data.repository.ApiCallback;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.data.repository.TraCuuRepository;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiChiTiet;
import com.nhom13.quanlymuontra.ui.hoso.HoSoMuonTrinhBay;
import java.util.UUID;
import retrofit2.Call;

/**
 * SCR-SV-03 Chi tiết loại thiết bị tại phòng (UI-Screens 5.3, UF-SV-01, UF-SV-03, FA 5.6; FC 6.2, 6.3).
 *
 * <ul>
 *   <li>{@link #load()} chỉ tải khi {@code canTaiLai}; {@link #refresh()} tải vô điều kiện (ĐX-FC-22). Tải lại giữ dữ
 *       liệu cũ trong {@link UiState}; phản hồi của lần tải đã bị thay bằng lần mới hơn bị bỏ qua.
 *   <li>Cổng kiểm tra quyền mượn: mỗi lần chọn Đặt trước hoặc Đăng ký chờ gọi {@code GET /sv/toi} mới, không bộ
 *       đệm (UF-SV-01). Chỉ dựa vào {@code trangThaiQuyenMuon} (ĐX-UF-03): khác Bình thường là bị chặn. Cờ chống bấm
 *       trùng bật đồng bộ trước khi gọi (FA 3.11). Lỗi của {@code svToi} không đi tiếp.
 *   <li>D09-L chỉ làm cổng. Khi cổng cho phép, chỉ phát sự kiện "được phép": SCR-SV-04 (D10-L) và đăng ký chờ (D13-L)
 *       nối vào sự kiện này ở ngày của chúng. Lớp này không gửi đăng ký chờ lên máy chủ.
 *   <li>Hai truy vấn đều là đọc nên hủy im lặng được khi {@code ViewModel} bị hủy (FC 6.3); {@code HUY} không hiển thị
 *       gì (FA 3.3 quy tắc 4). Số lượng khả dụng và ngày dự kiến do máy chủ tính, client không tự tính (FA 3.10).
 * </ul>
 */
public final class ChiTietLoaiThietBiViewModel extends ViewModel {

    /** Hành động sinh viên vừa chọn ở SCR-SV-03. */
    public enum HanhDong {
        DAT_TRUOC(HoSoMuonTrinhBay.HANH_DONG_DAT_TRUOC),
        DANG_KY_CHO(HoSoMuonTrinhBay.HANH_DONG_DANG_KY_CHO);

        @NonNull private final String maThamSo;

        HanhDong(@NonNull String maThamSo) {
            this.maThamSo = maThamSo;
        }

        /** Giá trị của tham số {@code hanhDongDaChon} khi mở SCR-SV-14 ở chế độ bị chặn (FA 5.3 quy tắc 3). */
        @NonNull
        public String getMaThamSo() {
            return maThamSo;
        }
    }

    /** Kết quả của cổng kiểm tra quyền mượn. */
    public static final class KetQuaCong {
        private final boolean duocPhep;
        @NonNull private final HanhDong hanhDong;

        KetQuaCong(boolean duocPhep, @NonNull HanhDong hanhDong) {
            this.duocPhep = duocPhep;
            this.hanhDong = hanhDong;
        }

        /** {@code true} khi quyền mượn là Bình thường: được đi tiếp tới hành động đã chọn. */
        public boolean isDuocPhep() {
            return duocPhep;
        }

        /** Hành động sinh viên đã chọn khi bấm nút. */
        @NonNull
        public HanhDong getHanhDong() {
            return hanhDong;
        }
    }

    private final TraCuuRepository traCuuRepository;
    private final HoSoMuonRepository hoSoMuonRepository;

    private final MutableLiveData<UiState<TraCuuThietBiChiTiet>> chiTiet =
            new MutableLiveData<>(UiState.<TraCuuThietBiChiTiet>dangTai(null));
    private final MutableLiveData<Boolean> dangKiemTraQuyen = new MutableLiveData<>(false);
    private final MutableLiveData<ApiError> loiKiemTraQuyen = new MutableLiveData<>();
    private final MutableLiveData<Event<KetQuaCong>> ketQuaCong = new MutableLiveData<>();

    @Nullable private UUID maLoai;
    @Nullable private UUID maPhong;
    private boolean canTaiLai = true;
    private int lanTai = 0;
    @Nullable private Call<TraCuuThietBiChiTiet> callChiTiet;
    private boolean dangKiemTra = false;
    @Nullable private Call<SinhVienTheInfo> callQuyen;

    public ChiTietLoaiThietBiViewModel(
            @NonNull TraCuuRepository traCuuRepository,
            @NonNull HoSoMuonRepository hoSoMuonRepository) {
        this.traCuuRepository = traCuuRepository;
        this.hoSoMuonRepository = hoSoMuonRepository;
    }

    // ---------------------------------------------------------------- trạng thái phơi bày

    /** Chi tiết loại tại phòng; luôn có giá trị. Không bao giờ ở trạng thái RONG. */
    @NonNull
    public LiveData<UiState<TraCuuThietBiChiTiet>> getChiTiet() {
        return chiTiet;
    }

    /** {@code true} khi đang gọi {@code svToi} cho cổng kiểm tra: nút ghim hiển thị đang xử lý. */
    @NonNull
    public LiveData<Boolean> getDangKiemTraQuyen() {
        return dangKiemTraQuyen;
    }

    /** Lỗi của lần kiểm tra quyền mượn gần nhất ({@code null} khi không có lỗi); hiển thị bằng khối thông báo. */
    @NonNull
    public LiveData<ApiError> getLoiKiemTraQuyen() {
        return loiKiemTraQuyen;
    }

    /** Kết quả cổng, phát một lần (đọc bằng {@code getContentIfNotHandled}). */
    @NonNull
    public LiveData<Event<KetQuaCong>> getKetQuaCong() {
        return ketQuaCong;
    }

    // ---------------------------------------------------------------- tải chi tiết

    /**
     * Đặt loại và phòng cần xem (tham số Safe Args của màn hình). Chỉ lần gọi đầu có tác dụng: một {@code ViewModel}
     * gắn với một Fragment, một Fragment chỉ xem một cặp (loại, phòng).
     */
    public void khoiTao(@NonNull UUID maLoai, @NonNull UUID maPhong) {
        if (this.maLoai != null) {
            return;
        }
        this.maLoai = maLoai;
        this.maPhong = maPhong;
    }

    /** Tải nếu đang cần tải lại (mở màn hình hoặc quay lại từ ngăn xếp). */
    public void load() {
        if (!canTaiLai) {
            return;
        }
        canTaiLai = false;
        tai();
    }

    /** Tải vô điều kiện: kéo để làm mới, Thử lại hoặc "Tải lại" của khung ảnh. */
    public void refresh() {
        canTaiLai = false;
        tai();
    }

    /** Fragment gọi ở {@code onDestroyView} khi không phải xoay màn hình: lần mở sau phải tải lại. */
    public void danhDauCanTaiLai() {
        canTaiLai = true;
    }

    private void tai() {
        if (maLoai == null || maPhong == null) {
            throw new IllegalStateException("Chưa gọi khoiTao trước khi tải");
        }
        huyChiTiet();
        // Lỗi kiểm tra quyền của lần trước không còn đúng khi tải lại màn hình.
        loiKiemTraQuyen.setValue(null);
        final int lan = ++lanTai;
        chiTiet.setValue(UiState.dangTai(duLieuHienCo()));
        callChiTiet =
                traCuuRepository.svTraCuuThietBiChiTiet(
                        maLoai,
                        maPhong,
                        new ApiCallback<TraCuuThietBiChiTiet>() {
                            @Override
                            public void onSuccess(@Nullable TraCuuThietBiChiTiet ketQua) {
                                if (lan != lanTai) {
                                    return;
                                }
                                if (ketQua == null) {
                                    chiTiet.setValue(
                                            UiState.loi(
                                                    ApiError.http(500, null, null, null, null),
                                                    duLieuHienCo()));
                                    return;
                                }
                                chiTiet.setValue(UiState.thanhCong(ketQua));
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanTai || loi.getLoai() == ApiError.Loai.HUY) {
                                    return;
                                }
                                chiTiet.setValue(UiState.loi(loi, duLieuHienCo()));
                            }
                        });
    }

    @Nullable
    private TraCuuThietBiChiTiet duLieuHienCo() {
        UiState<TraCuuThietBiChiTiet> hienTai = chiTiet.getValue();
        return hienTai == null ? null : hienTai.getDuLieu();
    }

    // ---------------------------------------------------------------- cổng kiểm tra quyền mượn

    /** Sinh viên bấm nút Đặt trước. */
    public void chonDatTruoc() {
        kiemTraQuyenMuon(HanhDong.DAT_TRUOC);
    }

    /** Sinh viên bấm nút Đăng ký chờ. */
    public void chonDangKyCho() {
        kiemTraQuyenMuon(HanhDong.DANG_KY_CHO);
    }

    private void kiemTraQuyenMuon(@NonNull final HanhDong hanhDong) {
        if (dangKiemTra) {
            return;
        }
        dangKiemTra = true;
        loiKiemTraQuyen.setValue(null);
        dangKiemTraQuyen.setValue(true);
        callQuyen =
                hoSoMuonRepository.svToi(
                        new ApiCallback<SinhVienTheInfo>() {
                            @Override
                            public void onSuccess(@Nullable SinhVienTheInfo hoSo) {
                                dangKiemTra = false;
                                dangKiemTraQuyen.setValue(false);
                                if (hoSo == null) {
                                    loiKiemTraQuyen.setValue(ApiError.http(500, null, null, null, null));
                                    return;
                                }
                                // ĐX-UF-03: chỉ dựa trên trangThaiQuyenMuon; mã khác Bình thường (kể cả chưa biết) là bị chặn.
                                boolean duocPhep =
                                        QuyenMuon.laBinhThuong(hoSo.getTrangThaiQuyenMuon());
                                ketQuaCong.setValue(new Event<>(new KetQuaCong(duocPhep, hanhDong)));
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                dangKiemTra = false;
                                dangKiemTraQuyen.setValue(false);
                                if (loi.getLoai() != ApiError.Loai.HUY) {
                                    loiKiemTraQuyen.setValue(loi);
                                }
                            }
                        });
    }

    // ---------------------------------------------------------------- vòng đời

    private void huyChiTiet() {
        if (callChiTiet != null) {
            callChiTiet.cancel();
            callChiTiet = null;
        }
    }

    @Override
    protected void onCleared() {
        huyChiTiet();
        if (callQuyen != null) {
            callQuyen.cancel();
            callQuyen = null;
        }
        super.onCleared();
    }
}
