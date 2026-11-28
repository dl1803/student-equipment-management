package com.nhom13.quanlymuontra.ui.phieuyeucau;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.StringRes;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.data.model.ApiError;

/**
 * Phân loại và trình bày lỗi khi gửi phiếu yêu cầu (UI-Screens 5.5, FA 3.3, FA 3.4). Hàm thuần, phân nhánh theo {@code
 * maLoi} (và loại lỗi), không theo nội dung {@code detail}. {@code detail} chỉ được lấy để hiển thị ({@link
 * #chiTietThoiGian}).
 */
public final class TrinhBayLoiGuiPhieu {

    /** Nhóm xử lý tương ứng bảng "Xử lý kết quả gửi" của UI-Screens 5.5. */
    public enum PhanLoai {
        DU_LIEU_KHONG_HOP_LE,
        VUOT_GIOI_HAN_THIET_BI,
        VUOT_THOI_GIAN_MUON_TOI_DA,
        SO_LUONG_THAY_DOI,
        THE_HET_HAN,
        KHONG_CON_THEO_HOC,
        BI_KHOA_QUYEN_MUON,
        /** Mất kết nối, quá thời gian chờ, hủy: không biết máy chủ đã tạo phiếu hay chưa. */
        CHUA_XAC_DINH,
        QUA_NHIEU_YEU_CAU,
        HE_THONG
    }

    private static final String MA_DU_LIEU_KHONG_HOP_LE = "DU_LIEU_KHONG_HOP_LE";
    private static final String MA_VUOT_GIOI_HAN_THIET_BI = "VUOT_GIOI_HAN_THIET_BI";
    private static final String MA_VUOT_THOI_GIAN_MUON_TOI_DA = "VUOT_THOI_GIAN_MUON_TOI_DA";
    private static final String MA_SO_LUONG_THAY_DOI = "SO_LUONG_KHA_DUNG_THAY_DOI";
    private static final String MA_THE_HET_HAN = "SV_THE_HET_HAN";
    private static final String MA_KHONG_CON_THEO_HOC = "SV_KHONG_CON_THEO_HOC";
    private static final String MA_BI_KHOA_QUYEN_MUON = "SV_BI_KHOA_QUYEN_MUON";
    private static final String MA_QUA_NHIEU_YEU_CAU = "QUA_NHIEU_YEU_CAU";
    private static final int STATUS_DU_LIEU_KHONG_HOP_LE = 400;
    private static final int STATUS_QUA_NHIEU_YEU_CAU = 429;

    private TrinhBayLoiGuiPhieu() {}

    @NonNull
    public static PhanLoai phanLoai(@NonNull ApiError loi) {
        if (loi.getLoai() != ApiError.Loai.HTTP) {
            return PhanLoai.CHUA_XAC_DINH;
        }
        String maLoi = loi.getMaLoi();
        if (maLoi != null) {
            switch (maLoi) {
                case MA_DU_LIEU_KHONG_HOP_LE:
                    return PhanLoai.DU_LIEU_KHONG_HOP_LE;
                case MA_VUOT_GIOI_HAN_THIET_BI:
                    return PhanLoai.VUOT_GIOI_HAN_THIET_BI;
                case MA_VUOT_THOI_GIAN_MUON_TOI_DA:
                    return PhanLoai.VUOT_THOI_GIAN_MUON_TOI_DA;
                case MA_SO_LUONG_THAY_DOI:
                    return PhanLoai.SO_LUONG_THAY_DOI;
                case MA_THE_HET_HAN:
                    return PhanLoai.THE_HET_HAN;
                case MA_KHONG_CON_THEO_HOC:
                    return PhanLoai.KHONG_CON_THEO_HOC;
                case MA_BI_KHOA_QUYEN_MUON:
                    return PhanLoai.BI_KHOA_QUYEN_MUON;
                case MA_QUA_NHIEU_YEU_CAU:
                    return PhanLoai.QUA_NHIEU_YEU_CAU;
                default:
                    break;
            }
        }
        Integer status = loi.getStatus();
        if (status != null && status == STATUS_DU_LIEU_KHONG_HOP_LE) {
            return PhanLoai.DU_LIEU_KHONG_HOP_LE;
        }
        if (status != null && status == STATUS_QUA_NHIEU_YEU_CAU) {
            return PhanLoai.QUA_NHIEU_YEU_CAU;
        }
        return PhanLoai.HE_THONG;
    }

    /** {@code true} khi lỗi đưa sinh viên về SCR-SV-04 để sửa; {@code false} thì ở lại SCR-SV-05. */
    public static boolean quayVeSua(@NonNull PhanLoai phanLoai) {
        switch (phanLoai) {
            case DU_LIEU_KHONG_HOP_LE:
            case VUOT_GIOI_HAN_THIET_BI:
            case VUOT_THOI_GIAN_MUON_TOI_DA:
            case SO_LUONG_THAY_DOI:
                return true;
            default:
                return false;
        }
    }

    /** Thông điệp cố định của từng nhóm lỗi. */
    @StringRes
    public static int thongDiep(@NonNull PhanLoai phanLoai) {
        switch (phanLoai) {
            case DU_LIEU_KHONG_HOP_LE:
                return R.string.gui_du_lieu_khong_hop_le;
            case VUOT_GIOI_HAN_THIET_BI:
                return R.string.gui_vuot_gioi_han;
            case VUOT_THOI_GIAN_MUON_TOI_DA:
                return R.string.gui_vuot_thoi_gian;
            case SO_LUONG_THAY_DOI:
                return R.string.gui_so_luong_thay_doi;
            case THE_HET_HAN:
                return R.string.gui_the_het_han;
            case KHONG_CON_THEO_HOC:
                return R.string.gui_khong_con_theo_hoc;
            case BI_KHOA_QUYEN_MUON:
                return R.string.gui_bi_khoa;
            case CHUA_XAC_DINH:
                return R.string.gui_chua_xac_dinh_noi_dung;
            case QUA_NHIEU_YEU_CAU:
                return R.string.qua_nhieu_yeu_cau;
            case HE_THONG:
            default:
                return R.string.loi_he_thong;
        }
    }

    /**
     * Nội dung {@code detail} (mức tối đa cho phép) chỉ để hiển thị cạnh ô thời gian; {@code null} khi máy chủ không
     * trả (API không có trường riêng cho mức tối đa, UI-Screens 5.5).
     */
    @Nullable
    public static String chiTietThoiGian(@Nullable ApiError loi) {
        if (loi == null) {
            return null;
        }
        String chiTiet = loi.getDetail();
        if (chiTiet == null) {
            return null;
        }
        String gon = chiTiet.trim();
        return gon.isEmpty() ? null : gon;
    }
}
