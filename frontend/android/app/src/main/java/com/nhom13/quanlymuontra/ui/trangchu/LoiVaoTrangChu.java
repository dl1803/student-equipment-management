package com.nhom13.quanlymuontra.ui.trangchu;

import androidx.annotation.StringRes;
import com.nhom13.quanlymuontra.R;

/**
 * Tám lối vào chức năng của SCR-SV-01 theo đúng thứ tự Wireframes (lưới hai cột): Tra cứu thiết bị, Đặt
 * trước thiết bị, Phiếu yêu cầu của tôi, Đăng ký chờ của tôi, Lịch sử mượn/trả, Biên bản và phí phạt, Vi
 * phạm, Hồ sơ mượn (UI-Screens 5.1). Chỉ có tên hiển thị; đích điều hướng do {@link TrangChuFragment}
 * quyết định vì chưa phải đích nào cũng có trong nav graph.
 */
enum LoiVaoTrangChu {
    TRA_CUU(R.string.trang_chu_loi_vao_tra_cuu),
    DAT_TRUOC(R.string.trang_chu_loi_vao_dat_truoc),
    PHIEU_YEU_CAU(R.string.trang_chu_loi_vao_phieu_yeu_cau),
    DANG_KY_CHO(R.string.trang_chu_loi_vao_dang_ky_cho),
    LICH_SU(R.string.trang_chu_loi_vao_lich_su),
    BIEN_BAN(R.string.trang_chu_loi_vao_bien_ban),
    VI_PHAM(R.string.trang_chu_loi_vao_vi_pham),
    HO_SO(R.string.trang_chu_loi_vao_ho_so);

    @StringRes private final int nhanResId;

    LoiVaoTrangChu(@StringRes int nhanResId) {
        this.nhanResId = nhanResId;
    }

    @StringRes
    int getNhanResId() {
        return nhanResId;
    }
}
