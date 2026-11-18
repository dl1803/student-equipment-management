package com.nhom13.quanlymuontra.common.nhan;

import android.content.Context;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.StringRes;
import androidx.annotation.VisibleForTesting;
import com.nhom13.quanlymuontra.R;
import java.util.Collections;
import java.util.EnumMap;
import java.util.HashMap;
import java.util.Map;
import java.util.Set;

/**
 * Ánh xạ cặp (họ, mã API) sang nhãn, tông và mức nhấn theo DS 7.2 (FC 7.3). Nhãn chỉ sinh từ mã
 * API, không từ phép tính ở client (NT-01). Mã chưa biết hiển thị nguyên mã, tông trung tính (DS
 * 7.1); tra cứu phân biệt chữ hoa chữ thường vì mã API là chữ hoa cố định (mọi giá trị enum trong openapi.yaml).
 */
public final class NhanTrangThai {

    private static final Map<HoTrangThai, Map<String, ThongTinTrangThai>> BANG =
            new EnumMap<>(HoTrangThai.class);

    static {
        muc(HoTrangThai.TINH_TRANG_THIET_BI, "SAN_SANG", R.string.nhan_tinh_trang_thiet_bi_san_sang, Tong.SUCCESS, false);
        muc(HoTrangThai.TINH_TRANG_THIET_BI, "DANG_DUOC_MUON", R.string.nhan_tinh_trang_thiet_bi_dang_duoc_muon, Tong.INFO, false);
        muc(HoTrangThai.TINH_TRANG_THIET_BI, "DANG_BAO_TRI_SUA_CHUA", R.string.nhan_tinh_trang_thiet_bi_dang_bao_tri_sua_chua, Tong.WARNING, false);
        muc(HoTrangThai.TINH_TRANG_THIET_BI, "DA_THANH_LY", R.string.nhan_tinh_trang_thiet_bi_da_thanh_ly, Tong.NEUTRAL, false);
        muc(HoTrangThai.TINH_TRANG_THIET_BI, "DA_BAO_MAT", R.string.nhan_tinh_trang_thiet_bi_da_bao_mat, Tong.DANGER, false);
        muc(HoTrangThai.PHIEU_YEU_CAU, "CHO_DUYET", R.string.nhan_phieu_yeu_cau_cho_duyet, Tong.WARNING, false);
        muc(HoTrangThai.PHIEU_YEU_CAU, "DA_DUYET", R.string.nhan_phieu_yeu_cau_da_duyet, Tong.SUCCESS, false);
        muc(HoTrangThai.PHIEU_YEU_CAU, "TU_CHOI", R.string.nhan_phieu_yeu_cau_tu_choi, Tong.DANGER, false);
        muc(HoTrangThai.PHIEU_YEU_CAU, "DA_HUY", R.string.nhan_phieu_yeu_cau_da_huy, Tong.NEUTRAL, false);
        muc(HoTrangThai.PHIEU_YEU_CAU, "HET_HAN", R.string.nhan_phieu_yeu_cau_het_han, Tong.NEUTRAL, false);
        muc(HoTrangThai.PHIEU_YEU_CAU, "DA_NHAN", R.string.nhan_phieu_yeu_cau_da_nhan, Tong.SUCCESS, false);
        muc(HoTrangThai.PHIEU_MUON, "DANG_MUON", R.string.nhan_phieu_muon_dang_muon, Tong.INFO, false);
        muc(HoTrangThai.PHIEU_MUON, "QUA_HAN", R.string.nhan_phieu_muon_qua_han, Tong.DANGER, true);
        muc(HoTrangThai.PHIEU_MUON, "DA_TRA", R.string.nhan_phieu_muon_da_tra, Tong.SUCCESS, false);
        muc(HoTrangThai.DANG_KY_CHO, "DANG_CHO", R.string.nhan_dang_ky_cho_dang_cho, Tong.INFO, false);
        muc(HoTrangThai.DANG_KY_CHO, "DA_THONG_BAO", R.string.nhan_dang_ky_cho_da_thong_bao, Tong.SUCCESS, false);
        muc(HoTrangThai.DANG_KY_CHO, "DA_NHAN", R.string.nhan_dang_ky_cho_da_nhan, Tong.SUCCESS, false);
        muc(HoTrangThai.DANG_KY_CHO, "DA_HUY", R.string.nhan_dang_ky_cho_da_huy, Tong.NEUTRAL, false);
        muc(HoTrangThai.DANG_KY_CHO, "HET_HAN", R.string.nhan_dang_ky_cho_het_han, Tong.NEUTRAL, false);
        muc(HoTrangThai.KET_QUA_KIEM_TRA, "NGUYEN_VEN", R.string.nhan_ket_qua_kiem_tra_nguyen_ven, Tong.SUCCESS, false);
        muc(HoTrangThai.KET_QUA_KIEM_TRA, "HU_HONG", R.string.nhan_ket_qua_kiem_tra_hu_hong, Tong.DANGER, false);
        muc(HoTrangThai.KET_QUA_KIEM_TRA, "THIEU_PHU_KIEN", R.string.nhan_ket_qua_kiem_tra_thieu_phu_kien, Tong.WARNING, false);
        muc(HoTrangThai.KET_QUA_KIEM_TRA, "MAT", R.string.nhan_ket_qua_kiem_tra_mat, Tong.DANGER, true);
        muc(HoTrangThai.BIEN_BAN, "CHUA_NOP", R.string.nhan_bien_ban_chua_nop, Tong.WARNING, false);
        muc(HoTrangThai.BIEN_BAN, "DA_NOP", R.string.nhan_bien_ban_da_nop, Tong.SUCCESS, false);
        muc(HoTrangThai.BIEN_BAN, "DA_HUY", R.string.nhan_bien_ban_da_huy, Tong.NEUTRAL, false);
        muc(HoTrangThai.LOAI_VI_PHAM, "TRA_TRE", R.string.nhan_loai_vi_pham_tra_tre, Tong.WARNING, false);
        muc(HoTrangThai.LOAI_VI_PHAM, "HU_HONG", R.string.nhan_loai_vi_pham_hu_hong, Tong.DANGER, false);
        muc(HoTrangThai.LOAI_VI_PHAM, "MAT", R.string.nhan_loai_vi_pham_mat, Tong.DANGER, false);
        muc(HoTrangThai.LOAI_VI_PHAM, "KHONG_DEN_NHAN", R.string.nhan_loai_vi_pham_khong_den_nhan, Tong.WARNING, false);
        muc(HoTrangThai.TRANG_THAI_HOC, "DANG_HOC", R.string.nhan_trang_thai_hoc_dang_hoc, Tong.SUCCESS, false);
        muc(HoTrangThai.TRANG_THAI_HOC, "TOT_NGHIEP", R.string.nhan_trang_thai_hoc_tot_nghiep, Tong.NEUTRAL, false);
        muc(HoTrangThai.TRANG_THAI_HOC, "THOI_HOC", R.string.nhan_trang_thai_hoc_thoi_hoc, Tong.DANGER, false);
        muc(HoTrangThai.TRANG_THAI_HOC, "DINH_CHI", R.string.nhan_trang_thai_hoc_dinh_chi, Tong.DANGER, false);
        muc(HoTrangThai.QUYEN_MUON, "BINH_THUONG", R.string.nhan_quyen_muon_binh_thuong, Tong.SUCCESS, false);
        muc(HoTrangThai.QUYEN_MUON, "TAM_KHOA", R.string.nhan_quyen_muon_tam_khoa, Tong.WARNING, true);
        muc(HoTrangThai.QUYEN_MUON, "CAM_MUON_DAI_HAN", R.string.nhan_quyen_muon_cam_muon_dai_han, Tong.DANGER, true);
        muc(HoTrangThai.TAI_KHOAN, "HOAT_DONG", R.string.nhan_tai_khoan_hoat_dong, Tong.SUCCESS, false);
        muc(HoTrangThai.TAI_KHOAN, "TAM_KHOA_DANG_NHAP", R.string.nhan_tai_khoan_tam_khoa_dang_nhap, Tong.WARNING, false);
        muc(HoTrangThai.TAI_KHOAN, "BI_KHOA", R.string.nhan_tai_khoan_bi_khoa, Tong.DANGER, false);
        muc(HoTrangThai.LOAI_BAO_TRI, "BAO_TRI_DINH_KY", R.string.nhan_loai_bao_tri_bao_tri_dinh_ky, Tong.NEUTRAL, false);
        muc(HoTrangThai.LOAI_BAO_TRI, "SUA_CHUA", R.string.nhan_loai_bao_tri_sua_chua, Tong.NEUTRAL, false);
        muc(HoTrangThai.KET_QUA_BAO_TRI, "DUA_VE_SAN_SANG", R.string.nhan_ket_qua_bao_tri_dua_ve_san_sang, Tong.SUCCESS, false);
        muc(HoTrangThai.KET_QUA_BAO_TRI, "DE_XUAT_THANH_LY", R.string.nhan_ket_qua_bao_tri_de_xuat_thanh_ly, Tong.WARNING, false);
        muc(HoTrangThai.NGUON_GIAO, "PHIEU_YEU_CAU", R.string.nhan_nguon_giao_phieu_yeu_cau, Tong.NEUTRAL, false);
        muc(HoTrangThai.NGUON_GIAO, "DANG_KY_CHO", R.string.nhan_nguon_giao_dang_ky_cho, Tong.NEUTRAL, false);
        muc(HoTrangThai.NGUON_GIAO, "TRUC_TIEP", R.string.nhan_nguon_giao_truc_tiep, Tong.NEUTRAL, false);
        muc(HoTrangThai.LOAI_THONG_BAO, "NHAC_HAN_TRA", R.string.nhan_loai_thong_bao_nhac_han_tra, Tong.WARNING, false);
        muc(HoTrangThai.LOAI_THONG_BAO, "QUA_HAN", R.string.nhan_loai_thong_bao_qua_han, Tong.DANGER, false);
        muc(HoTrangThai.LOAI_THONG_BAO, "THIET_BI_CHO_SAN_SANG", R.string.nhan_loai_thong_bao_thiet_bi_cho_san_sang, Tong.SUCCESS, false);
        muc(HoTrangThai.LOAI_THONG_BAO, "DUYET_HOAC_TU_CHOI", R.string.nhan_loai_thong_bao_duyet_hoac_tu_choi, Tong.INFO, false);
        muc(HoTrangThai.LOAI_THONG_BAO, "VI_PHAM_PHI_PHAT", R.string.nhan_loai_thong_bao_vi_pham_phi_phat, Tong.DANGER, false);
        muc(HoTrangThai.LOAI_THONG_BAO, "NHAC_NHO_NV", R.string.nhan_loai_thong_bao_nhac_nho_nv, Tong.INFO, false);
        muc(HoTrangThai.LOAI_THONG_BAO, "NHAC_BAO_TRI", R.string.nhan_loai_thong_bao_nhac_bao_tri, Tong.NEUTRAL, false);
    }

