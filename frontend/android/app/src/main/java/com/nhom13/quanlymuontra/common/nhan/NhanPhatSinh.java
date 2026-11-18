package com.nhom13.quanlymuontra.common.nhan;

import androidx.annotation.NonNull;
import androidx.annotation.StringRes;
import com.nhom13.quanlymuontra.R;

/**
 * Nhãn phát sinh từ giá trị logic hoặc điều kiện so sánh, không có mã trạng thái trong API (DS
 * 7.3, FC 7.3). Dùng cùng thành phần nhãn trạng thái. Điều kiện sinh nhãn do màn hình quyết định
 * từ dữ liệu máy chủ; lớp này chỉ giữ nhãn, tông và mức nhấn.
 */
public enum NhanPhatSinh {
    /** Hết hàng. */
    HET_HANG(R.string.nhan_phat_sinh_het_hang, Tong.DANGER, false),
    /** Không đủ. */
    KHONG_DU(R.string.nhan_phat_sinh_khong_du, Tong.DANGER, false),
    /** Hợp lệ. */
    HOP_LE(R.string.nhan_phat_sinh_hop_le, Tong.SUCCESS, true),
    /** Không hợp lệ. */
    KHONG_HOP_LE(R.string.nhan_phat_sinh_khong_hop_le, Tong.DANGER, true),
    /** Được duyệt. */
    DUOC_DUYET(R.string.nhan_phat_sinh_duoc_duyet, Tong.SUCCESS, true),
    /** Không được duyệt. */
    KHONG_DUOC_DUYET(R.string.nhan_phat_sinh_khong_duoc_duyet, Tong.DANGER, true),
    /** Đã ngưng sử dụng. */
    DA_NGUNG_SU_DUNG(R.string.nhan_phat_sinh_da_ngung_su_dung, Tong.NEUTRAL, false),
    /** Đang sử dụng. */
    DANG_SU_DUNG(R.string.nhan_phat_sinh_dang_su_dung, Tong.SUCCESS, false),
    /** Đã tắt. */
    DA_TAT(R.string.nhan_phat_sinh_da_tat, Tong.NEUTRAL, false),
    /** Chưa trả. */
    CHUA_TRA(R.string.nhan_phat_sinh_chua_tra, Tong.NEUTRAL, false),
    /** Bảo trì. */
    BAO_TRI(R.string.nhan_phat_sinh_bao_tri, Tong.NEUTRAL, false),
    /** Thanh lý. */
    THANH_LY(R.string.nhan_phat_sinh_thanh_ly, Tong.NEUTRAL, false),
    /** Tạm tính. */
    TAM_TINH(R.string.nhan_phat_sinh_tam_tinh, Tong.WARNING, false);

    @StringRes private final int nhanResId;
    @NonNull private final Tong tong;
    private final boolean nhanDam;

    NhanPhatSinh(@StringRes int nhanResId, @NonNull Tong tong, boolean nhanDam) {
        this.nhanResId = nhanResId;
        this.tong = tong;
        this.nhanDam = nhanDam;
    }

    @NonNull
    public ThongTinTrangThai thongTin() {
        return ThongTinTrangThai.daBiet(nhanResId, tong, nhanDam);
    }
}
