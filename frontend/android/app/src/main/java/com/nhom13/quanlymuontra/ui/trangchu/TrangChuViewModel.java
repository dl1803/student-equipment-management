package com.nhom13.quanlymuontra.ui.trangchu;

import androidx.annotation.NonNull;
import androidx.lifecycle.LiveData;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.ui.hoso.TaiHoSoViewModel;
import com.nhom13.quanlymuontra.ui.phieuyeucau.CongQuyenMuon;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;

/**
 * SCR-SV-01 Trang chủ sinh viên (UI-Screens 5.1): tải hồ sơ mượn để hiển thị lời chào và thẻ quyền mượn; quyền mượn
 * lấy nguyên từ máy chủ, không suy ra ở client. Mọi hành vi tải do {@link TaiHoSoViewModel} đảm nhận.
 *
 * <p>D10-L: lối vào Đặt trước thiết bị chạy cổng quyền mượn ({@link CongQuyenMuon}, FA 5.3 quy tắc 3): mỗi lần chọn gọi
 * {@code GET /sv/toi} mới, không dùng hồ sơ đã tải cho thẻ quyền mượn.
 */
public final class TrangChuViewModel extends TaiHoSoViewModel {

    private final CongQuyenMuon cong;

    public TrangChuViewModel(@NonNull HoSoMuonRepository hoSoMuonRepository) {
        super(hoSoMuonRepository);
        this.cong = new CongQuyenMuon(hoSoMuonRepository);
    }

    /** Sinh viên chọn lối vào Đặt trước thiết bị. */
    public void chonDatTruoc() {
        cong.kiemTra(HanhDong.DAT_TRUOC);
    }

    /** {@code true} khi đang gọi {@code svToi} cho cổng. */
    @NonNull
    public LiveData<Boolean> getDangKiemTraQuyen() {
        return cong.getDangKiemTra();
    }

    /** Lỗi của lần kiểm tra quyền mượn gần nhất ({@code null} khi không có lỗi). */
    @NonNull
    public LiveData<ApiError> getLoiKiemTraQuyen() {
        return cong.getLoi();
    }

    /** Kết quả cổng, phát một lần. */
    @NonNull
    public LiveData<Event<CongQuyenMuon.KetQua>> getKetQuaCong() {
        return cong.getKetQua();
    }

    @Override
    protected void onCleared() {
        cong.huy();
        super.onCleared();
    }
}