    private NhanTrangThai() {}

    private static void muc(
            HoTrangThai ho, String ma, @StringRes int nhanResId, Tong tong, boolean nhanDam) {
        Map<String, ThongTinTrangThai> theoMa = BANG.get(ho);
        if (theoMa == null) {
            theoMa = new HashMap<>();
            BANG.put(ho, theoMa);
        }
        theoMa.put(ma, ThongTinTrangThai.daBiet(nhanResId, tong, nhanDam));
    }

    /** Cách hiển thị của mã {@code ma} trong họ {@code ho}; không bao giờ ném lỗi vì mã lạ. */
    @NonNull
    public static ThongTinTrangThai thongTin(@NonNull HoTrangThai ho, @Nullable String ma) {
        Map<String, ThongTinTrangThai> theoMa = BANG.get(ho);
        ThongTinTrangThai thongTin = (theoMa == null || ma == null) ? null : theoMa.get(ma);
        return thongTin != null ? thongTin : ThongTinTrangThai.maLa(ma);
    }

    /** Chữ nhãn để hiển thị; mã chưa biết trả nguyên mã. */
    @NonNull
    public static String layNhan(
            @NonNull Context context, @NonNull HoTrangThai ho, @Nullable String ma) {
        ThongTinTrangThai thongTin = thongTin(ho, ma);
        return thongTin.isMaLa() ? thongTin.getMaGoc() : context.getString(thongTin.getNhanResId());
    }

    /** Các mã đã có nhãn của một họ, để kiểm thử độ phủ so với hợp đồng API. */
    @VisibleForTesting
    @NonNull
    static Set<String> maDaBiet(@NonNull HoTrangThai ho) {
        Map<String, ThongTinTrangThai> theoMa = BANG.get(ho);
        return theoMa == null ? Collections.<String>emptySet() : Collections.unmodifiableSet(theoMa.keySet());
    }
}
