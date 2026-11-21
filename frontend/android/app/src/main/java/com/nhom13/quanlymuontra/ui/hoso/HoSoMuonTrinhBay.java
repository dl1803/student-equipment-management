package com.nhom13.quanlymuontra.ui.hoso;

import androidx.annotation.Nullable;
import androidx.annotation.StringRes;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.nhan.QuyenMuon;

/**
 * Quy tắc chọn chữ của SCR-SV-14 (UI-Screens 5.14, UF-SV-08). Hàm thuần, không giữ trạng thái, kiểm thử được
 * trên JVM. Chỉ chọn chữ theo mã máy chủ trả về; không tự đánh giá điều kiện mượn (FA-07).
 *
 * <p>Hai hằng số {@code HANH_DONG_*} là giá trị của tham số {@code hanhDongDaChon} mà SCR-SV-03, 04 truyền khi
 * mở hồ sơ ở chế độ bị chặn (FA 5.3 quy tắc 3); D09-L và D10-L dùng lại.
 */
public final class HoSoMuonTrinhBay {

    /** Hành động vừa chọn là Đặt trước. */
    public static final String HANH_DONG_DAT_TRUOC = "DAT_TRUOC";

    /** Hành động vừa chọn là Đăng ký chờ. */
    public static final String HANH_DONG_DANG_KY_CHO = "DANG_KY_CHO";

    private HoSoMuonTrinhBay() {}

    /**
     * Hướng dẫn mở lại theo trạng thái quyền mượn (UI-Screens 5.14).
     *
     * @return mã chuỗi, hoặc {@code 0} khi mã quyền mượn chưa biết (không có hướng dẫn để hiển thị)
     */
    @StringRes
    public static int huongDan(@Nullable String quyenMuon) {
        if (quyenMuon == null) {
            return 0;
        }
        switch (quyenMuon) {
            case QuyenMuon.BINH_THUONG:
                return R.string.ho_so_huong_dan_binh_thuong;
            case QuyenMuon.TAM_KHOA:
                return R.string.ho_so_huong_dan_tam_khoa;
            case QuyenMuon.CAM_MUON_DAI_HAN:
                return R.string.ho_so_huong_dan_cam_muon_dai_han;
            default:
                return 0;
        }
    }

    /** Tiêu đề khối thông báo của chế độ bị chặn: nêu hành động vừa chọn không thực hiện được. */
    @StringRes
    public static int tieuDeBiChan(@Nullable String hanhDongDaChon) {
        if (HANH_DONG_DAT_TRUOC.equals(hanhDongDaChon)) {
            return R.string.ho_so_bi_chan_dat_truoc;
        }
        if (HANH_DONG_DANG_KY_CHO.equals(hanhDongDaChon)) {
            return R.string.ho_so_bi_chan_dang_ky_cho;
        }
        return R.string.ho_so_bi_chan_chung;
    }
}
