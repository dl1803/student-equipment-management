package com.nhom13.quanlymuontra.ui.trangchu;

import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.ui.hoso.TaiHoSoViewModel;

/**
 * SCR-SV-01 Trang chủ sinh viên (UI-Screens 5.1): chỉ tải hồ sơ mượn để hiển thị lời chào và thẻ quyền
 * mượn; quyền mượn lấy nguyên từ máy chủ, không suy ra ở client. Mọi hành vi tải do {@link
 * TaiHoSoViewModel} đảm nhận.
 */
public final class TrangChuViewModel extends TaiHoSoViewModel {

    public TrangChuViewModel(@NonNull HoSoMuonRepository hoSoMuonRepository) {
        super(hoSoMuonRepository);
    }
}
